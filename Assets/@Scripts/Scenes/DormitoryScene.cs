using Unity.VisualScripting;
using UnityEngine;
using static Define;
public class DormitoryScene : BaseScene
{
    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        //Scene타입 정의
        SceneType = EScene.DormitoryScene;

        //DevScene에서 맵 로딩
        Managers.Map.LoadMap("00_Dormitory");

        #region Player
        {
            //카메라 설정
            CameraController camera = Camera.main.GetOrAddComponent<CameraController>();
        }
        #endregion

        #region UI
        {
            Managers.UI.ShowBaseUI<UI_Joystick>();
            Managers.UI.ShowBaseUI<UI_PointAndClick>();
            Managers.UI.ShowBaseUI<UI_DormitoryScene>();
        }
        #endregion

        #region Contents_Init
        //Managers.Controller.InitController();
        //Managers.Listener.InitGameScene();
        //Managers.Listener.InstantiateListener();
        #endregion

        return true;
    }

    public override void Clear()
    {
        //TODO - Scene이 바뀔 경우 오브젝트들을 밀어주는 작업추가
    }

}
