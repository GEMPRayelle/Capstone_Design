using System.Collections.Generic;
using UnityEngine;
using static Define;

public class ControllerManager
{
    public GameObject ControllerContainer;
    public MonsterMovementController movementController;
    public PlayerMovementController playerMovementController;
    public SpawnController spawnController;
    public TileEffectController tileEffectController;
    public PlayerMouseController mouseController;
    public CameraController cameraController;
    public SharedPlayerState PlayerState;

    public void InitController()
    {
        PlayerState = new SharedPlayerState();

        ControllerContainer = Managers.Resource.Instantiate("ControllerContainer");
        movementController = Util.FindChild<MonsterMovementController>(ControllerContainer, "MovementController");
        playerMovementController = Util.FindChild<PlayerMovementController>(ControllerContainer, "PlayerMovementController");
        spawnController = Util.FindChild<SpawnController>(ControllerContainer, "SpawnController");
        tileEffectController = Util.FindChild<TileEffectController>(ControllerContainer, "TileEffectController");
        mouseController = Util.FindChild<PlayerMouseController>(ControllerContainer, "MouseController");
        cameraController = Camera.main.GetComponent<CameraController>();

        mouseController.SetOtherController();
    }

    // Player 관련 Controller들이 공통으로 들고 있는 상태 정보들 저장
    public class SharedPlayerState
    {
        public Player creature; //현재 생성된 캐릭터 정보, 절때 MouseController 외부에서 변경해선 안됨
        public bool isMoving; // 이동 중인지
        public PathFinder _pathFinder; //경로 탐색기
        public List<OverlayTile> path;//타일의 이동 경로 정보 리스트

        
        public List<int> spawnablePlayerID; // order가 스폰할 캐릭터들 ID

        public SharedPlayerState() 
        { 
            creature = null;
            isMoving = false; 
            _pathFinder = new PathFinder();
            path = new List<OverlayTile>();

            spawnablePlayerID = new List<int>();

            spawnablePlayerID.Add(HERO_WIZARD_ID);
            spawnablePlayerID.Add(HERO_LION_ID);
        }

        #region Helper_Methods
        public bool IsSpawnable()
        {
            return (spawnablePlayerID.Count > 0);
        }

        public bool IsSpawnEnd()
        {
            return (spawnablePlayerID.Count == 0);
        }

        
        public void CleanupAllPlayer() // 모든 타일 정보 초기화 함수
        {
            foreach (Creature player in Managers.Turn.activePlayerList) // 플레이어 마다
            {
                player.GetMovementRangeTiles(); // 이동 범위 타일 구하기
                player.GetSkillRangeTilesPlayer(); // 스킬 범위 타일 구하기
                creature = player as Player;

                Managers.Controller.tileEffectController.HideAllRangeTiles(); // 구한 모든 타일 가리기

                player.ResetMovementRangeTiles(); // 이동 타일 초기화
                player.ResetSkillRangeTiles(); // 스킬 범위 타일 초기화
            }

            if (creature != null) // creature은 조종하는 크리쳐기 때문에 반드시 초기화
                creature = null;
        }
        #endregion
    }

}
