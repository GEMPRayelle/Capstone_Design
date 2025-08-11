using Data;
using Spine.Unity;
using System.Net.Http.Headers;
using UnityEngine;
using UnityEngine.Rendering;
using static Define;

public class Monster : Creature
{
    public Data.MonsterData MonsterData { get { return (Data.MonsterData)CreatureData; } }
    public Rigidbody2D target;
    public Rigidbody2D rigid;
    
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
        CreatureState = ECreatureState.Idle;

        Speed = 2.5f;
        rigid = GetComponent<Rigidbody2D>();
        SkeletonAnimation skeletonAnim = GetComponent<SkeletonAnimation>();
        skeletonAnim.GetComponent<MeshRenderer>().sortingOrder = SortingLayers.MONSTER;
        Collider.radius = 0.25f;
        Collider.offset = transform.InverseTransformPoint(transform.position + (Vector3.right * 0.5f + Vector3.up) * ColliderRadius);
        GameObject player = GameObject.Find("@Players");

        if (player != null)
            target = player.transform.GetChild(0).GetComponent<Rigidbody2D>();

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
        
    }

    protected override void UpdateMove()
    {
        // 플레이어 따라가기(Test용)
        Vector2 dirVec = target.position - rigid.position;
        Vector2 nextVec = dirVec.normalized * Speed * Time.deltaTime;

        rigid.MovePosition(rigid.position + nextVec);
        rigid.linearVelocity = Vector2.zero;

        if (dirVec.x < 0.1)
            LookLeft = true;
        else if (dirVec.x > 0.1)
            LookLeft = false;
        //else
        //    ;

    }

    protected override void UpdateSkill()
    {
        base.UpdateSkill();

        if (Target.IsValid() == false)
        {
            Target = null;
            CreatureState = ECreatureState.Move;
            return;
        }
    }

    protected override void UpdateDead()
    {

    }

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

            itemHolder.SetInfo(0, rewardData.ItemTemplateId, dropPos);   
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
