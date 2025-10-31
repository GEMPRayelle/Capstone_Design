using Data;
using System;
using System.Collections.Generic;
using UnityEngine;
using static Define;
using Random = UnityEngine.Random;

//플레이어에 대한 스킬만 관리하는 클래스 (SkillManager)
public class SkillComponent : InitBase
{
    //본인이 들고있을 스킬 리스트
    public List<SkillBase> SkillList { get; } = new List<SkillBase>();
    //사용 가능한 스킬 리스트
    public List<SkillBase> ActiveSkills { get; set; } = new List<SkillBase>();

    public SkillBase DefaultSkill { get; private set; }

    private SkillBase _currentSkill;
    //현재 사용할 스킬
    public SkillBase CurrentSkill
    {
        get
        {
            //준비된 스킬이 하나도 없을 시
            if (ActiveSkills.Count == 0)
                return DefaultSkill; //평타
            
            return _currentSkill;
        }

        set
        {
            _currentSkill = value;
        }
    }

    private Creature _owner; //스킬을 사용하는 객체

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        return true;
    }

    //들고있는 스킬목록을 처음에 초기화
    public void SetInfo(Creature owner, CreatureData creatureData)
    {
        _owner = owner;

        AddSkill(creatureData.DefaultSkillId, ESkillSlot.Default);
        AddSkill(creatureData.SkillAId, ESkillSlot.A);
        AddSkill(creatureData.SkillBId, ESkillSlot.B);

        //Debugging
        Debug.Log($"{_owner.name} DefaultSkill: {(DefaultSkill != null ? DefaultSkill.name : "null")}, ActiveSkillCount: {ActiveSkills.Count}");
    }

    //리스트에 스킬을 추가하는 함수
    public void AddSkill(int skillTemplateID, ESkillSlot sKillSlot)
    {
        //예외처리 (0일시 없는 스킬임)
        if (skillTemplateID == 0)
            return;

        //일부 스킬에서 없는것이 있으면 진행하지않고 스킵
        if (Managers.Data.SkillDic.TryGetValue(skillTemplateID, out var data) == false)
        {
            Debug.LogWarning($"AddSkill Failed {skillTemplateID}");
            return;
        }

        //DataSheet에 있는 스킬들을 가져옴 (*반드시 클래스로 구현된 스킬들 ex:NormalAttack)
        SkillBase skill = gameObject.AddComponent(Type.GetType(data.ClassName)) as SkillBase;

        //DataSheet에 그 스킬이 없다면
        if (skill == null)
        {
            //에러 처리
            Debug.LogWarning($"AddSkill Failed {skillTemplateID}");
            return;
        }

        skill.SetInfo(_owner, skillTemplateID);

        //스킬 리스트에 DataSheet에서 가져온 스킬들을 추가
        SkillList.Add(skill);

        switch (sKillSlot)
        {
            case ESkillSlot.Default:
                DefaultSkill = skill;
                break;
        }


    }

}