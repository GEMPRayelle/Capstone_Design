using System.Collections.Generic;
using UnityEngine;

public class AreaSkill : SkillBase
{
    protected SpellIndicator _indicator;
    protected Vector2 _skillDir;
    protected Define.EIndicatorType _indicatorType = Define.EIndicatorType.Cone;
    protected int _angleRange = 360;

    public override void SetInfo(Creature owner, int skillTemplateID)
    {
        base.SetInfo(owner, skillTemplateID);
    }

    public override void DoSkill()
    {
        base.DoSkill();

        if (Owner.CreatureState != Define.ECreatureState.Skill)
            return;

        _skillDir = (Owner.Target.transform.position - Owner.transform.position).normalized;
    }

    public override void CancelSkill()
    {
        if (_indicator)
            _indicator.Cancel();
    }

    protected void AddIndicatorComponent()
    {
        _indicator = Util.FindChild<SpellIndicator>(gameObject, recursive: true);
        if (_indicator == null)
        {
            GameObject go = Managers.Resource.Instantiate(SkillData.PrefabLabel, gameObject.transform);
            _indicator = Util.GetOrAddComponent<SpellIndicator>(go);
        }
    }

    protected void SpawnSpellIndicator()
    {
        if (Owner.Target.IsValid() == false)
            return;

        _indicator.ShowCone(Owner.transform.position, _skillDir.normalized, _angleRange);
    }

    //피격하는 범위에 대한 처리
    protected override void OnAttackEvent()
    {
        float radius = Util.GetEffectRadius(SkillData.EffectSize);
        //범위에 따라서 실질적으로 피격 판정을 할 타겟들을 모아줌
        //TODO -> 그리드 범위 기반으로 판정해야함
        //List<Creature> targets = Managers.Object.FindConeRangeTargets(Owner, _skillDir, radius, _angleRange);
        List<Creature> targets = new List<Creature>();

        //피격판정을 진행
        foreach (var target in targets)
        {
            if (target.IsValid())
                target.OnDamaged(Owner, this);
        }
    }
}
