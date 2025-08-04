using UnityEngine;

//기본공격
public class NormalAttack : SkillBase
{
    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        return true;
    }

    public override void SetInfo(Creature owner, int skillTemplateID)
    {
        base.SetInfo(owner, skillTemplateID);
    }

    public override void DoSkill()
    {
        base.DoSkill();
        
        //TODO
        //스킬을 시전할때 할 작업들

        //CreatureState에서 UpdateAnimation에서 처리하던 스킬을 여기서 처리
        Owner.CreatureState = Define.ECreatureState.Skill;
        Owner.PlayAnimation(0, SkillData.AnimName, false);

        Owner.LookAtTarget(Owner.Target);//상대를 바라보게함
    }

    protected override void OnAttackEvent()
    {
        //TODO -> 아래 코드 정상활처리
        //만약 상대가 죽었으면 스킵
        //if (Owner.Target.IsValid() == false)
        //    return;

        //각 캐릭별 Data를 찾아서 Projectile의 데이터가 없다면
        if (SkillData.ProjectileId == 0)
        {
            //기본 근접 공격
            Owner.Target.OnDamaged(Owner, this);
        }
        //Projectile 데이터가 있다면
        else
        {
            //기본 원거리 공격
            GenerateProjectile(Owner, Owner.CenterPosition);
        }
    }
}
