using UnityEngine;
using UnityEngine.EventSystems;

public class UI_SkillListPopUp : UI_Popup
{
    enum GameObjects
    {
        CloseArea,
    }

    enum Buttons
    {
        Button_Move,
        Button_Attack,
        Button_Skill,
        Button_Info
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

        return true;
    }

    void OnClickCloseArea(PointerEventData evt)
    {
        evt.Use();
        Managers.UI.ClosePopupUI(this);
    }

    void OnClickMoveButton(PointerEventData evt)
    {
        evt.Use();
        Debug.Log("OnClickMovementButton");
        Managers.UI.ClosePopupUI(this);

    }

    void OnClickAttackButton(PointerEventData evt)
    {
        evt.Use();
        Debug.Log("OnClickAttactButton");
        Managers.UI.ClosePopupUI(this);

    }

    void OnClickSkillButton(PointerEventData evt)
    {
        evt.Use();
        Debug.Log("OnClickSkillButton");
        Managers.UI.ClosePopupUI(this);

    }

    void OnClickInfoButton(PointerEventData evt)
    {
        evt.Use();
        Debug.Log("OnClickInfoButton");
        Managers.UI.ClosePopupUI(this);
    }
}
