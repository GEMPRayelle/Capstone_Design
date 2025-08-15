using Data;
using Spine.Unity;
using System.Collections.Generic;
using System.Net.Http.Headers;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Rendering;
using static Define;

public class Monster : Creature
{
    public Data.MonsterData MonsterData { get { return (Data.MonsterData)CreatureData; } }
    public Rigidbody2D rigid;
    private float nearestDistanceSqr;


    public override ECreatureState CreatureState
    {
        get { return base.CreatureState; }
        set
        {
            if (_creatureState != value)
            {
                base.CreatureState = value;
                switch (value)
                {
                    case ECreatureState.Idle:
                        UpdateAITick = 0.5f;
                        break;
                    case ECreatureState.Attack:
                        UpdateAITick = 0.02f;
                        break;
                    case ECreatureState.Move:
                        UpdateAITick = 0.02f;
                        break;
                    case ECreatureState.Dead:
                        UpdateAITick = 1.0f;
                        break;

                }
            }
        }
    }

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        CreatureType = ECreatureType.Monster;

        Speed = 2.5f;
        rigid = GetComponent<Rigidbody2D>();
        GameObject player = GameObject.Find("@Players");

        if (player != null)
            Target = player.transform.GetChild(0).GetComponent<Player>() as BaseObject;

        StartCoroutine(CoUpdateAI());

        return true;
    }

    public override void SetInfo(int templateID)
    {
        base.SetInfo(templateID);

        CreatureState = ECreatureState.Move;
    }

    public void Update()
    {

    }

    #region AI
    protected override void UpdateIdle()
    {
        // idle은 플레이어가 마스터일떼만 적용
    }

    protected override void UpdateMove()
    {
        if (DetectPlayer(MONSTER_SEARCH_DISTANCE, Target) == true) // 실제 근처 플레이어 체크 함수
        {
            // 적 발견 시 공격 상태로 전환
            CreatureState = ECreatureState.Skill;

        }

        else // 근처에 적이 없다면
        {
            Vector2 dirVec = (Vector2)Target.transform.position - rigid.position;
            Vector2 nextVec = dirVec.normalized * Speed * Time.deltaTime;

            rigid.MovePosition(rigid.position + nextVec);
            rigid.linearVelocity = Vector2.zero;

            if (dirVec.x < 0.1)
                LookLeft = true;
            else if (dirVec.x > 0.1)
                LookLeft = false;
        };

    }

    protected override void UpdateAttack()
    {
        base.UpdateAttack();

        if (DetectPlayer(MONSTER_SEARCH_DISTANCE, Target) == false)
        {
            CreatureState = ECreatureState.Move;
        }
    }

    protected override void UpdateSkill()
    {
        base.UpdateSkill();

        if (DetectPlayer(MONSTER_SEARCH_DISTANCE, Target) == false)
        {
            //근처에 플레이어가 없으면 원상태로 복귀
            CreatureState = ECreatureState.Move;
            return;
        }
        //그게 아니라면 스킬 상태를 유지
    }

    protected override void UpdateDead() { }

    protected override void ChangedMaster() // 서번트->마스터 변경 시 로직
    {
        base.ChangedMaster(); // Creature가 해야되는 공통 로직 호출
        CreatureState = ECreatureState.Idle;
    }

    protected override void ChangedServent() // 마스터->서번트 변경 시 로직
    {
        base.ChangedServent(); // Creature가 해야되는 공통 로직 호출
        CreatureState = ECreatureState.Move;
    }
    #endregion

    #region Battle
    // 근처에 플레이어 있다면 true 아니면 false 리턴
    private bool DetectPlayer(float detectionRadius, BaseObject target)
    {
        if (target.IsValid() == false)
            return false;

        float distanceSqr = (Target.transform.position - transform.position).sqrMagnitude; // 거리 계산

        if (distanceSqr >= detectionRadius) // 거리가 일정 범위 이상이면 무시
            return false;


        return true;

    }

    public override void OnDamaged(BaseObject attacker, SkillBase skill)
    {
        base.OnDamaged(attacker, skill);
        Debug.Log($"OnDamaged: {attacker.name}");
    }

    public override void OnDead(BaseObject attacker, SkillBase skill)
    {
        base.OnDead(attacker, skill);

        int dropItemId = MonsterData.DropItemId;

        //TODO Item Drop 데이터 연동을 통해 정리
        RewardData rewardData = GetRandomReward();
        if (rewardData != null)
        {
            var itemHolder = Managers.Object.Spawn<ItemHolder>(transform.position, dropItemId);

            #region 포물선
            Vector2 ran = new Vector2(transform.position.x + Random.Range(-10, -15) * 0.1f, transform.position.y);
            Vector2 ran2 = new Vector2(transform.position.x + Random.Range(10, 15) * 0.1f, transform.position.y);
            Vector2 dropPos = Random.value < 0.5 ? ran : ran2;
            #endregion

            itemHolder.SetInfo(dropItemId, rewardData.ItemTemplateId, dropPos);
        }

        Managers.Object.Despawn(this);
    }
    #endregion

    RewardData GetRandomReward()
    {
        if (MonsterData == null)
            return null;

        if (Managers.Data.DropTableDic.TryGetValue(MonsterData.DropItemId, out DropTableData dropTableData) == false)
            return null;

        if (dropTableData.Rewards.Count <= 0)
            return null;

        int sum = 0;

        //100분율 확률
        int randomValue = UnityEngine.Random.Range(0, 100);

        foreach (RewardData item in dropTableData.Rewards)
        {
            sum += item.Probability;

            if (randomValue <= sum)
                return item;
        }

        return null;
    }
}
