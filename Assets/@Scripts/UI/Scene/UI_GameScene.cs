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
    /* TODO
     * GameScene에 배치될 UI는
     * - 턴 종료 버튼
     * - 추가 예정
     */
    GameEvent EndPlayerTurn;

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        BindObjects(typeof(GameObjects));
        EndPlayerTurn = Managers.Resource.Load<GameEvent>("EndPlayerTurn");

        GetObject((int)GameObjects.TurnEndBtn).BindEvent((evt) =>
        {
            GetObject((int)GameObjects.TurnEndBtn).GetComponent<UnityEngine.UI.Image>().raycastTarget = false;
            Debug.Log("EndTurn Button Clicked!");
            EndPlayerTurn.Raise();
            // 버튼 비활성화
        });
        return true;
    }

    public void activeTurnEndBtn()
    {
        // 버튼 활성화
        GetObject((int)GameObjects.TurnEndBtn).GetComponent<UnityEngine.UI.Image>().raycastTarget = true;
        Debug.Log("EndTurn Button On!!!");
    }


}
