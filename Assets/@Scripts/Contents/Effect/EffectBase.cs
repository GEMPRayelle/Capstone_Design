using Data;
using System.Collections;
using UnityEditor.ShaderKeywordFilter;
using UnityEngine;
using UnityEngine.InputSystem;
using static Define;

//도트, 버프, 힐 등의 이펙트의 베이스 클래스
public class EffectBase : BaseObject
{
    public Creature Owner;
    public SkillBase Skill;//어떤 스킬에서 파생된 이펙트인지 확인하기 위한 변수
    public EffectData EffectData;
    public EEffectType EffectType;
    public int DataTemplateId { get; set; }
    protected float Remains { get; set; } = 0f;//이펙트가 얼마나 유지되어야 하는지
    protected EEffectSpawnType _spawnType;
    protected bool Loop { get; set; } = true;

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        return true;
    }

    //skill은 원본 스킬이 뭔지 기억하도록 하기 위한 것
    public virtual void SetInfo(int templateID, Creature owner, EEffectSpawnType spawnType, SkillBase skill)
    {
        DataTemplateId = templateID;
        EffectData = Managers.Data.EffectDic[templateID];

        Skill = skill;

        Owner = owner;
        _spawnType = spawnType;

        if (string.IsNullOrEmpty(EffectData.SkeletonDataID) == false)
            SetSpineAnimation(EffectData.SkeletonDataID, SortingLayers.SKILL_EFFECT);

        EffectType = EffectData.EffectType;

        //AoE에서 사용,범위 벗어나면 AoE가 꺼줌
        if (_spawnType == EEffectSpawnType.External)
            Remains = float.MaxValue;
        else
            Remains = EffectData.TickTime * EffectData.TickCount;
    }

    public virtual void ApplyEffect()
    {
        ShowEffect();
        StartCoroutine(CoStartTimer());
    }

    protected virtual void ShowEffect()
    {
        //애니메이션이랑 데이터셋에 설정이 다 되어있으면 재생
        if (SkeletonAnim != null && SkeletonAnim.skeletonDataAsset != null)
            PlayAnimation(0, AnimName.IDLE, Loop);
    }

    protected void AddModifier(CreatureStat stat, object source, int order = 0)
    {
        if (EffectData.Amount != 0)
        {
            StatModifier add = new StatModifier(EffectData.Amount, EStatModType.Add, order, source);
            stat.AddModifier(add);
        }
        if (EffectData.PercentAdd != 0)
        {
            StatModifier percentAdd = new StatModifier(EffectData.PercentAdd, EStatModType.PercentAdd, order, source);
            stat.AddModifier(percentAdd);
        }
        if (EffectData.PercentMult != 0)
        {
            StatModifier percentMult = new StatModifier(EffectData.PercentMult, EStatModType.PercentMult, order, source);
            stat.AddModifier(percentMult);
        }
    }

    protected void RemoveModifier(CreatureStat stat, object source)
    {
        stat.ClearModifiersFromSource(source);
    }

    public virtual bool ClearEffect(EEffectClearType clearType)
    {
        Debug.Log($"ClearEffect - {gameObject.name} {EffectData.ClassName} -> {clearType}");

        switch (clearType)
        {
            case EEffectClearType.TimeOut:
            case EEffectClearType.TriggerOutAoE:
            case EEffectClearType.EndOfAirbone:
                Managers.Object.Despawn(this);
                return true;

            case EEffectClearType.ClearSkill:
                //AoE범위 안에 있는 경우 해제 X
                if (_spawnType != EEffectSpawnType.External)
                {
                    Managers.Object.Despawn(this);
                    return true;
                }
                break;
        }
        return false;
    }

    protected virtual void ProcessDot()
    {

    }

    //특정 시간마다 도트데미지를 주는 함수
    protected virtual IEnumerator CoStartTimer()
    {
        float sumTime = 0f;

        ProcessDot();

        while (Remains > 0f)
        {
            Remains -= Time.deltaTime;
            sumTime += Time.deltaTime;

            //틱마다 ProcessDot 호출
            if (sumTime >= EffectData.TickTime)
            {
                ProcessDot();
                sumTime -= EffectData.TickTime;
            }
            yield return null;
        }
        Remains = 0f;

        ClearEffect(EEffectClearType.TimeOut);
    }
}
