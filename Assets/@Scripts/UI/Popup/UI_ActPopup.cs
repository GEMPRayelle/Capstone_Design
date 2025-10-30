using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class UI_ActPopup : UI_Popup
{
    enum GameObjects 
    {
        CloseArea,
        Popup,
        Popup_Skill
    }

    enum Buttons
    {
        Button_Move,
        Button_Attack,
        Button_Skill,
        Button_Info,

        SkillA,
        SkillB
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

        return true;
    }

    void OnClickCloseArea(PointerEventData evt)
    {
        evt.Use();
        Managers.Controller.mouseController.PlayerControlState = Define.EPlayerControlState.None;
        Managers.UI.ClosePopupUI(this);
    }

    void OnClickMoveButton(PointerEventData evt)
    {
        evt.Use();
        Managers.Controller.spawnController.SwitchCopy(Managers.Controller.PlayerState.creature.currentStandingTile.gameObject); // 일단 조종하는 크리쳐 위에 생성
        Managers.Controller.PlayerState.creature.GetMovementRangeTiles();
        Managers.Controller.tileEffectController.ShowMovementRangeTiles();
        Managers.Controller.mouseController.PlayerControlState = Define.EPlayerControlState.Move; // Move모드로 변경
        Managers.UI.ClosePopupUI(this);

    }

    void OnClickAttackButton(PointerEventData evt)
    {
        evt.Use();
        Debug.Log("OnClickAttactButton");
        Managers.Controller.mouseController.PlayerControlState = Define.EPlayerControlState.Skill;
        Player player = Managers.Controller.PlayerState.creature; 
        player.SkillRange = 5;  // 일단 SkillRange만 변경
        Managers.Controller.PlayerState.creature.GetSkillRangeTilesPlayer();
        Managers.Controller.tileEffectController.ShowSkillRangeTile();
        Managers.UI.ClosePopupUI(this);

    }

    void OnClickSkillButton(PointerEventData evt)
    {
        evt.Use();
        Debug.Log("OnClickSkillButton");

        GameObject PopUpSkill = GetObject((int)GameObjects.Popup_Skill);
        PopUpSkill.SetActive(true);

        GameObject Popup = GetObject((int)GameObjects.Popup);
        Popup.SetActive(false);

        GameObject SkillA = GetButton((int)Buttons.SkillA).gameObject;
        GameObject SkillB = GetButton((int)Buttons.SkillB).gameObject;
        Player player = Managers.Controller.PlayerState.creature;
        
        // TODO@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
        SkillA.GetComponent<TextMeshPro>().SetText(player.Skills.SkillList[0].SkillData.Name);
        SkillB.GetComponent<TextMeshPro>().SetText(player.Skills.SkillList[1].SkillData.Name);

        Managers.UI.ClosePopupUI(this);

    }

    void OnClickInfoButton(PointerEventData evt)
    {
        evt.Use();
        Debug.Log("OnClickInfoButton");
        Managers.UI.ClosePopupUI(this);
    }

    void OnClickSkillAButton(PointerEventData evt)
    {
        evt.Use();
        Debug.Log("OnClickSkillAButton");
    }

    void OnClickSkillBButton(PointerEventData evt)
    {
        evt.Use();
        Debug.Log("OnClickSkillBButton");
    }
}
