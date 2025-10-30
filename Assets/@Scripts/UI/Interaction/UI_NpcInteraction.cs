using UnityEngine;
using UnityEngine.EventSystems;
using static Define;

public class UI_NpcInteraction : UI_Base    
{
    //내 npc가 누구인지
    private Npc _owner;

    //Npc위에 떠있는 버튼을 눌러서 상호작용 수행
    enum Buttons
    {
        InteractionButton
    }

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        BindButtons(typeof(Buttons));

        GetComponent<Canvas>().worldCamera = Camera.main;

        return true;
    }

    public void SetInfo(int dataId, Npc owner)
    {
        _owner = owner;
        GetButton((int)Buttons.InteractionButton).gameObject.BindEvent(OnClickInteractionButton);
    }

    private void OnClickInteractionButton(PointerEventData evt)
    {
        //npc쪽에 전달해서 이벤트 실행
        _owner?.OnClickEvent();
        
        //대화씬으로 이동
        Managers.Scene.LoadScene(EScene.TalkingScene);

        Debug.Log("OnClickInteractionButton");
    }
}
