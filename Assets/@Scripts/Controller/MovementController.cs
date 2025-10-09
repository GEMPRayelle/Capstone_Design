using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Define;

public class MovementController : InitBase
{
    public ArrowTranslator arrowTranslator;

    public float speed; //캐릭터의 이동 속도 -> CreatureData의 Speed로 변경해야함
    public Creature activeCharacter; //현재 행동중인 캐릭터
    public bool enableAutoMove; //자동 이동 여부
    public bool showAttackRange; //이동 중 공격범위 표시 여부
    public bool moveThroughAllies = true; //아군 캐릭터가 통과해서 이동할 수 있는지 여부

    //Event
    public GameEvent endTurnEvent; // 턴 종료 시 발생시킬 게임 이벤트
    public GameEventGameObject displayAttackRange; // 공격 범위 표시 이벤트 (GameObject 매개변수)
    public GameEvent actionCompleted; //행동 완료 이벤트
    //public GameEventString cancelActionEvent; //행동 취소 이벤트 (String 매개변수)

    public OverlayController overlayController;
    public MouseController mouseController; //이동을 위한 마우스 클릭 감지

    private PathFinder _pathFinder;
    private ArrowTranslator _arrowTranslator;

    private List<OverlayTile> _path = new List<OverlayTile>(); //현재 계획된 이동 경로
    private List<OverlayTile> inRangeTiles = new List<OverlayTile>(); //현재 캐릭터가 이동 가능한 모든 타일들
    private List<OverlayTile> inAttackRangeTiles = new List<OverlayTile>(); //현재 마우스 커서 위치에서의 공격 범위 타일들
    private OverlayTile _focusedTile; //현재 마우스 커서가 위치한 타일

    private bool _movementModeEnabled = false; //이동 모드가 활성화되었는지 나타내는 플래그
    private bool _isMoving = false; // 현재 캐릭터가 물리적으로 이동 중인지 나타내는 플래그
    private bool _hasMoved = false; // 이번 턴에 이미 이동했는지 나타내는 플래그

    public override bool Init()
    {
        if (base.Init() == false) 
            return false;

        _pathFinder = new PathFinder();        // A* 경로 찾기 시스템
        arrowTranslator = new ArrowTranslator(); // 화살표 방향 결정 시스템

        return true;
    }

    private void Update()
    {
        // 캐릭터 생존 상태 확인 및 정리
        // 활성 캐릭터가 죽었다면 이동 관련 상태를 모두 리셋
        if (activeCharacter && !activeCharacter.IsAlive)
        {
            ResetMovementManager();
        }

        // 실시간 경로 계획 및 화살표 표시 시스템
        if (_focusedTile)
        {
            // 조건 확인:
            // 1. 포커스된 타일이 이동 범위 내에 있음
            // 2. 이동 모드가 활성화됨
            // 3. 현재 이동 중이 아님  
            // 4. 해당 타일이 막혀있지 않음
            if (inRangeTiles.Contains(_focusedTile) && _movementModeEnabled && !_isMoving && !_focusedTile.isBlocked)
            {
                // A* 알고리즘으로 최적 경로 계산
                _path = _pathFinder.FindPath(activeCharacter.currentStandingTile, _focusedTile, inRangeTiles, false, moveThroughAllies);

                // 기존 화살표들을 모두 제거 (깨끗한 화면 유지)
                foreach (var item in inRangeTiles)
                {
                    item.SetArrowSprite(ArrowDirection.None);
                }

                // 새로 계산된 경로에 화살표 표시
                for (int i = 0; i < _path.Count; i++)
                {
                    // 각 타일의 이전, 현재, 다음 위치를 파악
                    var previousTile = i > 0 ? _path[i - 1] : activeCharacter.currentStandingTile;
                    var futureTile = i < _path.Count - 1 ? _path[i + 1] : null;

                    // 3개 타일의 위치 관계를 분석하여 적절한 화살표 방향 결정
                    // 직선: →, ↑ 등의 단순 화살표
                    // 꺾임: ↗, ↘ 등의 방향 전환 화살표
                    var arrowDir = arrowTranslator.TranslateDirection(previousTile, _path[i], futureTile);
                    _path[i].SetArrowSprite(arrowDir);
                }
            }
        }

        // 실제 캐릭터 이동 처리
        // 경로가 있고 이동 중일 때 지속적으로 호출
        if (_path.Count > 0 && _isMoving)
        {
            MoveAlongPath();
        }
    }

