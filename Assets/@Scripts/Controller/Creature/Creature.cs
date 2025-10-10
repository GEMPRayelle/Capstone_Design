using Data;
using Spine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Define;

public class Creature : BaseObject
{
    public BaseObject Target { get; protected set; } //찾아가려는 타겟
    public SkillComponent Skills { get; protected set; } //현재 가지고있는 스킬

    public Data.CreatureData CreatureData { get; private set; }
    public OverlayTile currentStandingTile;//현재 서 있는 타일 정보
    public EffectComponent Effects { get; set; }//이펙트(상태 이상효과) 목록
    public bool IsMoved = false; // 현재 턴에 이동했는지에 대한 정보

    //Event
    public GameEventGameObjectList moveAlongPath;
    public GameEvent endTurn;

    private List<OverlayTile> path; //현재 계산된 최적의 경로
    private Senario bestSenario; //현재 턴에서 실행할 최적의 시나리오
    private PathFinder _pathFinder { get; set; } //A* 기반 경로 탐색

    protected ECreatureState _creatureState = ECreatureState.None;
    public virtual ECreatureState CreatureState
    {
        get { return _creatureState; }
        set
        {
            //Creature의 State가 바뀔때마다 UpdateAnimation을 자동으로 호출하여 애니메이션이 재생되도록함
            if (_creatureState != value)
            {
                _creatureState = value;
                UpdateAnimation();
            }
        }
    }

    public bool IsAlive { get { return _creatureState != ECreatureState.Dead; } }

    protected float AttackDistance
    {
        get
        {
            float env = 2.2f;
            if (Target != null && Target.ObjectType == EObjectType.Env)
                return Mathf.Max(env, Collider.radius + Target.Collider.radius + 0.1f);

            float baseValue = CreatureData.AtkRange;
            return baseValue;
        }
    }

    private float DistToTargetSqr
    {
        get
        {
            Vector3 dir = (Target.transform.position - transform.position);
            float distToTarget = Math.Max(0, dir.magnitude - Target.ExtraCells * 1f - ExtraCells * 1f); // TEMP
            return distToTarget * distToTarget;
        }
    }

