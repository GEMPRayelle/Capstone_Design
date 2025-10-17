using UnityEngine;

public class AreaAttack : AreaSkill
{
    //스킬 범위가 출력
    public override void SetInfo(Creature owner, int skillTemplateID)
    {
        base.SetInfo(owner, skillTemplateID);

        _angleRange = 90;

        AddIndicatorComponent();

        if (_indicator != null)
            _indicator.SetInfo(Owner, SkillData, Define.EIndicatorType.Cone);
    }

    //스킬을 사용
    public override void DoSkill()
    {
        base.DoSkill();

        SpawnSpellIndicator();
    }
}