    #region Input Management
    /// <summary>
    /// 이동을 위해 좌클릭을 눌렸을 때 호출되는 함수
    /// 계획된 경로에 따라 실제 이동을 시작함
    /// 
    /// 처리 과정:
    /// 1. 이동 조건 확인 (모드 활성화, 경로 존재)
    /// 2. 이동 상태로 전환 (isMoving = true)
    /// 3. 화면 정리 (타일 색상 제거)
    /// 4. 이니셔티브 업데이트 (턴 순서 계산)
    /// 5. UI 버튼 비활성화 (중복 실행 방지)
    /// </summary>
    public void ActionButtonPressed()
    {
        if (_movementModeEnabled && _path.Count > 0)
        {
            _isMoving = true; // 이동 상태로 전환

            // 모든 타일의 색상과 화살표 제거 (깔끔한 화면)
            overlayController.ClearTiles(null);

            // 캐릭터의 이니셔티브(행동 순서) 업데이트
            // 이동 비용만큼 다음 턴이 늦어짐
            //activeCharacter.UpdateInitiative(Constants.MoveCost);

            // 이동 버튼 비활성화 (중복 클릭 방지)
            //if (MoveButton)
            //    MoveButton.GetComponent<Button>().interactable = false;
        }
    }

    /// <summary>
    /// 이동 취소 버튼이 눌렸을 때 호출되는 메서드
    /// 현재 계획 중인 이동을 취소하고 이전 상태로 되돌림
    /// 
    /// 처리 과정:
    /// 1. 취소 이벤트 발생 ("Move" 액션 취소 알림)
    /// 2. 이동 관련 상태 모두 리셋
    /// 3. UI 화면 정리
    /// </summary>
    public void CancelButtonPressed()
    {
        if (_movementModeEnabled)
        {
            // 다른 시스템에 이동 취소를 알림
            //cancelActionEvent.Raise("Move");

            // 모든 이동 관련 상태 초기화
            ResetMovementManager();
        }
    }
    #endregion

    #region Movement State Management
    /// <summary>
    /// 이동 관련 모든 상태를 초기화하는 메서드
    /// 이동 완료, 취소, 또는 오류 상황에서 호출되어 깨끗한 상태로 복원
    /// 
    /// 초기화 대상:
    /// - 이동 모드 해제
    /// - 이동 중 상태 해제  
    /// - 화면 색상 정리
    /// - 캐릭터 이동 완료 처리
    /// - 경로 데이터 초기화
    /// </summary>
    public void ResetMovementManager()
    {
        _movementModeEnabled = false;  // 이동 모드 해제
        _isMoving = false;            // 이동 중 상태 해제

        // 모든 타일의 색상과 화살표 제거
        overlayController.ClearTiles(null);

        // 캐릭터에게 이동 완료를 알림 (애니메이션, 상태 업데이트 등)
        activeCharacter.CharacterMoved();

        // 경로 데이터 초기화
        _path = new List<OverlayTile>();
    }

