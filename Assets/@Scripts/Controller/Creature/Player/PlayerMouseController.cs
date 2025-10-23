using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using static ControllerManager;
using static Define;

public class PlayerMouseController : InitBase
{
    private GameObject cursor; // 커서
    private bool isClickedOrder; // Order 캐릭터가 다른 캐릭터 스폰시킬때 클릭된지에 대한 여부
    private SharedPlayerState PlayerState; // Controller들이 공유하는 데이터
 
    //GameEvent
    GameEvent AllPlayerSpawn; // 모든 플레이어 스폰 될 때 Raise

    // MouseController -> SpawnController
    GameEvent despawnCopy; // copy 지울때 날릴 이벤트
    GameEventGameObject SwitchToOrderForSpawn; // 스폰모드일 때 Copy 세팅을 위해 날리는 이벤트
    GameEventGameObject InstantiatePlayerByOrder; // 스폰모드일 때 실제 Spawn 함수를 실행하기위해 날리는 이벤트

    // MouseController -> TileEffectController
    GameEvent HighlightSpawnTile; // 스폰 타일 보여줄 때 날릴 이벤트
    GameEvent ShowRangeTiles; // 계산된 범위 타일 중 실제 이동 가능 길 보여줄 때 날릴 이벤트
    GameEvent HideAllRangeTiles; // 계산된 범위 타일 가릴 때 날릴 이벤트




    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        PlayerState = Managers.Controller.PlayerState;
        cursor = Managers.Resource.Instantiate("Cursor");
        isClickedOrder = false;

        AllPlayerSpawn = Managers.Resource.Load<GameEvent>("AllPlayerSpawn");
        despawnCopy = Managers.Resource.Load<GameEvent>("DespawnCopy");
        SwitchToOrderForSpawn = Managers.Resource.Load<GameEventGameObject>("switchToOrderForSpawn");
        InstantiatePlayerByOrder = Managers.Resource.Load<GameEventGameObject>("InstantiatePlayerByOrder");

        HighlightSpawnTile = Managers.Resource.Load<GameEvent>("HighlightSpawnTile");
        ShowRangeTiles = Managers.Resource.Load<GameEvent>("ShowRangeTiles");
        HideAllRangeTiles = Managers.Resource.Load<GameEvent>("HideAllRangeTiles");

