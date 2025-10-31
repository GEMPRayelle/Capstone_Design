using System.Linq;
using UnityEngine;
using static ControllerManager;
using static Define;

public class PlayerMouseController : InitBase
{
    private GameObject cursor; // 커서
    private SharedPlayerState PlayerState; // Controller들이 공유하는 데이터

    // 직접 호출을 위해 들고 있는 컨트롤러
    SpawnController _spawnController;
    TileEffectController _tileEffectController;
    PlayerMovementController _playerMovementController;

    // GameEvent
    GameEvent AllPlayerSpawn; // MouseController에서 스폰 끝나면 버튼 활성화. MouseController -> UI_GameScene

    // MouseController을 통해 소환되는 팝업창들
    UI_ActPopup actPopUp;
    UI_MovementPopup MovementPopUp;

    public EPlayerControlState PlayerControlState = EPlayerControlState.None;


    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        PlayerState = Managers.Controller.PlayerState;
        cursor = Managers.Resource.Instantiate("Cursor");

        AllPlayerSpawn = Managers.Resource.Load<GameEvent>("AllPlayerSpawn");

        return true;
    }

    public void StartSpawnMode() // 스폰 모드로 시작하는 함수
    {
        PlayerControlState = EPlayerControlState.Spawn;
        PlayerState.creature = Managers.Turn.activePlayerList.First<Creature>().GetComponent<Player>();
        _tileEffectController.HighlightSpawnTile();
        _spawnController.SwitchToOrderForSpawn(PlayerState.creature.currentStandingTile.gameObject);
        Managers.Controller.cameraController.SetCameraTarget(PlayerState.creature.gameObject);
    }

    public void SetOtherController()
    {
        _spawnController = Managers.Controller.spawnController;
        _tileEffectController = Managers.Controller.tileEffectController;
        _playerMovementController = Managers.Controller.playerMovementController;
    }

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



    void LateUpdate()
    {
        RaycastResult hit = GetFocusedObjects(); // 마우스가 가리키는 타일 감지
        //타일이 감지 됐다면, 이동중이 아닐때만 감지
        if (hit.HasTile && !PlayerState.isMoving)
        {
            UpdateCursor(hit.tile);
            UpdateHover(hit);
            UpdateClick(hit);
        }


    }
    #endregion

    private void UpdateCursor(OverlayTile tile)
    {
        //매 프레임마다 마우스 위치 확인 및 처리하는 작업
        if (PlayerState.isMoving == false)
            cursor.transform.position = tile.transform.position; //커서의 위치를 해당 타일로 이동

        cursor.GetComponent<SpriteRenderer>().sortingOrder =
            tile.GetComponent<SpriteRenderer>().sortingOrder + 1; //커서의 렌더링 순서 조절
    }

    private void UpdateHover(RaycastResult hit)
    {
        switch(PlayerControlState)
        {
            case EPlayerControlState.None:
                break;
            case EPlayerControlState.Move:
                // Move Preview, Skill Preview(Copy위치 따라)
                MovePreview(hit);
                SkillPreview(hit);
                break;
            case EPlayerControlState.Spawn:
                // Spawn Preview
                SpawnPreview(hit);
                break;
            case EPlayerControlState.Skill:
                // Skill Preview
                SkillPreview(hit);
                break;
            case EPlayerControlState.UI:
                break;
        }
    }

    private void MovePreview(RaycastResult hit) // 이동상태일 때 이동에 관한 표시
    {
        if (PlayerState.creature.MovementRangeTiles.Contains(hit.tile)) // 이동 범위거나 isBlocked가 false인 타일들 위에 마우스가 있다면
        {
            _tileEffectController.ShowMovementRangeTiles();
            _spawnController.UpdateCopyPosition(hit.tile);
        }

        else
        {
            //_tileEffectController.HideMovementRangeTiles();
            _spawnController.HideCopy();
        }
                
    }

    private void SkillPreview(RaycastResult hit)
    {
        switch(PlayerControlState)
        {
            case EPlayerControlState.Move:
                {
                    // 공통으로 수행되는 부분, 이전에 그려졌던것들 초기화
                    _tileEffectController.ClearArrows(); // 화살표 이동 전 표시 끄기
                    _tileEffectController.HideSkillRangeTiles(); // 스킬 범위 이동 전 표시 끄기
                    if (PlayerState.creature.MovementRangeTiles.Contains(hit.tile)) // 이동 범위 내 타일에 마우스가 있다면
                    {
                        PlayerState.creature.GetSkillRangeTilesCopy(); // Copy위치 기준 스킬 범위 구해서
                        _tileEffectController.ShowSkillRangeTile(); // 표시
                        _tileEffectController.ShowPathToTile(hit.tile); // 화살표로 방향 표시
                    }

                    else // 이동 범위 밖이면
                    {
                        _tileEffectController.ShowMovementRangeTiles(); // 스킬 범위와 이동 범위가 겹치는 부위는 지워졌으니 다시 그리기
                    }
                }
                break;
            case EPlayerControlState.Skill:
                {
                    
                }
                break;
        }
        
        
    }

    private void SpawnPreview(RaycastResult hit) // 스폰 모드일 경우 미리보기
    {
        if (PlayerState.creature.MovementRangeTiles.Contains(hit.tile) && hit.tile.isBlocked == false) // 소환 범위 내라면
            _spawnController.HandleSpawnPreviewCopy(hit.tile); // Copy 위치 업데이트

        else
            _spawnController.HideCopy(); // 소환 범위 밖이면 Copy 숨기기
    }


    private void UpdateClick(RaycastResult hit)
    {
        if (!Input.GetMouseButtonDown(0)) // 마우스 클릭이 없는 경우 무시
        {
            return; 
        }

        // State 변경 및 변경에 따른 실행
        if (hit.HasPlayer && PlayerControlState != EPlayerControlState.Spawn && PlayerControlState != EPlayerControlState.UI) // 플레이어 변경의 경우(스폰, UI 사용 중일 때는 클릭감지 X)
        {
            // Change Player?
            CleanUpPlayer(PlayerState.creature); // 변경 전 플레이어 정보 초기화
            PlayerState.creature = hit.player;   // 새로운 플레이어로 변경
            Managers.Controller.cameraController.SetCameraTarget(PlayerState.creature.gameObject);
            ShowActPopUp();                      // 팝업 켜지기

        }
        
        else if (!hit.HasPlayer && hit.HasTile) // 이동, 스폰의 경우
        {
            // 스폰
            if (PlayerControlState == EPlayerControlState.Spawn) // 스폰모드 경우
            {
                if (PlayerState.creature.MovementRangeTiles.Contains(hit.tile) && hit.tile.isBlocked == false) // 소환 범위 내 타일 클릭 시
                    _spawnController.InstantiatePlayerByOrder(hit.tile.gameObject); // 실제 스폰

                if (PlayerState.IsSpawnEnd()) // 스폰 끝나면
                {
                    PlayerControlState = EPlayerControlState.None; // None 모드로 변경(아무 동작 X)
                    _tileEffectController.HideAllRangeTiles(); // 소환 범위 타일 숨기기
                    PlayerState.creature.ResetMovementRangeTiles(); // 소환 범위 타일 초기화
                    PlayerState.creature = null; // 조종 풀기
                    Managers.Turn.Init();
                    AllPlayerSpawn.Raise();
                }
            }

            // 이동
            else if(PlayerControlState == EPlayerControlState.Move)
            {
                if (PlayerState.creature.MovementRangeTiles.Contains(hit.tile)) // 이동 범위 안을 클릭했다면
                    ShowMovementPopUp(hit.tile); // 이동 팝업 뜨기
                else // 클릭한 타일이 이동 범위 밖이라면 
                {
                    CancelMovement();
                }


            }
        }
    }
    #region Helper Function
    private void ShowActPopUp()
    {
        if (actPopUp != null)
            return;

        PlayerControlState = EPlayerControlState.UI;
        actPopUp = Managers.UI.ShowPopupUI<UI_ActPopup>();
        GameObject PopUp = Util.FindChild(actPopUp.gameObject, "Popup");
        //PopUp.GetComponent<RectTransform>().anchoredPosition = new Vector3(800, 800, 1); 위치 조정 필요
        
        if (PlayerState.creature.IsMoved == true) // 이미 이동한 크리쳐라면
        {
            // Move 버튼 블러 처리
            GameObject MoveBtn = Util.FindChild(PopUp, "Button_Move", true);
            MoveBtn.GetComponent<UnityEngine.UI.Image>().raycastTarget = false;
            MoveBtn.GetComponent<UnityEngine.UI.Image>().color = Color.red;
        }
    }

    private void CleanUpPlayer(Creature player) // 해당 플레이어 초기화
    {
        if (player == null)
            return;

        _spawnController.DespawnCopy(); // 플레이어 Copy 삭제
        _tileEffectController.HideAllRangeTiles(); // 플레이어 근처 타일 안보이게 하기
        player.ResetMovementRangeTiles(); // 플레이어 이동 가능 타일들 Reset
        player.ResetSkillRangeTiles(); // 플레이어 스킬 범위 타일들 Reset
    }

    private void ShowMovementPopUp(OverlayTile tile)
    {
        if (MovementPopUp != null)
            return;

        PlayerControlState = EPlayerControlState.UI;
        MovementPopUp = Managers.UI.ShowPopupUI<UI_MovementPopup>();
        GameObject PopUp = Util.FindChild(MovementPopUp.gameObject, "Popup");
        PopUp.GetComponent<RectTransform>().position = Camera.main.WorldToScreenPoint(tile.transform.position);


    }
    #endregion

    #region Event
    public void StartMovement() // MovementPopUp에서 Move누르면 실행되는 함수
    {
        PlayerControlState = EPlayerControlState.None;
        _tileEffectController.HideAllRangeTiles(); // 이동하기전 범위 안보이게 처리
        _spawnController.DespawnCopy(); // copy 삭제(이제 보일 필요가 없기 때문)
        PlayerState.isMoving = true;
        PlayerState.creature.CreatureState = ECreatureState.Move;
    }

    public void CancelMovement()
    {
        PlayerControlState = EPlayerControlState.None;
        CleanUpPlayer(PlayerState.creature); // 이동,스킬 범위같은것 초기화
        PlayerState.creature = null; // 조종 풀기
    }

    public void EndPlayerEvent() // 턴 종료 버튼 눌릴 때 실행되는 함수(GameEvent로 동작)
    {
        PlayerControlState = EPlayerControlState.None;
        PlayerState.CleanupAllPlayer();

        if (PlayerState.creature != null)
        {
            PlayerState.creature = null; // 조종하는 플레이어 조종 풀기
        }

        if (actPopUp != null)
            Managers.UI.ClosePopupUI(actPopUp);

        if (MovementPopUp != null)
            Managers.UI.ClosePopupUI(MovementPopUp);
    }
    #endregion


}