    /// <summary>
    /// 계획된 경로를 따라 캐릭터를 실제로 이동시키는 메서드
    /// 매 프레임 호출되어 부드러운 이동 애니메이션을 구현
    /// 
    /// 동작 원리:
    /// 1. 현재 위치에서 다음 타일로 일정 속도로 이동
    /// 2. 타일에 도착하면 경로에서 해당 타일 제거
    /// 3. 마지막 타일 도착 시 캐릭터를 해당 타일에 고정
    /// 4. 모든 이동 완료 시 후처리 실행
    /// </summary>
    private void MoveAlongPath()
    {
        // 프레임 기반 이동 거리 계산 (프레임률에 무관하게 일정한 속도)
        var step = speed * Time.deltaTime;

        // 다음 목표 타일의 Z 좌표 (높이) 가져오기
        var zIndex = _path[0].transform.position.z;

        // Vector3.MoveTowards로 부드러운 이동 구현
        // 현재 위치에서 목표 위치로 step 거리만큼 이동
        activeCharacter.transform.position = Vector3.MoveTowards(
            activeCharacter.transform.position,
            _path[0].transform.position,
            step);

        // 목표 타일에 충분히 가까워졌는지 확인 (부동소수점 오차 고려)
        if (Vector3.Distance(activeCharacter.transform.position, _path[0].transform.position) < 0.0001f)
        {
            // 마지막 타일인 경우 캐릭터를 정확한 위치에 배치
            if (_path.Count == 1)
                PositionCharacterOnTile(activeCharacter, _path[0]);

            // 도착한 타일을 경로에서 제거
            _path.RemoveAt(0);
        }

        // 모든 이동이 완료된 경우
        if (_path.Count == 0)
        {
            // 이동 관련 상태 초기화
            ResetMovementManager();
            _hasMoved = true; // 이번 턴에 이동했다고 표시

            // 턴 시스템에 행동 완료를 알림 (CTB 등에서 중요)
            if (actionCompleted)
                actionCompleted.Raise();

            // 자동 이동 모드인 경우 턴 자동 종료
            if (enableAutoMove)
            {
                if (endTurnEvent)
                    endTurnEvent.Raise(); // 턴 종료 이벤트 발생
                else
                    SetActiveCharacter(activeCharacter.gameObject); // 다음 캐릭터로 전환
            }
        }
    }
    #endregion

    #region Helper Func
    /// <summary>
    /// 현재 캐릭터의 이동 가능 범위를 계산하고 시각화하는 메서드
    /// 캐릭터의 이동력 스탯을 기반으로 도달 가능한 모든 타일을 찾음
    /// 
    /// 처리 과정:
    /// 1. OverlayController에서 이동 범위 색상 가져오기
    /// 2. 캐릭터와 현재 위치 유효성 확인
    /// 3. MapManager로 이동 가능한 모든 타일 계산
    /// 4. 해당 타일들을 지정된 색상으로 시각화
    /// </summary>
    private void GetInRangeTiles()
    {
        // 이동 범위 표시에 사용할 색상 (주로 파란색)
        var moveColor = overlayController.MoveRangeColor;

        if (activeCharacter && activeCharacter.currentStandingTile)
        {
            // RangeFinder를 사용하여 이동 가능한 모든 타일 계산
            // 매개변수:
            // - 시작 타일: 캐릭터의 현재 위치
            // - 범위: 캐릭터의 이동력 스탯
            // - ignoreObstacles: false (장애물 고려함)
            // - walkThroughAllies: 아군 통과 설정값 사용
            inRangeTiles = Managers.Map.GetTilesInRange(
                activeCharacter.currentStandingTile,
                activeCharacter.MovementRange,
                false,
                moveThroughAllies);

            // 계산된 타일들을 지정된 색상으로 시각화
            overlayController.ColorTiles(moveColor, inRangeTiles);
        }
    }

    /// <summary>
    /// 캐릭터를 특정 타일에 정확하게 배치하는 메서드
    /// 이동 완료나 스폰 시 캐릭터와 타일을 연결하는 역할
    /// </summary>
    /// <param name="character">배치할 캐릭터</param>
    /// <param name="tile">배치할 타일</param>
    public void PositionCharacterOnTile(Creature character, OverlayTile tile)
    {
        if (tile != null)
        {
            // 캐릭터 위치를 타일 위치에 맞춤 (약간 위로 배치)
            character.transform.position = new Vector3(
                tile.transform.position.x,
                tile.transform.position.y + 0.0001f,  // Z-fighting 방지
                tile.transform.position.z);

            // 캐릭터와 타일 간의 게임 로직 연결
            // 이는 다른 시스템들이 "어떤 캐릭터가 어느 타일에 있는지" 알 수 있게 함
            // TODO -> 필요시 MapManager를 통해 구현
            //character.LinkCharacterToTile(tile);
        }
    }
    #endregion

