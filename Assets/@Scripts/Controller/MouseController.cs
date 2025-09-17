using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;
using UnityEngine.WSA;
using static Define;

public class MouseController : InitBase
{
    private List<OverlayTile> path; //타일의 이동 경로 정보 리스트
    private List<OverlayTile> rangeFinderTiles;
    private Player _creature; //현재 생성된 캐릭터 정보
    private PathFinder _pathFinder; //경로 탐색기
    private ArrowTranslator arrowTranslator; // 경로 방향 화살표 계산기
    private bool isMoving; // 캐릭터가 이동 중인지 여부
    private GameObject cursor;

    public override bool Init()
    {
        if (base.Init() == false) 
            return false;

        _pathFinder = new PathFinder();
        arrowTranslator = new ArrowTranslator();

        path = new List<OverlayTile>();
        rangeFinderTiles = new List<OverlayTile>();
        isMoving = false;

        cursor = Managers.Resource.Instantiate("Cursor");
        return true;
    }

    void LateUpdate()
    {
        RaycastHit2D? hit = GetFocusedOnTile();//마우스가 가리키는 타일 감지
        //타일이 감지 됐다면, 이동중이 아닐때만 감지
        if (hit.HasValue && isMoving == false)
        {
            //매 프레임마다 마우스 위치 확인 및 처리하는 작업
            OverlayTile tile = hit.Value.collider.gameObject.GetComponent<OverlayTile>(); //타일의 정보를 가져옴

            if (tile == null)
                return;

            if (isMoving == false)
                cursor.transform.position = tile.transform.position; //커서의 위치를 해당 타일로 이동

            cursor.gameObject.GetComponent<SpriteRenderer>().sortingOrder
                = tile.transform.GetComponent<SpriteRenderer>().sortingOrder + 1; //커서의 렌더링 순서 조절

            //이동 범위내 타일을 가리키고 있고 캐릭터가 이동 중이 아니라면
            if (!isMoving && _creature != null)
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
                }
            }


            //마우스 왼쪽 클릭시
            if (Input.GetMouseButtonDown(0))
            {
                // tile.ShowTile(); // 타일 시각적으로 표시

                //캐릭터가 생성되지 않았다면
                if (_creature == null)
                {
                    int heroTemplateID = HERO_WIZARD_ID;
                    //_creature = Instantiate(characterPrefab).GetComponent<CharacterInfo>(); // 캐릭터 생성
                    _creature = Managers.Object.Spawn<Player>(tile.transform.position, heroTemplateID); //Addressable에 등록된 characterPrefab으로 수정, Hero여도 상관없음
                    _creature.CreatureState = Define.ECreatureState.Idle;
                    PositionCharacterOnLine(tile); // 캐릭터 위치 설정
                    GetInRangeTiles(); // 이동 가능한 타일 계산 및 표시
                    CameraController camera = Camera.main.GetOrAddComponent<CameraController>();
                    camera.Target = _creature;
                }

                // 이미 캐릭터가 있다면
                else
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
            foreach (var tile in rangeFinderTiles)
            {
                tile.HideTile();
            }
            MoveAlongPath();
        }

        // 경로가 없거나, 이동중이 아닌경우
        else if ((path.Count == 0 || isMoving == false) && _creature != null)
        {
            _creature.CreatureState = ECreatureState.Idle;
            foreach (var tile in rangeFinderTiles)
            {
                tile.ShowTile();
            }
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
        Debug.Log($"Current Anim: {current.Animation.Name}, loop:{current.Loop}, trackTime: {current.TrackTime}");
        _creature.CreatureState = ECreatureState.Move;

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
        _creature.currentStandingTile = tile;
    }

    //마우스가 가리키는 타일을 감지하는 함수
    private static RaycastHit2D? GetFocusedOnTile()
    {
        //마우스 위치를 월드 좌표로 변환
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        //2D 좌표로 변환
        Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);
        //해당 위치에 있는 모든 콜라이더 감지
        RaycastHit2D[] hits = Physics2D.RaycastAll(mousePos2D, Vector2.zero);
        //마우스가 가리키고 있는 위치에 감지된 오브젝트가 
        if (hits.Length > 0)
        {
                //z값 기준으로 가장 위에 있는 타일 반환
                return hits.OrderByDescending(i => i.collider.transform.position.z).First();
        }
        //아무것도 감지 되지 않으면 null리턴
        return null;
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
            _creature.movementRange);

        // 계산된 타일들을 시각적으로 표시
        foreach (var item in rangeFinderTiles)
        {
            item.ShowTile();
        }
    }
}
