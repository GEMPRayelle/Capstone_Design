using Unity.VisualScripting;
using UnityEngine;

public class DevScene : BaseScene
{
    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        //Scene타입 정의
        SceneType = Define.EScene.DevScene;

        //맵 생성
        GameObject map = Managers.Resource.Instantiate("BaseMap");
        map.transform.position = Vector3.zero;
        map.name = "@BaseMap";

        #region Player
        {
            //ObjectManager를 통한 플레이어 스폰
            Player servant = Managers.Object.Spawn<Player>(Vector3.right, 0);
            servant.CreatureState = Define.ECreatureState.Idle;
            servant.PlayerState = Define.EPlayerState.Servant;

            Player master = Managers.Object.Spawn<Player>(Vector3.left, 0);
            master.CreatureState = Define.ECreatureState.Idle;
            master.PlayerState = Define.EPlayerState.Master;


            Managers.Game.PlayerState = Define.EPlayerState.Servant;

            //카메라 설정
            CameraController camera = Camera.main.GetOrAddComponent<CameraController>();
            camera.Servant = servant;
            camera.Master = master;
            camera.Target = servant;
        }
        #endregion

        #region Monster
        {
            Monster monster = Managers.Object.Spawn<Monster>(Vector3.zero, 0); // 처음 실행할 때 monster spawner 기능 MonsterRoot에 만들기
            Managers.Object.MonsterRoot.gameObject.GetOrAddComponent<MonsterSpawner>();
            Managers.Object.Despawn<Monster>(monster);

        }
        #endregion

        #region UI
        {
            //조이스틱 UI 생성
            Managers.UI.ShowBaseUI<UI_Joystick>();
            // Tag 버튼 UI 생성
            Managers.UI.ShowBaseUI<UI_TagBtn>();
        }
        #endregion

        return true;
    }

    public override void Clear()
    {
        //TODO - Scene이 바뀔 경우 오브젝트들을 밀어주는 작업추가
    }

}
