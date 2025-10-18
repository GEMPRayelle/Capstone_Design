using Unity.VisualScripting;
using UnityEngine;
using static Define;
public class DevScene : BaseScene
{
    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        //Scene타입 정의
        SceneType = Define.EScene.DevScene;
        
        //DevScene에서 맵 로딩
        Managers.Map.LoadMap("DevMap");

        #region Player
        {
            //int heroTemplateID = HERO_KNIGHT_ID;
            //int heroTemplateID = HERO_WIZARD_ID;
            //int heroTemplateID = HERO_LION_ID;
            //int heroTemplateID = HERO_VAMPIRE_ID;

            //ObjectManager를 통한 플레이어 스폰
            //Player master = Managers.Object.Spawn<Player>(new Vector3(4,0,0), heroTemplateID);
            //master.CreatureState = Define.ECreatureState.Idle;

            //카메라 설정
            CameraController camera = Camera.main.GetOrAddComponent<CameraController>();
        }
        #endregion

        #region Monster
        {
            //Monster monster = Managers.Object.Spawn<Monster>(Vector3.zero, MONSTER_GOBLIN_ARCHER_ID); 
            // 처음 실행할 때 monster spawner 기능 MonsterRoot에 만들기
            // Managers.Object.MonsterRoot.gameObject.GetOrAddComponent<MonsterSpawner>();
            //Managers.Object.Despawn<Monster>(monster);
        }
        #endregion

        #region UI
        {
            //조이스틱 UI 생성
            //Managers.UI.ShowBaseUI<UI_Joystick>();
            // Tag 버튼 UI 생성
            //UI_TagBtn tagBtn = Managers.UI.ShowBaseUI<UI_TagBtn>();
            //tagBtn.gameObject.SetActive(false);

            //Managers.UI.ShowBaseUI<UI_TagGauge>();
            Managers.UI.ShowBaseUI<UI_GameScene>();
        }
        #endregion

        #region Contents_Init
        Managers.Controller.InitController();
        Managers.Listener.InitGameScene();
        Managers.Listener.InstantiateListener();
        #endregion

        return true;
    }

    public override void Clear()
    {
        //TODO - Scene이 바뀔 경우 오브젝트들을 밀어주는 작업추가
    }

}
