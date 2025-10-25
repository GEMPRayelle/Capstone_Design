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
            SkillRangeTiles = new List<OverlayTile>();
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

        public void GetInRangeTiles()
        {
            // 캐릭터의 현재 위치를 기준으로 이동 가능한 타일 계산
            if (creature.IsMoved == false) // 이동 가능한 타일이므로 isMoved가 false일때만 계산
            {
                rangeFinderTiles = Managers.Map.GetTilesInRange(
                new Vector2Int(creature.currentStandingTile.gridLocation.x, creature.currentStandingTile.gridLocation.y),
                creature.MovementRange);
            }

            else
            {
                rangeFinderTiles = new List<OverlayTile>();
            }

        }

        public void GetSkillRangeTilesPlayer()
        {
            SkillRangeTiles = Managers.Map.GetTilesInRange(
                creature.currentStandingTile,
                creature.NormalAttackRange, true, true); // TODO 현재 활성화 된 스킬의 AttackRange, 즉 스킬 ui  누르면 SkillRange도 그 스킬에 정보로 변경되게
        }

        public void GetSkillRangeTilesCopy()
        {
            if (Managers.Controller.spawnController.IsCopyValid())
            {
                SkillRangeTiles = Managers.Map.GetTilesInRange(
                    Managers.Controller.spawnController.GetCopyStandingTile(),
                    creature.NormalAttackRange, true, true); // TODO
            }

        }

        public void ResetRangeTiles() { rangeFinderTiles.Clear(); }

        public void ResetSkillRangeTiles() { SkillRangeTiles.Clear(); }
        #endregion
    }

}
