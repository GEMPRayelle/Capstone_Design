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

    GameEvent StartMovementPlayer;
    GameEvent CancelMovement;

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

        StartMovementPlayer = Managers.Resource.Load<GameEvent>("StartMovementPlayer");
        CancelMovement = Managers.Resource.Load<GameEvent>("CancelMovement");
        return true;
    }

    void OnClickCloseArea(PointerEventData evt)
    {
        evt.Use();
        CancelMovement.Raise();
        Managers.UI.ClosePopupUI(this);
    }

    void OnClickMovementButton(PointerEventData evt)
    {
        evt.Use();
        Debug.Log("OnClickMovementButton");
        StartMovementPlayer.Raise();
        Managers.UI.ClosePopupUI(this);

    }

    void OnClickSkillButton(PointerEventData evt)
    {
        evt.Use();
        Debug.Log("OnClickSKillButton");
    }

    void OnClickCloseButton(PointerEventData evt)
    {
        evt.Use();
        CancelMovement.Raise();
        Managers.UI.ClosePopupUI(this);
    }
}
