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

        //ObjectManager를 통한 플레이어 스폰
        //TODO -> 플레이어 프리팹을 직접 만들지를 않고 스폰 하면 에러가 나기에 임시 주석 처리
        //Player player = Managers.Object.Spawn<Player>(Vector3.zero);
        //player.CreatureState = Define.ECreatureState.Idle;

        //조이스틱 UI 생성
        Managers.UI.ShowBaseUI<UI_Joystick>();

        //임시 카메라 설정
        Camera camera = Camera.main;
        camera.orthographicSize = 15;

        return true;
    }

    public override void Clear()
    {
        //TODO - Scene이 바뀔 경우 오브젝트들을 밀어주는 작업추가
    }

}
