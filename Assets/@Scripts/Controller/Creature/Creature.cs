using Data;
using Spine;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static Define;

public class Creature : BaseObject
{
    public BaseObject Target { get; protected set; } //찾아가려는 타겟
    public SkillComponent Skills { get; protected set; } //현재 가지고있는 스킬
    public float Speed { get; protected set; } = 1.0f;
    public ECreatureType CreatureType { get; protected set; } = ECreatureType.None;
    public Data.CreatureData CreatureData { get; private set; }
    public OverlayTile currentStandingTile;//현재 서 있는 타일 정보
    public EffectComponent Effects { get; set; }//이펙트(상태 이상효과) 목록
    protected EPlayerState activePlayerState { get; private set; } = EPlayerState.None; // 현재 활성화된 플레이어 스테이트(마스터, 서번트)에 대한 정보. 가져오기만 하면 됨

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
    public CreatureStat MaxHp;
    public CreatureStat Atk;
    public CreatureStat CriRate;
    public CreatureStat CriDamage;
    public CreatureStat ReduceDamageRate;
    public CreatureStat LifeStealRate;
    public CreatureStat ThornsDamageRate; //쏜즈
    public CreatureStat MoveSpeed;
    public CreatureStat AttackSpeedRate;
    #endregion

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        ObjectType = EObjectType.Creature;
        CreatureState = ECreatureState.Idle;

        Managers.Game.OnPlayerStateChanged -= HandleOnPlayerStateChanged;
        Managers.Game.OnPlayerStateChanged += HandleOnPlayerStateChanged;

        return true;
    }

    //001_SkeletonData -> Knight
    public virtual void SetInfo(int templateID)
    {
        DataTemplateID = templateID;

        //데이터를 긁어오지 못했을경우 예외처리없이 크래시처리
        if (CreatureType == ECreatureType.Player)
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

    // PlayerState 변환 시 작동하는 함수
    private void HandleOnPlayerStateChanged(EPlayerState playerstate) 
    {
        switch (playerstate)
        {
            case EPlayerState.None:
                break;
            case EPlayerState.Master:
                activePlayerState = EPlayerState.Master;
                ChangedMaster();
                break;
            case EPlayerState.Servant:
                activePlayerState = EPlayerState.Servant;
                ChangedServent();
                break;
        }
    }

    protected virtual void ChangedMaster() // 마스터로 변경됬을 때 공통 로직 구현
    {

    }

    protected virtual void ChangedServent() // 서번트로 변경 됬을 때 공통 로직 구현
    {

    }

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
    protected virtual void UpdateAttack() 
    {

    }

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
