using Data;
using UnityEngine;
using static Define;

public class Monster : Creature
{
    public Data.MonsterData MonsterData { get { return (Data.MonsterData)CreatureData; } }

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

        ObjectType = EObjectType.Monster;

        StartCoroutine(CoUpdateAI());

        return true;
    }

    public override void SetInfo(int templateID)
    {
        base.SetInfo(templateID);

        CreatureState = ECreatureState.Idle;
    }

    #region AI
    protected override void UpdateIdle()
    {

    }

    protected override void UpdateMove()
    {
        
    }

    protected override void UpdateAttack()
    {
        
    }

    protected override void UpdateSkill()
    {
        base.UpdateSkill();
    }

    protected override void UpdateDead() { }
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

        //int dropItemId = MonsterData.DropItemId;

        ////TODO Item Drop 데이터 연동을 통해 정리
        //RewardData rewardData = GetRandomReward();
        //if (rewardData != null)
        //{
        //    var itemHolder = Managers.Object.Spawn<ItemHolder>(transform.position, dropItemId);

        //    #region 포물선
        //    Vector2 ran = new Vector2(transform.position.x + Random.Range(-10, -15) * 0.1f, transform.position.y);
        //    Vector2 ran2 = new Vector2(transform.position.x + Random.Range(10, 15) * 0.1f, transform.position.y);
        //    Vector2 dropPos = Random.value < 0.5 ? ran : ran2;
        //    #endregion

        //    itemHolder.SetInfo(dropItemId, rewardData.ItemTemplateId, dropPos);
        //}
        Managers.Turn.activeMonsterList.Remove(this);
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