    #region Stat Property
    public float Hp { get; set; }
    public float Speed { get; protected set; } = 1.0f;
    public CreatureStat MaxHp;
    public CreatureStat Atk;
    public CreatureStat CriRate;
    public CreatureStat CriDamage;
    public CreatureStat ReduceDamageRate;
    public CreatureStat LifeStealRate;
    public CreatureStat ThornsDamageRate; //쏜즈
    public CreatureStat MoveSpeed;
    public CreatureStat AttackSpeedRate;
    public int MovementRange = 3; // 캐릭터의 이동 가능 범위 (타일 수 기준)
    public int SkillRange = 3;
    #endregion

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        return true;
    }

    //001_SkeletonData -> Knight
    public virtual void SetInfo(int templateID)
    {
        DataTemplateID = templateID;

        //데이터를 긁어오지 못했을경우 예외처리없이 크래시처리
        if (ObjectType == EObjectType.Player)
            CreatureData = Managers.Data.HeroDic[templateID];
        else
            CreatureData = Managers.Data.MonsterDic[templateID];

        //Name
        gameObject.name = $"{CreatureData.DataId}_{CreatureData.DescriptionTextID}";

        //Collider
        Collider.offset = new Vector2(CreatureData.ColliderOffsetX, CreatureData.ColliderOffsetY);
        Collider.radius = CreatureData.ColliderRadius;

        //Spine
        SetSpineAnimation(CreatureData.SkeletonDataID, SortingLayers.CREATURE);

        //Skills
        Skills = gameObject.GetOrAddComponent<SkillComponent>();
        Skills.SetInfo(this, CreatureData);
        //Stat
        Hp = CreatureData.MaxHp;
        MaxHp = new CreatureStat(CreatureData.MaxHp);
        Atk = new CreatureStat(CreatureData.Atk);
        CriRate = new CreatureStat(CreatureData.CriRate);
        CriDamage = new CreatureStat(CreatureData.CriDamage);
        ReduceDamageRate = new CreatureStat(0);
        LifeStealRate = new CreatureStat(0);
        ThornsDamageRate = new CreatureStat(0);
        MoveSpeed = new CreatureStat(CreatureData.MoveSpeed);
        AttackSpeedRate = new CreatureStat(1);
        Speed = CreatureData.speed;
        MovementRange = CreatureData.MovementRange;
        SkillRange = CreatureData.SkillRange;

        //A*
        _pathFinder = new PathFinder();

        //State
        CreatureState = ECreatureState.Idle;

        //Effect
        Effects = gameObject.GetOrAddComponent<EffectComponent>();
        Effects.SetInfo(this);
    }

    protected override void UpdateAnimation()
    {
        switch (CreatureState)
        {
            case ECreatureState.Idle:
                PlayAnimation(0, AnimName.IDLE, true);
                break;
            //공격 애니메이션 적용은 Skill쪽으로 이전
            case ECreatureState.Attack:
                break;
            case ECreatureState.Skill:
                break;
            case ECreatureState.Move:
                PlayAnimation(0, AnimName.MOVE, true);
                break;
            case ECreatureState.OnDamaged:
                PlayAnimation(0, AnimName.IDLE, true);
                Skills.CurrentSkill.CancelSkill();//현재 사용하는 스킬은 캔슬함
                break;
            case ECreatureState.Dead:
                PlayAnimation(0, AnimName.DEAD, true);
                RigidBody.simulated = false;
                break;
            default:
                break;
        }
    }

    #region Turn-Management
    public void StartTurn()
    {
        StartCoroutine(CalculateBestSenario());
    }

    private IEnumerator EndTurn()
    {
        yield return new WaitForSeconds(0.25f);

        //타일 색상 초기화

        //턴 종료 로그

        //턴 종료 이벤트 발생
        endTurn.Raise();
    }
    #endregion

    #region Battle
    public override void OnDamaged(BaseObject attacker, SkillBase skill)
    {
        base.OnDamaged(attacker, skill);

        if (attacker.IsValid() == false)
            return;

        //데미지를 주는건 Creature들끼리 가능
        Creature creature = attacker as Creature;
        if (creature == null)
            return;

        float finalDamage = creature.Atk.Value; //TODO - 최종 공격력 계산식 기입
        Hp = Mathf.Clamp(Hp - finalDamage, 0, MaxHp.Value); //0과 MaxHp사이에서 벗어나지 않도록함

        Managers.Object.ShowDamageFont(CenterPosition, finalDamage, transform, false);

        if (Hp <= 0)
        {
            Hp = 0;
            OnDead(attacker, skill);
            CreatureState = ECreatureState.Dead;
        }

        //TODO 스킬에 따른 Effect 적용
        if (skill.SkillData.EffectIds != null)
            Effects.GenerateEffects(skill.SkillData.EffectIds.ToArray(), EEffectSpawnType.Skill, skill);
    }

    public override void OnDead(BaseObject attacker, SkillBase skill)
    {
        base.OnDead(attacker, skill);
    }

    /// <summary>
    /// 현재 위치에서 가장 가까운 적 캐릭터를 찾는 함수
    /// </summary>
    /// <param name="position">위치 기준</param>
    /// <returns></returns>
    private BaseObject FindClosestObject(OverlayTile position)
    {
        Creature target = null;
        var closestDistance = 1000; //충분히 큰 초기값

        foreach (var player in Managers.Turn.activePlayerList)
        {
            if (player.IsAlive) //살아있는 플레이어 캐릭터만 고려
            {
                var currentDistance = _pathFinder.GetManhattenDistance(position, player.currentStandingTile);

                if (currentDistance <= closestDistance)
                {
                    closestDistance = currentDistance;
                    target = player;
                }
            }
        }

        return target;
    }

    /// <summary>
    /// 죽이기 가장 쉬운 캐릭터를 찾는 함수 (for Strategy AI)
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    private Creature FindClosestObjectToDeath(OverlayTile position)
    {
        Creature target = null;
        int lowestHealth = -1;
        var isObjectInRange = true; //범위 내에 적이 있는지 확인

        foreach (var player in Managers.Turn.activePlayerList)
        {
            if (player.IsAlive && player.currentStandingTile)
            {
                var currentDistance = _pathFinder.GetManhattenDistance(position, player.currentStandingTile);
                var currentHealth = player.Hp;

                // 공격 범위 내에 있는 경우 우선 고려
                //TODO AttackDistance는 데이터시트에 있는 값이 아님
                if (currentDistance <= player.AttackDistance &&
                    ((lowestHealth == -1) || (currentHealth <= lowestHealth || isObjectInRange)))
                {
                    lowestHealth = (int)currentHealth;
                    target = player;
                    isObjectInRange = false;
                }
                // 범위 내에 적이 없다면 전체에서 가장 약한 적 선택
                else if (isObjectInRange && ((lowestHealth == -1) || (currentHealth <= lowestHealth)))
                {
                    lowestHealth = (int)currentHealth;
                    target = player;
                }
            }
        }

        return target;
    }

    /// <summary>
    /// 대상 타일 주변에서 가장 가까운 인접 타일을 찾는 함수
    /// , 직접 그 타일로 이동이 불가능할때 사용해야함
    /// </summary>
    /// <param name="targetObjectTile"></param>
    /// <returns></returns>
    private OverlayTile GetClosestNeighbour(OverlayTile targetObjectTile)
    {
        //대상 타일의 모든 인접 타일 가져옴
        var targetNeightbour = Managers.Map.GetNeighbourTiles(targetObjectTile, new List<OverlayTile>());
        var targetTile = targetNeightbour[0];
        var targetDistance = _pathFinder.GetManhattenDistance(targetTile, currentStandingTile);

        //가장 가까운 인접 타일 탐색
        foreach (var item in targetNeightbour)
        {
            var distance = _pathFinder.GetManhattenDistance(item, currentStandingTile);

            if (distance < targetDistance)
            {
                targetTile = item;
                targetDistance = distance;
            }
        }

        return targetTile;
    }

    /// <summary>
    /// 주어진 타일들 내에 있는 모든 적 캐릭터를 찾음
    /// 주로 범위 공격 능력의 피해 대상을 찾는데 사용
    /// </summary>
    /// <param name="tiles">검색할 타일 리스트</param>
    /// <returns>해당 타일들에 있는 적 캐릭터 리스트</returns>
    private List<Creature> FindAllCharactersInTiles(List<OverlayTile> tiles)
    {
        var playersInRange = new List<Creature>();

        foreach (var tile in tiles)
        {
            // 타일에 캐릭터가 있고, 적팀이며, 살아있는지 확인
            // TODO: 아군에게 도움이 되는 능력도 고려하도록 개선 필요
            //if (tile.activeCharacter &&
            //    tile.activeCharacter.teamID != teamID &&
            //    tile.activeCharacter.isAlive)
            //{
            //    playersInRange.Add(tile.activeCharacter);
            //}
        }

        return playersInRange;
    }

    //range범위에 있는 Objs중에서 가장 가까이 있는 BaseObject를 반환
    public BaseObject FindClosetObjectInRange(float range, IEnumerable<BaseObject> objs, Func<BaseObject, bool> func = null)
    {
        BaseObject target = null;
        float bestDistanceSqr = float.MaxValue;
        float searchDistanceSqr = range * range;

        foreach (BaseObject obj in objs)
        {
            Vector3 dir = obj.transform.position - transform.position;
            float distToTargetSqr = dir.sqrMagnitude; //연산의 효율성땜에 제곱으로 계산

            // 탐색 범위보다 멀리 있으면 스킵.
            if (distToTargetSqr > searchDistanceSqr)
                continue;

            // 이미 더 좋은 후보를 찾았으면 스킵.
            if (distToTargetSqr > bestDistanceSqr)
                continue;

            // 추가 조건
            if (func != null && func.Invoke(obj) == false)
                continue;

            target = obj;
            bestDistanceSqr = distToTargetSqr;
        }

        return target;
    }

    public List<SkillBase> GetUsableSkillList()
    {
        List<SkillBase> canUsableSkillList = new List<SkillBase>();
        foreach (SkillBase skill in Skills.SkillList)
        {
            //if (skill.IsReadytoUse)
            //{
            //    canUsableSkillList.Add(skill);
            //}
        }

        canUsableSkillList.Add(Skills.DefaultSkill);
        return canUsableSkillList;
    }
    #endregion

    #region AI
    public float UpdateAITick { get; protected set; } = 0.0f;

    protected IEnumerator CoUpdateAI()
    {
        while (true)
        {
            switch (CreatureState)
            {
                case ECreatureState.Idle:
                    UpdateIdle();
                    break;
                case ECreatureState.Move:
                    UpdateMove();
                    break;
                case ECreatureState.Attack:
                    UpdateAttack();
                    break;
                case ECreatureState.Skill:
                    UpdateSkill();
                    break;
                case ECreatureState.OnDamaged:
                    UpdateOnDamaged();
                    break;
                case ECreatureState.Dead:
                    UpdateDead();
                    break;
            }

            if (UpdateAITick > 0)
                yield return new WaitForSeconds(UpdateAITick);
            else
                yield return null;
        }

    }

    protected virtual void UpdateIdle() { }
    protected virtual void UpdateMove() { }
    protected virtual void UpdateAttack() { }

    protected virtual void UpdateSkill()
    {
        if (_coWait != null)
            return;

        //TODO -> 타입검사에 자기 자신을 못때리도록 수정해야함
        if (Target.IsValid() == false || Target.ObjectType == EObjectType.None)
        {
            CreatureState = ECreatureState.Idle;
            return;
        }

        //예외처리
        float distToTargetSqr = DistToTargetSqr;
        float attackDistanceSqr = AttackDistance * AttackDistance;
        if (distToTargetSqr > attackDistanceSqr)
        {
            CreatureState = ECreatureState.Idle;
            return;
        }

        //Debugging
        if (Skills.CurrentSkill == null)
        {
            Debug.LogWarning($"{name}의 CurrentSkill이 null입니다.");
        }

        //현재 사용할 수 있는 스킬을 사용
        Skills.CurrentSkill.DoSkill();

        //스킬을 쓰는 상대를 보도록함
        LookAtTarget(Target);

        var trackEntry = SkeletonAnim.state.GetCurrent(0);
        float animationDuration = trackEntry.Animation.Duration;
        float timeScale = trackEntry.TimeScale;
        float actualDuration = animationDuration / timeScale;

        Debug.Log($"Animation Duration: {animationDuration}, TimeScale: {timeScale}, Actual Duration: {actualDuration}");

        StartWait(actualDuration);
    }
    protected virtual void UpdateOnDamaged() { }
    protected virtual void UpdateDead() { }

    // 이동 시스템에게 경로를 따라 이동하도록 전달
    private IEnumerator Move(List<OverlayTile> path)
    {
        //TODO 목표 이동을 시각적으로 강조할지?

        yield return new WaitForSeconds(0.25f);

        //이동 이벤트 발생 (GameObject 리스트로 변환)
        moveAlongPath.Raise(path.Select(x => x.gameObject).ToList());

    }

    private IEnumerator CalculateBestSenario()
    {
        var tileInMovementRange = Managers.Map.GetTilesInRange(currentStandingTile, MovementRange);

        //이동가능한 범위를 시작적으로 표현할지?

        var senario = new Senario();

        foreach (var tile in tileInMovementRange)
        {
            if (!tile.isBlocked) // 막혀있지 않은 타일만 고려
            {
                // 해당 타일에서의 행동 시나리오 생성
                var tempSenario = CreateTileSenarioValue(tile);

                // 타일 효과 (용암, 독 등)를 시나리오 점수에 반영
                ApplyTileEffectsToSenarioValue(tile, tempSenario);

                // 현재 최고 시나리오와 비교
                senario = CompareSenarios(senario, tempSenario);

                // 점수가 같은 경우 더 가까운 경로 선택
                senario = CheckIfSenarioValuesAreEqual(tileInMovementRange, senario, tempSenario);

                // 공격 대상이 없는 경우의 처리
                senario = CheckSenarioValueIfNoTarget(senario, tile, tempSenario);
            }
        }

        // 최적 시나리오가 결정되면 실행, 그렇지 않으면 턴 종료
        if (senario.positionTile)
        {
            ApplyBestSenario(senario);
        }
        else
        {
            StartCoroutine(EndTurn());
        }

        yield return null;
    }

    //최적의 시나리오를 시행하는 함수
    private void ApplyBestSenario(Senario senario)
    {
        bestSenario = senario;
        var currentTile = currentStandingTile;

        //현재 위치에서 목표 위치까지의 경로 계산
        path = _pathFinder.FindPath(currentTile, bestSenario.positionTile, new List<OverlayTile>());

        //이동할 필요가 없고 공격 가능한 경우 즉시 공격
        if (path.Count == 0 && bestSenario.targetTile != null)
        {
            CreatureState = ECreatureState.Skill;
        }
        else
        {
            //이동이 필요한 경우 이동 시작
            StartCoroutine(Move(path));
        }
    }

    /// <summary>
    /// 타일 효과(용암, 독 등)를 시나리오 점수에 반영
    /// 부정적인 효과일수록 점수를 깎아서 해당 타일을 피하도록 함
    /// </summary>
    /// <param name="tile">평가할 타일</param>
    /// <param name="tempSenario">수정할 시나리오</param>
    private void ApplyTileEffectsToSenarioValue(OverlayTile tile, Senario tempSenario)
    {
        //TODO 구현이 필요함
    }

    /// <summary>
    /// 두 시나리오를 비교하여 더 좋은 시나리오를 반환
    /// </summary>
    /// <param name="senario">현재 최고 시나리오</param>
    /// <param name="tempSenario">비교할 새 시나리오</param>
    /// <returns>더 좋은 시나리오</returns>
    private static Senario CompareSenarios(Senario senario, Senario tempSenario)
    {
        if ((tempSenario != null && tempSenario.senarioValue > senario.senarioValue))
        {
            senario = tempSenario;
        }

        return senario;
    }

    /// <summary>
    /// 점수가 동일한 시나리오들 중에서 더 가까운 경로를 선택
    /// 동점일 때는 이동 거리가 짧은 것을 선호
    /// </summary>
    /// <param name="tileInMovementRange">이동 가능 범위</param>
    /// <param name="senario">현재 시나리오</param>
    /// <param name="tempSenario">비교할 시나리오</param>
    /// <returns>더 효율적인 시나리오</returns>
    private Senario CheckIfSenarioValuesAreEqual(List<OverlayTile> tileInMovementRange, Senario senario, Senario tempSenario)
    {
        if (tempSenario.positionTile != null && tempSenario.senarioValue == senario.senarioValue)
        {
            // 각 시나리오의 경로 길이 계산
            var tempSenarioPathCount = _pathFinder.FindPath(currentStandingTile, tempSenario.positionTile, tileInMovementRange).Count;
            var senarioPathCount = _pathFinder.FindPath(currentStandingTile, senario.positionTile, tileInMovementRange).Count;

            // 더 짧은 경로를 선택
            if (tempSenarioPathCount < senarioPathCount)
                senario = tempSenario;
        }

        return senario;
    }

    /// <summary>
    /// 공격 대상이 없는 경우의 시나리오 처리
    /// 가장 약한 적에게 다가가는 것을 목표로 함
    /// </summary>
    /// <param name="senario">현재 최고 시나리오</param>
    /// <param name="tile">평가 중인 타일</param>
    /// <param name="tempSenario">현재 타일의 시나리오</param>
    /// <returns>업데이트된 시나리오</returns>
    private Senario CheckSenarioValueIfNoTarget(Senario senario, OverlayTile tile, Senario tempSenario)
    {
        if (tempSenario.positionTile == null && !senario.targetTile)
        {
            // 가장 약한 적 찾기
            var targetCharacter = FindClosestObjectToDeath(tile);
            if (targetCharacter)
            {
                // 그 적 근처의 가장 가까운 타일 찾기
                var targetTile = GetClosestNeighbour(targetCharacter.currentStandingTile);

                if (targetCharacter && targetTile)
                {
                    // 해당 적까지의 경로와 거리 계산
                    var pathToCharacter = _pathFinder.FindPath(tile, targetTile, new List<OverlayTile>());
                    var distance = pathToCharacter.Count;

                    // 거리와 적 체력을 고려한 점수 계산 (음수: 가까울수록, 약할수록 좋음)
                    var senarioValue = -distance - targetCharacter.Hp;

                    //TODO : AttackDistance를 다른걸로 치환해야함
                    if (distance >= AttackDistance)
                    {
                        // 타일 효과 고려
                        //if (tile.tileData && tile.tileData.effect)
                        //{
                        //    var tileEffectValue = GetEffectsSenarioValue(
                        //        new List<ScriptableEffect>() { tile.tileData.effect },
                        //        new List<Entity>() { this });
                        //    senarioValue -= tileEffectValue;
                        //}

                        // 유효한 이동 타일이고 더 좋은 점수라면 업데이트
                        if (tile.grid2DLocation != currentStandingTile.grid2DLocation &&
                            tile.grid2DLocation != targetCharacter.currentStandingTile.grid2DLocation &&
                            (senarioValue > senario.senarioValue || !senario.positionTile))
                            senario = new Senario(senarioValue, null, null, tile, false);
                    }
                }
            }
        }

        return senario;
    }
    #endregion

    #region Ability
    private Senario CreateTileSenarioValue(OverlayTile overlayTile)
    {
        // 기본 공격 시나리오 생성 (성격에 따라)
        var attackSenario = new Senario();
        //var attackSenario = AutoAttackBasedOnPersonality(overlayTile);

        // 사용 가능한 모든 능력 검토
        //foreach (var abilityContainer in abilitiesForUse)
        //{
        //    // 마나와 쿨다운 체크
        //    if (GetStat(Stats.CurrentMana).statValue >= abilityContainer.ability.cost &&
        //        abilityContainer.turnsSinceUsed >= abilityContainer.ability.cooldown)
        //    {
        //        // 능력 시나리오 생성
        //        var tempSenario = CreateAbilitySenario(abilityContainer, overlayTile);

        //        // 더 좋은 시나리오라면 교체
        //        if (tempSenario.senarioValue > attackSenario.senarioValue)
        //            attackSenario = tempSenario;
        //    }
        //}

        return attackSenario;
    }
    #endregion

    #region Wait
    protected Coroutine _coWait;
    protected void StartWait(float seconds)
    {
        CancleWait();
        _coWait = StartCoroutine(CoWait(seconds));
    }

    IEnumerator CoWait(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        _coWait = null;
    }

    protected void CancleWait()
    {
        if (_coWait != null)
            StopCoroutine(_coWait);
        _coWait = null;
    }
    #endregion
}