        return true;
    }


    void LateUpdate()
    {
        RaycastResult hit = GetFocusedObjects(); // 마우스가 가리키는 타일 감지
        //타일이 감지 됐다면, 이동중이 아닐때만 감지
        if (hit.HasTile && !PlayerState.isMoving)
        {
            UpdateCursor(hit.tile);
            HandleMouseHover(hit);
            HandleMouseClick(hit);
        }

        
    }

    #region 현재 마우스 컨트롤 상태
    EPlayerControlState _playerControlState = EPlayerControlState.None;
    public EPlayerControlState PlayerControlState
    {
        get
        {
            _playerControlState = GetCurrentControlState();
            return _playerControlState;
        }
        private set
        {
            _playerControlState = value;
        }
    }

    // 현재 마우스 클릭 상태를 가져오는 함수
    private EPlayerControlState GetCurrentControlState()
    {
        if (PlayerState.creature == null) // 조종중인 크리쳐가 없다면 NONE
            return EPlayerControlState.None;

        if (PlayerState.creature.IsMoved == true) // 이미 조종한 크리쳐를 조종하려 한다면 
            return EPlayerControlState.ControlledFinish;

        if (PlayerState.creature.PlayerType == EPlayerType.Offensive) // 아직 행동하지않은 공격형 크리쳐를 조종한다면
            return EPlayerControlState.ControllingOffensive;

        if (PlayerState.creature.PlayerType == EPlayerType.Order) // 아직 행동하지않은 오더를 조종한다면
        {
            return isClickedOrder ? // 오더가 스폰을 시작했다면
                EPlayerControlState.ControllingOrderForSpawn : // True : 스폰 모드
                EPlayerControlState.ControllingOrderForMove; // False : 이동 모드
        }

        return EPlayerControlState.None;
    }
    #endregion

    #region 마우스 RayCast 검사
    // Ray 쏴서 결과값 받을 구조체
    public struct RaycastResult
    {
        public OverlayTile tile; // 타일
        public Player player; // 플레이어
        public RaycastHit2D hit;

        public bool HasTile => tile != null;
        public bool HasPlayer => player != null;
    }

    //마우스가 가리키는 타일을 감지하는 함수
    private static RaycastResult GetFocusedObjects()
    {
        RaycastResult result = new RaycastResult();
        //마우스 위치를 월드 좌표로 변환
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        //2D 좌표로 변환
        Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);
        //해당 위치에 있는 모든 콜라이더 감지
        RaycastHit2D[] hits = Physics2D.RaycastAll(mousePos2D, Vector2.zero);
        //마우스가 가리키고 있는 위치에 감지된 오브젝트가 
        if (hits.Length > 0)
        {
            // z값 기준으로 정렬 (가장 위에 있는 것부터)
            var sortedHits = hits.OrderByDescending(i => i.collider.transform.position.z).ToArray();

            // 위에 있는 것부터 하나씩 검색
            foreach (var hit in sortedHits)
            {
                // Player 컴포넌트 확인
                if (result.player == null)
                {
                    Player player = hit.collider.GetComponent<Player>();
                    if (player != null)
                    {
                        result.player = player;
                        result.hit = hit;
                    }
                }

                // OverlayTile 컴포넌트 확인
                if (result.tile == null)
                {
                    OverlayTile tile = hit.collider.GetComponent<OverlayTile>();
                    if (tile != null)
                    {
                        result.tile = tile;
                        if (result.player == null) // Player가 없다면 hit 정보 저장
                            result.hit = hit;
                    }
                }

                // 둘 다 찾았다면 break
                if (result.HasPlayer && result.HasTile)
                    break;
            }
        }
        return result;
    }
    #endregion

    #region MouseHover
    private void UpdateCursor(OverlayTile tile)
    {
        //매 프레임마다 마우스 위치 확인 및 처리하는 작업
        if (PlayerState.isMoving == false)
            cursor.transform.position = tile.transform.position; //커서의 위치를 해당 타일로 이동

        cursor.GetComponent<SpriteRenderer>().sortingOrder =
            tile.GetComponent<SpriteRenderer>().sortingOrder + 1; //커서의 렌더링 순서 조절
    }

    private void HandleMouseHover(RaycastResult hit)
    {
        OverlayTile tile = hit.tile; //타일의 정보를 가져옴

        if (tile == null)
            return;

        if (PlayerState.creature == null)
            return;

        //blocking되지 않은 타일위에 마우스를 대면
        if (tile.isBlocked == false)
        {
            HandleHoverOnTile(tile);
        }

        // 현재 가리키는 타일이 플레이어 캐릭터가 있다면
        else
        {
            HandleHoverOnBlockedTile();
        }
    }

    private void HandleHoverOnTile(OverlayTile tile)
    {
        switch (PlayerControlState)
        {
            case EPlayerControlState.ControlledFinish: // 이미 이동 완료한 creature을 조종할 때 
                break;
            case EPlayerControlState.ControllingOffensive:
            case EPlayerControlState.ControllingOrderForMove:
                HandleMovementPreview(tile); // 공격형 이동할때 or 오더가 스폰을 끝내고 움직일때 로직
                break;

            case EPlayerControlState.ControllingOrderForSpawn:
                HandleSpawnPreview(tile); // 오더가 스폰중일때 소환시킬 Playable 캐릭터 실루엣 생성
                break;
        }
    }

    private void HandleMovementPreview(OverlayTile tile)
    {
        // 기존 화살표 제거
        Managers.Controller.tileEffectController.ClearArrows();

        if (PlayerState.rangeFinderTiles.Contains(tile)) // 범위 안 tile hover
        {
            // 현재 위치에서 클릭한 타일까지 경로 계산
            Managers.Controller.tileEffectController.ShowPathToTile(tile);
            // Skill Range Highlights
            // GetSkillRangeTiles(tile);
        }
        else // 범위 밖 tile hover하면
        {
            //// 범위 밖 클릭 시 가장 가까운 범위 내 타일까지만 경로 표시
            //OverlayTile closestTile = GetClosestTileInRange(tile);
            //if (closestTile != null)
            //{
            //    // 가장 가까운 범위 내 타일까지의 경로 계산
            //    ShowPathToTile(closestTile);
            //}
            //// Skill Range Highlights
            //GetSkillRangeTiles(closestTile);
            //path = _pathFinder.FindPath(_creature.currentStandingTile, tile, rangeFinderTiles); // 길 찾기 실패하도록 path에 빈 리스트 리턴받도록 하기
            HandleHoverOnBlockedTile();  // 마치 blocking된 tile 마우스 hover했는것처럼 기능
        }
    }

    private void HandleSpawnPreview(OverlayTile tile) // 소환할때 마우스 tile에 hover상태일때 실행
    {
        // 마우스가 이동 범위 내 타일에 있다면
        if (PlayerState.rangeFinderTiles.Contains(tile))
        {
            // copy가 있다면 위치와 init할거 하기
            Managers.Controller.spawnController.HandleSpawnPreviewCopy(tile);
        }

        else if (PlayerState.rangeFinderTiles.Contains(tile) == false) // 범위 밖 
        {
            Managers.Controller.spawnController.HideCopy();
        }
    }

    private void HandleHoverOnBlockedTile()
    {
        // 화살표 제거 (길찾기 방향 표시 X)
        Managers.Controller.tileEffectController.ClearArrows();
        // 실루엣 안보이게 처리
        Managers.Controller.spawnController.HideCopy();
        // Skill Range 하이라이트 전 타일 원래대로 되돌리기
        //ResetSkillRangeTiles();
    }
    #endregion

    #region MouseClick
    private void HandleMouseClick(RaycastResult hit)
    {
        //마우스 클릭 아니면 무시
        if (Input.GetMouseButtonDown(0) == false)
            return;

        bool changePlayer = false; // 조종할 플레이어가 변경될 때 바꼈는지 판별하는 bool

        // 플레이어 hit됬는지 검사
        if (hit.HasPlayer)
        {
            changePlayer = HandlePlayerClick(hit); // order인지 offensive인지 안에서 검사
        }
        // Order을 클릭해놓고 빈 타일을 누른 경우(스폰할 경우)
        else if (PlayerState.creature != null && PlayerState.creature.PlayerType == EPlayerType.Order && isClickedOrder == true)
        {
            HandleSpawnClick(hit.tile);
        }

        // 범위 밖 타일을 클릭했다면
        else if (PlayerState.creature != null && PlayerState.rangeFinderTiles.Contains(hit.tile) == false)
        {
            CleanupPlayer(); // 현재 플레이어 근처 타일 비활성화
            PlayerState.creature = null; // 조종하는 플레이어 조종 풀기
        }

        // 감지된 캐릭터가 없다면 && 조종할 플레이어를 변경한 경우가 아니면 == 이동할 경우
        if (changePlayer == false && PlayerState.creature != null && CanMove())
        {
            StartMovement(hit.tile);
        }
    }

    private bool HandlePlayerClick(RaycastResult hit) // 플레이어를 클릭한 경우 실행
    {
        Player clickedPlayer = hit.player;

        if (clickedPlayer.PlayerType == EPlayerType.Offensive) // 클릭 된 캐릭터가 Offensive일 경우
        {
            return HandleOffensivePlayerClick(clickedPlayer, hit.tile);
        }
        else if (clickedPlayer.PlayerType == EPlayerType.Order) // 클릭된 캐릭터가 Order일 경우
        {
            return HandleOrderPlayerClick(clickedPlayer, hit.tile);
        }

        return false;
    }

    private bool HandleOffensivePlayerClick(Player player, OverlayTile tile)
    {
        if (isClickedOrder == true) // order 클릭중일땐 클릭해도 효과 X
            return false;

        SwitchToPlayer(player, tile); // 조종하는 캐릭터 변경
        return true;
    }

    private bool HandleOrderPlayerClick(Player player, OverlayTile tile)
    {
        if (isClickedOrder == true)  // 이미 order조종중이라면 효과 X
            return false;

        if (PlayerState.IsSpawnEnd()) // 소환다하고 order를 클릭한다면 
        {
            SwitchToPlayer(player, tile); // 조종하는 캐릭터 변경

            return true;
        }
        else // 스폰할 캐릭터가 남았고 order를 클릭했다면
        {
            // 스폰 모드로 전환
            SwitchToOrderForSpawn.Raise(tile.gameObject);
            PlayerState.creature = player;
            PlayerState.creature.CreatureState = ECreatureState.Idle;
            HighlightSpawnTile.Raise();
            isClickedOrder = true;
            SetCameraTarget(PlayerState.creature);
            return false; // 플레이어는 바뀌었지만 이동하지 않음
        }
    }

    private void SwitchToPlayer(Player newPlayer, OverlayTile tile)
    {
        // 바꾸기 전 플레이어가 있다면
        if (PlayerState.creature != null)
        {
            CleanupPlayer();
        }

        PlayerState.creature = newPlayer;
        PlayerState.creature.CreatureState = ECreatureState.Idle;
        if (PlayerState.creature.IsMoved == false) // 이미 이동한 크리쳐가 아니라면
        {
            GetInRangeTiles(); // 이동 가능한 타일 계산
            ShowRangeTiles.Raise(); // 및 표시
        }
        SetCameraTarget(PlayerState.creature);
        Debug.Log($"Current Playable Character : {PlayerState.creature.name}");
    }



    private void HandleSpawnClick(OverlayTile tile) // Spawn을 위해 실행되는 함수
    {
        if (!PlayerState.rangeFinderTiles.Contains(tile) || tile.isBlocked || PlayerState.IsSpawnable() == false) // 범위 밖 타일에서 생성, blocking tile에서 생성은 X
            return; // 생성할 spawnablePlayer가 없다면 생성 X

        InstantiatePlayerByOrder.Raise(tile.gameObject); // 캐릭터 생성

        if (PlayerState.IsSpawnEnd()) // 이제 다 소환했을 때 처리
        {
            FinishSpawning();
        }
    }

    private void FinishSpawning() // 소환 끝나고 실행되는 함수
    {
        isClickedOrder = false; // 소환시 클릭은 해제
        HideAllRangeTiles.Raise(); // 소환 하이라이트 타일 숨기기
        PlayerState.creature = null; // 조종하던 order 풀기

        despawnCopy.Raise();

        Managers.Turn.Init();
        // 턴 매니저에 턴 시작 알리기 + 턴 종료 버튼 활성화
        AllPlayerSpawn.Raise();
    }

    private void StartMovement(OverlayTile tile)
    {
        if (PlayerControlState == EPlayerControlState.ControlledFinish) // 이미 이동한 크리쳐라면
            return; // 무시

        PlayerState.isMoving = true; // 이동 시작
        tile.HideTile(); // 클릭한 타일 숨김 처리 
        HideAllRangeTiles.Raise(); // 이동 시작하기 직전 이동 범위 표시 지우기

        PlayerState.creature.CreatureState = ECreatureState.Move;
    }
    #endregion

    #region Helper Methods
    private bool CanMove() // 이동할 수 있는 상태인지 State기반 체크
    {
        return PlayerControlState == EPlayerControlState.ControllingOffensive ||
               PlayerControlState == EPlayerControlState.ControllingOrderForMove;
    }

    private void CleanupPlayer() // 조종하던 크리쳐 Clean 함수
    {
        // 바꾸기 전 플레이어가 있다면
        if (PlayerState.creature != null)
        {
            // Copy 비활성화
            despawnCopy.Raise();

            // 바꾸기 전 플레이어 근처 타일 비활성화
            HideAllRangeTiles.Raise();
        }
    }

    private void SetCameraTarget(Player target)
    {
        CameraController camera = Camera.main.GetOrAddComponent<CameraController>();
        camera.Target = target;
    }



    // 범위 밖 타일에서 가장 가까운 범위 내 타일을 찾는 함수
    private OverlayTile GetClosestTileInRange(OverlayTile targetTile)
    {
        if (PlayerState.rangeFinderTiles == null || PlayerState.rangeFinderTiles.Count == 0)
            return null;

        OverlayTile closestTile = null;
        float minDistance = float.MaxValue;

        foreach (var rangeTile in PlayerState.rangeFinderTiles)
        {
            float distance = Vector2.Distance(targetTile.transform.position, rangeTile.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestTile = rangeTile;
            }
        }

        return closestTile;
    }

    //캐릭터 기준으로 이동 가능한 타일 계산 및 표시
    private void GetInRangeTiles()
    {
        // 캐릭터의 현재 위치를 기준으로 이동 가능한 타일 계산
        PlayerState.rangeFinderTiles = Managers.Map.GetTilesInRange(
            new Vector2Int(PlayerState.creature.currentStandingTile.gridLocation.x, PlayerState.creature.currentStandingTile.gridLocation.y),
            PlayerState.creature.MovementRange);

    }

    public void GetSkillRangeTiles(OverlayTile tile)
    {
        //// Skill Range 하이라이트 전 타일 원래대로 되돌리기
        //ResetSkillRangeTiles();

        //// copy가 있다면 위치와 init할거 하기
        //UpdateCopyPosition(tile);

        //// 캐릭터 실루엣 위치를 기준으로 이동 가능한 타일 계산
        //PlayerState.SkillRangeTiles = Managers.Map.GetTilesInRange(
        //    new Vector2Int(PlayerState.creature.currentStandingTile.gridLocation.x, PlayerState.creature.currentStandingTile.gridLocation.y),
        //    PlayerState.creature.SkillRange);

        //// 계산된 타일들을 시각적으로 표시
        //foreach (var item in SkillRangeTiles)
        //{
        //    item.HighlightTileBlue();
        //}
    }
    #endregion

    #region GameEvent오면 실행 함수
    public void EndPlayerEvent() // 턴 종료 버튼 눌릴 때 실행되는 함수(GameEvent로 동작)
    {
        CleanupPlayer(); // 현재 플레이어 근처 타일 비활성화
        PlayerState.creature = null; // 조종하는 플레이어 조종 풀기
    }
    #endregion
}