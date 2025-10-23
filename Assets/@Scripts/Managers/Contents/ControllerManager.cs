using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ControllerManager
{
    public GameObject ControllerContainer;
    public PlayerMouseController mouseController;
    public MovementController movementController;
    public PlayerMovementController playerMovementController;
    public SpawnController spawnController;
    public TileEffectController tileEffectController;
    public SharedPlayerState PlayerState;

    public void InitController()
    {
        PlayerState = new SharedPlayerState();

        ControllerContainer = Managers.Resource.Instantiate("ControllerContainer");
        movementController = Util.FindChild<MovementController>(ControllerContainer, "MovementController");
        playerMovementController = Util.FindChild<PlayerMovementController>(ControllerContainer, "PlayerMovementController");
        spawnController = Util.FindChild<SpawnController>(ControllerContainer, "SpawnController");
        tileEffectController = Util.FindChild<TileEffectController>(ControllerContainer, "TileEffectController");
        mouseController = Util.FindChild<PlayerMouseController>(ControllerContainer, "MouseController");

    }

    // Player 관련 Controller들이 공통으로 들고 있는 상태 정보들 저장
    public class SharedPlayerState
    {
        public Player creature; //현재 생성된 캐릭터 정보
        public bool isMoving; // 이동 중인지
        public PathFinder _pathFinder; //경로 탐색기
        public List<OverlayTile> path;//타일의 이동 경로 정보 리스트

        public List<OverlayTile> rangeFinderTiles; // 캐릭터 이동 범위
        public List<OverlayTile> SkillRangeTiles; // 캐릭터 공격 범위
        public List<int> spawnablePlayerID; // order가 스폰할 캐릭터들 ID

        public SharedPlayerState() 
        { 
            creature = null;
            isMoving = false; 
            _pathFinder = new PathFinder();
            path = new List<OverlayTile>();

            rangeFinderTiles = new List<OverlayTile>();
            //SkillRangeTiles = new List<OverlayTile>();
            spawnablePlayerID = new List<int>();

            spawnablePlayerID.Add(201000);
            spawnablePlayerID.Add(201003);
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

        // GetRangeTiles도 여기?
        #endregion
    }

}
