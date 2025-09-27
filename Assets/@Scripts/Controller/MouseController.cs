using System.Collections.Generic;
using System.Linq;
using System.Net;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.LowLevel;
using UnityEngine.Tilemaps;
using UnityEngine.WSA;
using static Define;
using static UnityEditor.Experimental.GraphView.GraphView;

public class MouseController : InitBase
{
    private List<OverlayTile> path; //타일의 이동 경로 정보 리스트
    private List<OverlayTile> rangeFinderTiles; // 캐릭터 이동 범위
    private List<OverlayTile> SkillRangeTiles; // 캐릭터 공격 범위
    private List<int> spawnablePlayerID; // order가 스폰할 캐릭터들 ID
    private Player _creature; //현재 생성된 캐릭터 정보
    private Player _copy; // 캐릭터의 실루엣 당담
    private PathFinder _pathFinder; //경로 탐색기
    private ArrowTranslator arrowTranslator; // 경로 방향 화살표 계산기
    private bool isMoving; // 캐릭터가 이동 중인지 여부
    private GameObject cursor; // 커서
    private bool isClickedOrder; // Order 캐릭터가 다른 캐릭터 스폰시킬때 클릭된지에 대한 여부

    // Ray 쏴서 결과값 받을 구조체
    public struct RaycastResult
    {
        public OverlayTile tile; // 타일
        public Player player; // 플레이어
        public RaycastHit2D hit;

        public bool HasTile => tile != null;
        public bool HasPlayer => player != null;
    }


    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        _pathFinder = new PathFinder();
        arrowTranslator = new ArrowTranslator();

        path = new List<OverlayTile>();
        rangeFinderTiles = new List<OverlayTile>();
        SkillRangeTiles = new List<OverlayTile>();
        spawnablePlayerID = new List<int>();
        isMoving = false;
        isClickedOrder = false;