    #region AI System
    /// <summary>
    /// AI나 다른 시스템에서 캐릭터 이동을 명령할 때 사용하는 메서드
    /// 플레이어 입력 대신 프로그래밍적으로 이동 경로를 지정
    /// 
    /// 사용 시나리오:
    /// - AI 캐릭터의 자동 이동
    /// - 스크립트 이벤트나 컷씬
    /// - 리플레이 시스템
    /// 
    /// 처리 과정:
    /// 1. 이동 상태로 전환  
    /// 2. GameObject 리스트를 OverlayTile 리스트로 변환
    /// </summary>
    /// <param name="pathToFollow">이동할 경로 (GameObject 리스트)</param>
    public void MoveCharacterCommand(List<GameObject> pathToFollow)
    {
        if (activeCharacter)
        {
            _isMoving = true; // 이동 상태로 전환

            // 이동 비용만큼 이니셔티브 업데이트 -> 이동 비용은 아직 쓸 생각 없음
            //activeCharacter.UpdateInitiative(Constants.MoveCost);

            // GameObject 리스트를 OverlayTile 컴포넌트로 변환
            // LINQ Select를 사용한 함수형 프로그래밍 스타일
            if (pathToFollow.Count > 0)
                _path = pathToFollow.Select(x => x.GetComponent<OverlayTile>()).ToList();
        }
    }

    /// <summary>
    /// 마우스가 새로운 타일에 포커스되었을 때 호출되는 메서드
    /// 실시간 상호작용과 공격 범위 미리보기 기능을 제공
    /// 
    /// 처리 과정:
    /// 1. 이동 중이 아닐 때만 포커스 업데이트
    /// 2. 조건 확인 후 공격 범위 표시 이벤트 발생
    /// 
    /// 공격 범위 표시 조건:
    /// - 이동 모드 활성화
    /// - 포커스된 타일이 이동 범위 내
    /// - 이동 중이 아님
    /// - 공격 범위 표시 옵션 활성화
    /// </summary>
    /// <param name="focusedOnTile">포커스된 타일 GameObject</param>
    public void FocusedOnNewTile(GameObject focusedOnTile)
    {
        // 이동 중이 아닐 때만 포커스 업데이트
        if (!_isMoving)
            _focusedTile = focusedOnTile.GetComponent<OverlayTile>();

        // 공격 범위 표시 조건 확인
        // LINQ Where와 Any를 사용한 효율적인 검색
        if (_movementModeEnabled &&
            inRangeTiles.Where(x => x.grid2DLocation == _focusedTile.grid2DLocation).Any() &&
            !_isMoving &&
            showAttackRange &&
            displayAttackRange)

            // 공격 범위 표시 이벤트 발생
            displayAttackRange.Raise(focusedOnTile);
    }

