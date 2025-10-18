using UnityEngine;
using UnityEngine.EventSystems;

public class UI_MovementPopup : UI_Popup
{
    enum GameObjects 
    {
        CloseArea,
    }

    enum Buttons
    {
        Button_Move,
        Button_Skill,
        Button_Close,
    }
    
    enum Texts
    {
        Text_PopupName,
        Text_PopupDescription,
    }


    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        BindObjects(typeof(GameObjects));
        BindButtons(typeof(Buttons));
        BindTexts(typeof(Texts));

        GetObject((int)GameObjects.CloseArea).BindEvent(OnClickCloseArea);
        GetButton((int)Buttons.Button_Move).gameObject.BindEvent(OnClickMovementButton);
        GetButton((int)Buttons.Button_Skill).gameObject.BindEvent(OnClickSkillButton);
        GetButton((int)Buttons.Button_Close).gameObject.BindEvent(OnClickCloseButton);

        GetText((int)Texts.Text_PopupName).text = "이동할 위치";
        GetText((int)Texts.Text_PopupDescription).text = $"버튼을 눌러야 이동이 확정됩니다.";

        return true;
    }

    void OnClickCloseArea(PointerEventData evt)
    {
        Managers.UI.ClosePopupUI(this);
    }

    void OnClickMovementButton(PointerEventData evt)
    {
        Debug.Log("OnClickMovementButton");
    }

    void OnClickSkillButton(PointerEventData evt)
    {
        Debug.Log("OnClickSKillButton");
    }

    void OnClickCloseButton(PointerEventData evt)
    {
        Managers.UI.ClosePopupUI(this);
    }
}
