using UnityEngine;

//GameScene에 실질적으로 고정으로 배치될 UI들에 대한 제어를 하는 클래스
public class UI_GameScene : UI_Scene
{
    /* TODO
     * GameScene에 배치될 UI는
     * - 턴 종료 버튼
     * - 추가 예정
     */

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        return true;
    }


}