    /// <summary>
    /// 특정 위치에서의 공격 범위를 계산하고 시각화하는 메서드
    /// 이동 계획 단계에서 "이 위치로 이동하면 어디를 공격할 수 있는가"를 보여줌
    /// 
    /// 처리 과정:
    /// 1. 공격 범위 색상 가져오기
    /// 2. 지정된 위치를 중심으로 공격 범위 계산
    /// 3. 해당 타일들을 시각화
    /// 
    /// 특이사항:
    /// - ignoreObstacles = true: 공격은 벽을 넘어서도 가능할 수 있음
    /// </summary>
    /// <param name="focusedOnTile">공격 범위 계산 기준 타일</param>
    public void ShowAttackRangeTiles(GameObject focusedOnTile)
    {
        // 공격 범위 표시 색상 (주로 주황색이나 빨간색)
        var attackColor = overlayController.AttackRangeColor;

        // 지정된 위치에서의 공격 범위 계산
        // ignoreObstacles = true: 공격은 장애물을 넘어서 가능할 수 있음
        inAttackRangeTiles = Managers.Map.GetTilesInRange(
            focusedOnTile.GetComponent<OverlayTile>(),
            (int)activeCharacter.AttackDistance,
            true,  // 공격은 장애물 무시
            moveThroughAllies);

        // 공격 범위 타일들을 시각화
        overlayController.ColorTiles(attackColor, inAttackRangeTiles);
    }
    #endregion

    #region Character Management
    /// <summary>
    /// 새로운 캐릭터를 활성화하고 필요시 자동으로 이동 모드를 시작하는 메서드
    /// 턴이 바뀔 때마다 호출되어 다음 행동할 캐릭터를 설정
    /// 
    /// 처리 과정:
    /// 1. 활성 캐릭터 설정
    /// 2. 이동 상태 초기화 (새 턴이므로)
    /// 3. 자동 이동 모드인 경우 즉시 이동 모드 활성화
    /// </summary>
    /// <param name="character">새로 활성화할 캐릭터</param>
    public void SetActiveCharacter(GameObject character)
    {
        activeCharacter = character.GetComponent<Creature>();
        _hasMoved = false; // 새 턴이므로 이동 상태 초기화

        // 자동 이동 모드이고 캐릭터가 살아있으면 즉시 이동 모드 시작
        if (enableAutoMove && activeCharacter.IsAlive)
            StartCoroutine(DelayedMovementmode());
    }

    /// <summary>
    /// 다음 프레임에 이동 모드를 활성화하는 코루틴
    /// 레이스 컨디션(경쟁 상태) 방지를 위해 한 프레임 지연
    /// 
    /// 필요한 이유:
    /// - 캐릭터 설정과 UI 업데이트가 동시에 일어날 때의 충돌 방지
    /// - 다른 시스템들이 캐릭터 변경을 완전히 처리할 시간 제공
    /// </summary>
    IEnumerator DelayedMovementmode()
    {
        yield return new WaitForFixedUpdate(); // 다음 물리 업데이트까지 대기
        InitiateMovementMode(); // 이동 모드 시작
    }

    /// <summary>
    /// 캐릭터 스폰 시 특정 타일에 배치하는 메서드
    /// 게임 시작이나 캐릭터 소환 시 사용
    /// </summary>
    /// <param name="newCharacter">스폰할 캐릭터</param>
    public void SpawnCharacter(GameObject newCharacter)
    {
        PositionCharacterOnTile(newCharacter.GetComponent<Creature>(), _focusedTile);
    }

    /// <summary>
    /// 외부에서 이동 모드 시작을 요청할 때 호출하는 공개 메서드
    /// UI 버튼 클릭 등에서 사용
    /// </summary>
    public void StartMovementMode()
    {
        StartCoroutine(DelayedMovementmode());
    }

    /// <summary>
    /// 실제로 이동 모드를 시작하는 내부 메서드
    /// 이미 이동하지 않은 경우에만 실행
    /// 
    /// 처리 과정:
    /// 1. 이동 여부 확인
    /// 2. 이동 가능 범위 계산 및 표시
    /// 3. 이동 모드 활성화
    /// 4. 행동 미리보기 이벤트 발생
    /// </summary>
    private void InitiateMovementMode()
    {
        if (!_hasMoved) // 아직 이번 턴에 이동하지 않은 경우에만
        {
            GetInRangeTiles();          // 이동 범위 계산 및 시각화
            _movementModeEnabled = true; // 이동 모드 활성화

            // UI에 이동 비용 미리보기 표시
            //if (previewAction)
            //    previewAction.Raise(Constants.MoveCost);
        }
    }
    #endregion
}