        cursor = Managers.Resource.Instantiate("Cursor");
        spawnablePlayerID.Add(HERO_WIZARD_ID);
        spawnablePlayerID.Add(HERO_LION_ID);
        return true;
    }


    void LateUpdate()
    {
        RaycastResult hit = GetFocusedObjects();//마우스가 가리키는 타일 감지
        //타일이 감지 됐다면, 이동중이 아닐때만 감지
        if (hit.HasTile == true && isMoving == false)
        {
            //매 프레임마다 마우스 위치 확인 및 처리하는 작업
            OverlayTile tile = hit.tile; //타일의 정보를 가져옴

            if (tile == null)
                return;

            if (isMoving == false)
                cursor.transform.position = tile.transform.position; //커서의 위치를 해당 타일로 이동

            cursor.gameObject.GetComponent<SpriteRenderer>().sortingOrder
                = tile.transform.GetComponent<SpriteRenderer>().sortingOrder + 1; //커서의 렌더링 순서 조절

            //이동 범위내 타일을 가리키고 있고 캐릭터가 이동 중이 아니라면
            if (!isMoving && tile.isBlocked == false)
            {
                if (_creature == null)
                    return;

                if ((_creature.PlayerType == EPlayerType.Offensive) || (_creature.PlayerType == EPlayerType.Order && isClickedOrder == false)) // 공격형 이동할때 or 오더가 스폰을 끝내고 움직일때 로직
                {
                    // 기존 화살표 제거
                    foreach (var item in rangeFinderTiles)
                    {
                        Managers.Map.mapDict[item.grid2DLocation].SetSprite(ArrowDirection.None);
                    }

                    if (rangeFinderTiles.Contains(tile))
                    {
                        // 현재 위치에서 클릭한 타일까지 경로 계산
                        path = _pathFinder.FindPath(_creature.currentStandingTile, tile, rangeFinderTiles);
                        // 경로 상의 타일에 방향 화살표 설정
                        for (int i = 0; i < path.Count; i++)
                        {
                            var previousTile = i > 0 ? path[i - 1] : _creature.currentStandingTile;
                            var futureTile = i < path.Count - 1 ? path[i + 1] : null;

                            var arrow = arrowTranslator.TranslateDirection(previousTile, path[i], futureTile);
                            path[i].SetSprite(arrow);
                        }

                        // Skill Range Highlights
                        GetSkillRangeTiles(tile);
                    }

                    else
                    {
                        // 범위 밖 클릭 시 가장 가까운 범위 내 타일까지만 경로 표시
                        OverlayTile closestRangeTile = GetClosestTileInRange(tile);
                        if (closestRangeTile != null)
                        {
                            // 가장 가까운 범위 내 타일까지의 경로 계산
                            path = _pathFinder.FindPath(_creature.currentStandingTile, closestRangeTile, rangeFinderTiles);

                            // 경로 상의 타일에 방향 화살표 설정
                            for (int i = 0; i < path.Count; i++)
                            {
                                var previousTile = i > 0 ? path[i - 1] : _creature.currentStandingTile;
                                var futureTile = i < path.Count - 1 ? path[i + 1] : null;

                                var arrow = arrowTranslator.TranslateDirection(previousTile, path[i], futureTile);
                                path[i].SetSprite(arrow);
                            }
                        }
                        // Skill Range Highlights
                        GetSkillRangeTiles(closestRangeTile);
                    }
                }

                else if (_creature.PlayerType == EPlayerType.Order && isClickedOrder == true) // 오더가 스폰중일때 소환시킬 Playable 캐릭터 실루엣 생성
                {
                    if (rangeFinderTiles.Contains(tile))
                    {
                        if (_copy == null)
                            return;

                        // copy가 있다면 위치와 init할거 하기
                        _copy.transform.position = tile.transform.position;
                        _copy.currentStandingTile = tile;
                        _copy.CreatureState = ECreatureState.Skill;
                        _copy.gameObject.SetActive(true);
                    }
                }


            }

            // 현재 가리키는 타일이 플레이어 캐릭터가 있다면
            else if (tile.isBlocked == true)
            {
                // 화살표 제거 (길찾기 방향 표시 X)
                foreach (var item in rangeFinderTiles)
                {
                    Managers.Map.mapDict[item.grid2DLocation].SetSprite(ArrowDirection.None);
                }
                // 실루엣 안보이게 처리
                if (_copy != null)
                    _copy.gameObject.SetActive(false);
                // Skill Range 하이라이트 전 타일 원래대로 되돌리기
                foreach (var skillTile in SkillRangeTiles)
                {
                    if (skillTile.isBlocked == true || rangeFinderTiles.Contains(skillTile) == false)
                        skillTile.HideTile();
                    else
                        skillTile.ShowTile();
                }
            }


            //마우스 왼쪽 클릭시
            if (Input.GetMouseButtonDown(0))
            {
                bool changePlayer = false; // 조종할 플레이어가 변경될 때 바꼈는지 판별하는 bool
                // 플레이어 hit됬는지 검사
                if (hit.HasPlayer == true && hit.player.PlayerType == EPlayerType.Offensive) // Offensive일 경우
                {
                    if (isClickedOrder == true) // order 클릭중일땐 클릭해도 효과 X
                        return;
                    // 바꾸기 전 플레이어가 있다면
                    if (_creature != null)
                    {
                        changePlayer = true;
                        if (_copy != null)
                            Managers.Object.Despawn<Player>(_copy); // 실루엣 전용 플레이어 삭제

                        // 바꾸기 전 플레이어 근처 타일 비활성화
                        foreach (var rangetile in rangeFinderTiles)
                        {
                            rangetile.HideTile();
                            rangetile.SetSprite(ArrowDirection.None);
                        }
                    }

                    _creature = hit.player;
                    _creature.CreatureState = Define.ECreatureState.Idle;
                    _copy = InstantiateCopyPlayer(tile, _creature); // 실루엣 전용 플레이어 재생성
                    GetInRangeTiles(); // 이동 가능한 타일 계산 및 표시
                    CameraController camera = Camera.main.GetOrAddComponent<CameraController>();
                    camera.Target = _creature;

                }

                else if (hit.HasPlayer == true && hit.player.PlayerType == EPlayerType.Order) // Order일 경우
                {
                    if (isClickedOrder == true) // 이미 order조종중이라면
                    {
                        return;
                    }

                    else if (isClickedOrder == false && spawnablePlayerID.Count == 0) // 소환다하고 order를 클릭한다면 
                    {
                        // 이동 로직

                        // 바꾸기 전 플레이어가 있다면
                        if (_creature != null)
                        {
                            changePlayer = true;
                            if (_copy != null)
                                Managers.Object.Despawn<Player>(_copy); // 실루엣 전용 플레이어 삭제

                            // 바꾸기 전 플레이어 근처 타일 비활성화
                            foreach (var rangetile in rangeFinderTiles)
                            {
                                rangetile.HideTile();
                                rangetile.SetSprite(ArrowDirection.None);
                            }
                        }

                        _creature = hit.player;
                        _creature.CreatureState = Define.ECreatureState.Idle;
                        _copy = InstantiateCopyPlayer(tile, _creature); // 실루엣 전용 플레이어 재생성
                        GetInRangeTiles(); // 이동 가능한 타일 계산 및 표시
                        CameraController camera = Camera.main.GetOrAddComponent<CameraController>();
                        camera.Target = _creature;
                    }

                    else if (isClickedOrder == false && spawnablePlayerID.Count > 0) // 스폰할 캐릭터가 남았고 order를 클릭했다면
                    {
                        if (_copy != null)
                            Managers.Object.Despawn<Player>(_copy); // 실루엣 전용 플레이어 삭제

                        _creature = hit.player;
                        _creature.CreatureState = ECreatureState.Idle;

                        _copy = InstantiateCopyPlayer(tile); // 실루엣 캐릭터 생성
                        HighlightSpawnTile();
                        isClickedOrder = true;
                        CameraController camera = Camera.main.GetOrAddComponent<CameraController>();
                        camera.Target = _creature;
                    }



                }

                else if (_creature != null && hit.HasPlayer == false && _creature.PlayerType == EPlayerType.Order) // Order을 클릭해놓고 빈 타일을 누른 경우(스폰할 경우)
                {
                    if (isClickedOrder == true)
                    {
                        if (rangeFinderTiles.Contains(tile) == false || tile.isBlocked == true) // 범위 밖 타일에서 생성, blocking tile에서 생성은 X
                            return;

                        if (spawnablePlayerID.Count <= 0) // 생성할 spawnablePlayer가 없다면 생성 X
                            return;

                        InstantiatePlayerByOrder(_creature, tile); // 캐릭터 생성

                        if (_copy != null)
                            Managers.Object.Despawn<Player>(_copy); // 실루엣 전용 플레이어 삭제

                        _copy = InstantiateCopyPlayer(tile); // 새로운 실루엣 캐릭터 생성



                        if (spawnablePlayerID.Count == 0) // 이제 다 소환했을 때 처리
                        {
                            isClickedOrder = false; // 소환시 클릭은 해제
                            foreach (var item in rangeFinderTiles) // 소환 하이라이트 타일 숨기기
                            {
                                item.HideTile();
                            }
                            _creature = null; // 조종하던 order 풀기
                        }
                    }
                    else
                    {

                    }

                }

                // 감지된 캐릭터가 없다면 && 조종할 플레이어를 변경한 경우가 아니면
                if (changePlayer == false && _creature != null)
                {
                    isMoving = true; // 이동 시작
                    tile.HideTile(); // 클릭한 타일 숨김 처리
                    _creature.CreatureState = ECreatureState.Move;
                }
            }
        }

        // 경로가 존재하고 이동 중이라면 캐릭터 이동 처리
        if (path.Count > 0 && isMoving)
        {
            // 이동할땐 캐릭터 이동 범위 타일 숨기기
            foreach (var tile in rangeFinderTiles)
            {
                tile.HideTile();
            }

            // 캐릭터 스킬 범위 타일도 숨기기
            foreach (var tile in SkillRangeTiles)
            {
                tile.HideTile();
            }

            MoveAlongPath();
        }

        // 경로가 없거나, 이동중이 아닌경우
        else if ((path.Count == 0 || isMoving == false) && _creature != null)
        {
            _creature.CreatureState = ECreatureState.Idle;
            //foreach (var tile in rangeFinderTiles) 왜 넣었었지?
            //{
            //    tile.ShowTile();
            //}
            isMoving = false;
        }
    }

    // 범위 밖 타일에서 가장 가까운 범위 내 타일을 찾는 함수
    private OverlayTile GetClosestTileInRange(OverlayTile targetTile)
    {
        if (rangeFinderTiles == null || rangeFinderTiles.Count == 0)
            return null;

        OverlayTile closestTile = null;
        float minDistance = float.MaxValue;

        foreach (var rangeTile in rangeFinderTiles)
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


    //캐릭터를 경로 따라 이동시키는 함수
    private void MoveAlongPath()
    {
        var current = _creature.SkeletonAnim.AnimationState.GetCurrent(0);
        //Debug.Log($"Current Anim: {current.Animation.Name}, loop:{current.Loop}, trackTime: {current.TrackTime}");

        //프레임 기반으로 이동하도록 계산
        var step = _creature.Speed * Time.deltaTime;

        float zIndex = path[0].transform.position.z;//렌더링 순서용 타일 z값 저장
                                                    //캐릭터를 다음 타일 방향으로 이동
        _creature.transform.position = Vector2.MoveTowards(_creature.transform.position, path[0].transform.position, step);
        //z값 보정
        _creature.transform.position = new Vector3(_creature.transform.position.x, _creature.transform.position.y, zIndex);

        //목표 타일에 거의 다 도착했을 경우
        if (Vector2.Distance(_creature.transform.position, path[0].transform.position) < 0.00001f)
        {
            PositionCharacterOnLine(path[0]);//정확한 위치로 보정
            path.RemoveAt(0);//현재 타일 제거후 다음 타일 경로로 이동하게 함
        }

        // 경로가 끝났다면
        if (path.Count == 0)
        {
            // 기존 화살표 제거
            foreach (var item in rangeFinderTiles)
            {
                Managers.Map.mapDict[item.grid2DLocation].SetSprite(ArrowDirection.None);
            }

            GetInRangeTiles(); // 새로운 이동 범위 계산
            isMoving = false; // 이동 종료
            _creature.CreatureState = ECreatureState.Idle;
        }
    }

    //캐릭터를 타일에 정확한 위치에 보내도록 보정시키는 함수
    private void PositionCharacterOnLine(OverlayTile tile)
    {
        //캐릭터 위치 설정 (약간 뒤로 띄움)
        _creature.transform.position = new Vector3(tile.transform.position.x, tile.transform.position.y + 0.0001f, tile.transform.position.z);
        //렌더링 순서 설정
        //_creature.GetComponent<SpriteRenderer>().sortingOrder = tile.GetComponent<SpriteRenderer>().sortingOrder;
        //캐릭터가 서 있는 타일 저장

        // 전에 서있던 타일에 blocked 해체
        _creature.currentStandingTile.isBlocked = false;
        // 새로 이동한 타일 설정
        _creature.currentStandingTile = tile;
        // 새로 이동한 타일 blocked 설정
        _creature.currentStandingTile.isBlocked = true;
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

    //캐릭터 기준으로 이동 가능한 타일 계산 및 표시
    private void GetInRangeTiles()
    {
        // 캐릭터의 현재 위치를 기준으로 이동 가능한 타일 계산
        rangeFinderTiles = Managers.Map.GetTilesInRange(
            new Vector2Int
            (
                _creature.currentStandingTile.gridLocation.x,
                _creature.currentStandingTile.gridLocation.y
            ),
            _creature.MovementRange);

        // 계산된 타일들을 시각적으로 표시
        foreach (var item in rangeFinderTiles)
        {
            var path = _pathFinder.FindPath(_creature.currentStandingTile, item, rangeFinderTiles);

            if (path.Count == 0)
                item.HideTile();
            else if (path.Count > 0)
                item.ShowTile();
        }
    }

    public void GetSkillRangeTiles(OverlayTile tile)
    {
        // Skill Range 하이라이트 전 타일 원래대로 되돌리기
        foreach (var skillTile in SkillRangeTiles)
        {
            if (skillTile.isBlocked == true || rangeFinderTiles.Contains(skillTile) == false)
                skillTile.HideTile();
            else
                skillTile.ShowTile();
        }

        // copy 있는지 확인
        if (_copy == null)
            return;

        // copy가 있다면 위치와 init할거 하기
        _copy.transform.position = tile.transform.position;
        _copy.currentStandingTile = tile;
        _copy.CreatureState = ECreatureState.Skill;
        _copy.gameObject.SetActive(true);

        // 캐릭터 실루엣 위치를 기준으로 이동 가능한 타일 계산
        SkillRangeTiles = Managers.Map.GetTilesInRange(
            new Vector2Int
            (
                _copy.currentStandingTile.gridLocation.x,
                _copy.currentStandingTile.gridLocation.y
            ),
            _copy.SkillRange);

        // 계산된 타일들을 시각적으로 표시
        foreach (var item in SkillRangeTiles)
        {
            item.HighlightTileBlue();
        }
    }

    // 플레이어 실루엣 생성
    private Player InstantiateCopyPlayer(OverlayTile tile, Player original) // tile위치에 original을 생성
    {
        Player player = Managers.Object.Spawn<Player>(tile.transform.position, original.DataTemplateID);
        player.currentStandingTile = tile;
        // TODO 실루엣 처리
        player.GetComponent<CircleCollider2D>().enabled = false;
        player.CreatureState = ECreatureState.Skill;

        return player;
    }

    // Order의 다른 Playable 캐릭터 실루엣 생성
    private Player InstantiateCopyPlayer(OverlayTile tile) // tile위치에 order가 스폰할 캐릭터 실루엣 생성
    {
        if (spawnablePlayerID.Count == 0)
            return null;

        Player player = Managers.Object.Spawn<Player>(tile.transform.position, spawnablePlayerID.First());
        player.currentStandingTile = tile;
        // TODO 실루엣 처리
        player.GetComponent<CircleCollider2D>().enabled = false;
        player.CreatureState = ECreatureState.Skill;
        player.gameObject.SetActive(false);

        return player;
    }

    // Order의 다른 Playable 캐릭터 생성
    private void InstantiatePlayerByOrder(Player order, OverlayTile tile)
    {
        Player player = Managers.Object.Spawn<Player>(tile.transform.position, spawnablePlayerID.First());
        player.currentStandingTile = tile;
        player.PlayerType = EPlayerType.Offensive; // TODO : 나중에 데이터 시트에 추가해서 SetInfo에서 설정되도록
        player.currentStandingTile.isBlocked = true;
        // TODO : 턴 매니저의 List에도 Add
        spawnablePlayerID.RemoveAt(0);
    }

    // Order 캐릭터 주변 스폰 가능한 타일 하이라이트
    private void HighlightSpawnTile()
    {
        if (_creature.PlayerType == EPlayerType.Order)
        {
            rangeFinderTiles = Managers.Map.GetTilesInRange(
                new Vector2Int
                (
                    _creature.currentStandingTile.gridLocation.x,
                    _creature.currentStandingTile.gridLocation.y
                ),
                2);

            // 계산된 타일들을 시각적으로 표시
            foreach (var tile in rangeFinderTiles)
            {
                tile.HighlightTileRed();
            }
        }

    }

}