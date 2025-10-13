using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UI;
using static Define;

//GameScene에 실질적으로 고정으로 배치될 UI들에 대한 제어를 하는 클래스
public class UI_GameScene : UI_Scene
{
    enum GameObjects
    {
        TurnEndBtn
    }

    // GameEvent 
    GameEvent EndPlayerTurn; // 턴 종료 버튼 누르면 Raise


    UnityEngine.UI.Image TurnEndBtn;


    private Color enableColor = new Color(0.9f, 0.9f, 0.9f, 1.0f);
    private Color disabledColor = new Color(0.4f, 0.4f, 0.4f, 0.4f);
    private Color hoverColor = Color.white;

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        BindObjects(typeof(GameObjects));
        EndPlayerTurn = Managers.Resource.Load<GameEvent>("EndPlayerTurn");
        TurnEndBtn = GetObject((int)GameObjects.TurnEndBtn).GetComponent<UnityEngine.UI.Image>();

        // 초기엔 클릭안되게
        TurnEndBtn.color = disabledColor;
        TurnEndBtn.raycastTarget = false;

        // 클릭 이벤트
        GetObject((int)GameObjects.TurnEndBtn).BindEvent((evt) =>
        {
            TurnEndBtn.raycastTarget = false;
            TurnEndBtn.color = disabledColor;
            Debug.Log("EndTurn Button Clicked!");
            EndPlayerTurn.Raise();
            // 버튼 비활성화
        }, Define.EUIEvent.Click);

        // Hover 진입 이벤트
        GetObject((int)GameObjects.TurnEndBtn).BindEvent((evt) =>
        {
            Debug.Log("HoverEnter");
            if (TurnEndBtn.raycastTarget == true) // 활성화 상태일 때만 hover 효과
            {
                TurnEndBtn.color = hoverColor;
            }
        }, Define.EUIEvent.PointerEnter);

        // Hover 벗어남 이벤트
        GetObject((int)GameObjects.TurnEndBtn).BindEvent((evt) =>
        {
            Debug.Log("HoverEnd");
            if (TurnEndBtn.raycastTarget == true) // 활성화 상태일 때만
            {
                TurnEndBtn.color = enableColor;
            }
        }, Define.EUIEvent.PointerExit);

        return true;
    }

    public void activeTurnEndBtn()
    {
        // 버튼 활성화
        TurnEndBtn.raycastTarget = true;
        TurnEndBtn.color = enableColor;
    }


}
