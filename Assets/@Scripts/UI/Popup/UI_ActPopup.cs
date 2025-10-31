using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class UI_ActPopup : UI_Popup
{
    enum GameObjects 
    {
        CloseArea,
        Popup,
        Popup_Skill,
        Popup_Info,
        Popup_Skill_Detail
    }

    enum Buttons
    {
        Button_Move,
        Button_Attack,
        Button_Skill,
        Button_Info,

        SkillA,
        SkillB,

        SkillUse
    }

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        BindObjects(typeof(GameObjects));
        BindButtons(typeof(Buttons));

        GetObject((int)GameObjects.CloseArea).BindEvent(OnClickCloseArea);
        GetButton((int)Buttons.Button_Move).gameObject.BindEvent(OnClickMoveButton);
        GetButton((int)Buttons.Button_Attack).gameObject.BindEvent(OnClickAttackButton);
        GetButton((int)Buttons.Button_Skill).gameObject.BindEvent(OnClickSkillButton);
        GetButton((int)Buttons.Button_Info).gameObject.BindEvent(OnClickInfoButton);

        GetButton((int)Buttons.SkillA).gameObject.BindEvent(OnClickSkillAButton);
        GetButton((int)Buttons.SkillB).gameObject.BindEvent(OnClickSkillBButton);

        GetObject((int)GameObjects.Popup_Skill).SetActive(false);
        GetObject((int)GameObjects.Popup_Info).SetActive(false);
        GetObject((int)GameObjects.Popup_Skill_Detail).SetActive(false);

        return true;
    }

    void OnClickCloseArea(PointerEventData evt)
    {
        Managers.Controller.mouseController.PlayerControlState = Define.EPlayerControlState.None;
        Managers.UI.ClosePopupUI(this);
    }

    void OnClickMoveButton(PointerEventData evt)
    {
        Managers.Controller.spawnController.SwitchCopy(Managers.Controller.PlayerState.creature.currentStandingTile.gameObject); // 일단 조종하는 크리쳐 위에 생성
        Managers.Controller.PlayerState.creature.GetMovementRangeTiles();
        Managers.Controller.tileEffectController.ShowMovementRangeTiles();
        Managers.Controller.mouseController.PlayerControlState = Define.EPlayerControlState.Move; // Move모드로 변경
        Managers.UI.ClosePopupUI(this);

    }

    void OnClickAttackButton(PointerEventData evt)
    {
        Debug.Log("OnClickAttactButton");
        Managers.Controller.mouseController.PlayerControlState = Define.EPlayerControlState.Skill;
        Player player = Managers.Controller.PlayerState.creature; 
        player.SkillRange = 5;  // 일단 SkillRange만 변경, TODO : CurrentSkill을 기본 스킬로 변경
        Managers.Controller.PlayerState.creature.GetSkillRangeTilesPlayer();
        Managers.Controller.tileEffectController.ShowSkillRangeTile();
        Managers.UI.ClosePopupUI(this);

    }

    void OnClickSkillButton(PointerEventData evt)
    {
        Debug.Log("OnClickSkillButton");

        GameObject PopUpSkill = GetObject((int)GameObjects.Popup_Skill);
        PopUpSkill.SetActive(true); // PopUpSkill창 키기

        GameObject Popup = GetObject((int)GameObjects.Popup);
        Popup.SetActive(false); // Popup창 끄기

        GameObject SkillA = GetButton((int)Buttons.SkillA).gameObject;
        GameObject SkillB = GetButton((int)Buttons.SkillB).gameObject;
        Player player = Managers.Controller.PlayerState.creature;

        Util.FindChild<TextMeshProUGUI>(SkillA, null, true).text = player.Skills.SkillList[0].SkillData.Name;
        Util.FindChild<TextMeshProUGUI>(SkillB, null, true).text = player.Skills.SkillList[1].SkillData.Name;
    }

    void OnClickInfoButton(PointerEventData evt)
    {
        GetObject((int)GameObjects.Popup_Info).SetActive(true);
        Debug.Log("OnClickInfoButton");
    }

    void OnClickSkillAButton(PointerEventData evt)
    {
        GetObject((int)GameObjects.Popup_Skill_Detail).SetActive(true);
        //GetObject((int)GameObjects.Popup_Skill_Detail).GetComponent<RectTransform>().anchoredPosition = GetObject((int)GameObjects.Popup_Skill).GetComponent<RectTransform>().anchoredPosition + new Vector2(100, 0); 위치 조정
        // TODO : HeroSkill Data 정해지면 그걸로 각각 설정, UI, 해상도 크기 설정, 어떻게 꺼지는지(새로운 팝업창으로 따로 관리 OR 지금처럼), 사용 스킬 설정
        Debug.Log("OnClickSkillAButton");

    }

    void OnClickSkillBButton(PointerEventData evt)
    {
        GetObject((int)GameObjects.Popup_Skill_Detail).SetActive(true);
        // TODO : HeroSkill Data 정해지면 그걸로 각각 설정, UI, 해상도 크기 설정, 어떻게 꺼지는지(새로운 팝업창으로 따로 관리 OR 지금처럼), 사용 스킬 설정
        Debug.Log("OnClickSkillBButton");
    }
}
