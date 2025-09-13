using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using static Define;

public class MapManager
{
    public GameObject Map { get; private set; }
    public string MapName { get; private set; }
    public Grid CellGrid { get; private set; }

    public GameObject MouseController { get; private set; }

    public Dictionary<Vector3Int, BaseObject> _cells = new Dictionary<Vector3Int, BaseObject>();

    public Vector3Int World2Cell(Vector3 worldPos) { return CellGrid.WorldToCell(worldPos); }
    public Vector3 Cell2World(Vector3 cellPos) { return CellGrid.WorldToCell(cellPos); }

    public Dictionary<Vector2Int, OverlayTile> mapDict; //타일 위치와 OverlayTile을 매핑해서 저장

    ECellCollisionType[,] _collision; //충돌 정보를 구분하여 처리할 2차원 배열

    //맵에서 갈 수 있는 영역, x,y를 벗어난 위치로는 갈 수 없음
    private int _MinX;
    private int _MaxX;
    private int _MinY;
    private int _MaxY;

    public void LoadMap(string mapName)
    {
        DestroyMap(); //다른 맵으로 넘어갈때 기존 맵 삭제

        //맵 생성
        GameObject map = Managers.Resource.Instantiate(mapName);
        map.transform.position = Vector3.zero;
        map.name = $"@Map_{mapName}";

        #region Refactor
        //자식 오브젝트들 중 TileMap컴포넌트를 모두 가져오고
        //렌더링 순서 기준으로 정렬

        //TODO 맵 로딩시 할 작업들
        //var tileMaps = Map.transform.GetComponentsInChildren<Tilemap>().
        //    OrderByDescending(x => x.GetComponent<TilemapRenderer>().sortingOrder);

        MouseController = Managers.Resource.Instantiate("MouseController");
        Transform forest = map.gameObject.GetComponentInChildren<Transform>();
        var tileMaps = forest.GetComponentsInChildren<Tilemap>()
            .Where(x => x.name == "Terrain_tile")
            .OrderByDescending(x => x.GetComponent<TilemapRenderer>().sortingOrder);
        //타일 위치와 OverlayTile을 저장할 딕셔너리 초기화
        mapDict = new Dictionary<Vector2Int, OverlayTile>();

        // tile root 생성
        GameObject tileRoot = Managers.Resource.Instantiate("@TileRoot");

        //TileMap의 모든 셀을 순회
        foreach (var tm in tileMaps)
        {
            //TileMap의 셀 범위 가져오기
            BoundsInt bounds = tm.cellBounds;

            //z축부터 역순으로 순회 (렌더링 순서 고려)
            for (int z = bounds.max.z - 1; z >= bounds.min.z; z--)
            {
                for (int y = bounds.min.y; y < bounds.max.y; y++)
                {
                    for (int x = bounds.min.x; x < bounds.max.x; x++)
                    {
                        //해당 위치에 타일이 존재하는지 확인
                        if (tm.HasTile(new Vector3Int(x, y, z)))
                        {
                            //해당 위치에 OverlayTile이 없으면 생성
                            if (!mapDict.ContainsKey(new Vector2Int(x, y)))
                            {
                                //타일의 월드 좌표 중심 위치 계산
                                var cellWorldPosition = tm.GetCellCenterWorld(new Vector3Int(x, y, z + 1));

                                //OverlayTile 프리팹을 생성하고 부모 컨테이너에 배치
                                var overlayTile = Managers.Object.SpawnTileObject("OverlayTile", tileRoot.transform);

                                //OverlayTile 위치 설정 (Z값 + 1로 살짝 띄워서 렌더링 우선순위 확보)
                                overlayTile.transform.position = new Vector3(cellWorldPosition.x, cellWorldPosition.y, cellWorldPosition.z + 1);
                                //OverlayTile의 렌더링 순서를 타일맵과 동일하게 설정, 테스트를 위해 2로 임의로 설정
                                overlayTile.GetComponent<SpriteRenderer>().sortingOrder = 2; // tm.GetComponent<TilemapRenderer>().sortingOrder;
                                //OverlayTile의 그리드 위치 정보 저장
                                overlayTile.gameObject.GetComponent<OverlayTile>().gridLocation = new Vector3Int(x, y, z);

                                //딕셔너리에 위치와 OverlayTile을 매핑해 저장
                                mapDict.Add(new Vector2Int(x, y), overlayTile.gameObject.GetComponent<OverlayTile>());
                            }
                        }
                    }
                }
            }
        }
        #endregion

        Map = map;
        MapName = mapName;
        CellGrid = map.GetComponent<Grid>();

        ParseCollisionData(map, mapName);
    }

    public void DestroyMap()
    {
        ClearObjects();//모든 오브젝트들 제거

        if (Map != null)
            Managers.Resource.Destroy(Map);
    }
    
    //collision txt데이터를 파싱해서 그 정보를 찾아 2차원 배열에 기입할 함수
    void ParseCollisionData(GameObject map, string mapName, string tilemap = "Tilemap_Collision")
    {
        //collision이라고 되어있는 자식들을 찾음
        GameObject collision = Util.FindChild(map, tilemap, true);
        //Collision 타일들이 실제로 보이면 안되기에 비활성화
        if (collision != null) 
            collision.SetActive(false);

        //Collision 관련 파일을 읽어옴
        TextAsset txt = Managers.Resource.Load<TextAsset>($"{mapName}Collision");
        StringReader reader = new StringReader(txt.text);

        //데이터 파일들을 파싱
        _MinX = int.Parse(reader.ReadLine());
        _MaxX = int.Parse(reader.ReadLine());
        _MinY = int.Parse(reader.ReadLine());
        _MaxY = int.Parse(reader.ReadLine());

        int xCount = _MaxX - _MinX + 1;
        int yCount = _MaxY - _MinY + 1;
        _collision = new ECellCollisionType[xCount, yCount];

        for (int y = 0; y < yCount; y++)
        {
            string line = reader.ReadLine();
            for (int x = 0; x < xCount; x++)
            {
                switch (line[x])
                {
                    case Define.MAP_TOOL_WALL:
                        _collision[x, y] = ECellCollisionType.Wall;
                        break;
                    case Define.MAP_TOOL_NONE:
                        _collision[x, y] = ECellCollisionType.None;
                        break;
                    case Define.MAP_TOOL_SEMI_WALL:
                        _collision[x, y] |= ECellCollisionType.SemiWall;
                        break;
                }
            }
        }
    }

    //주어진 타일을 기준으로 상하좌우 인접한 타일들을 찾아 반환하는 함수
    //z값의 차이를 기준으로 높이 차이가 너무 크면 제외하는 로직이 포함됨
    public List<OverlayTile> GetSurroundingTiles(Vector2Int originTile)
    {
        //조건을 만족하는 인접 타일들을 저장하는 리스트
        var surroundingTiles = new List<OverlayTile>();

        //오른쪽 타일 확인
        Vector2Int TileToCheck = new Vector2Int(originTile.x + 1, originTile.y); // 오른쪽 타일 좌표 계산
        if (mapDict.ContainsKey(TileToCheck)) // 해당 위치에 타일이 존재하는지 확인
        {
            // 높이 차이가 1 이하일 경우만 추가 (지형 이동 가능 조건)
            if (Mathf.Abs(mapDict[TileToCheck].transform.position.z - mapDict[originTile].transform.position.z) <= 1)
                surroundingTiles.Add(mapDict[TileToCheck]);
        }

        //왼쪽 타일 확인
        TileToCheck = new Vector2Int(originTile.x - 1, originTile.y);
        if (mapDict.ContainsKey(TileToCheck))
        {
            if (Mathf.Abs(mapDict[TileToCheck].transform.position.z - mapDict[originTile].transform.position.z) <= 1)
                surroundingTiles.Add(mapDict[TileToCheck]);
        }

        //위쪽 타일 확인
        TileToCheck = new Vector2Int(originTile.x, originTile.y + 1);
        if (mapDict.ContainsKey(TileToCheck))
        {
            if (Mathf.Abs(mapDict[TileToCheck].transform.position.z - mapDict[originTile].transform.position.z) <= 1)
                surroundingTiles.Add(mapDict[TileToCheck]);
        }

        //아래쪽 타일 확인
        TileToCheck = new Vector2Int(originTile.x, originTile.y - 1);
        if (mapDict.ContainsKey(TileToCheck))
        {
            if (Mathf.Abs(mapDict[TileToCheck].transform.position.z - mapDict[originTile].transform.position.z) <= 1)
                surroundingTiles.Add(mapDict[TileToCheck]);
        }

        //조건을 만족하는 인접 타일 리스트 반환
        return surroundingTiles;
    }

    #region Helper Func
    //BFS 방식으로 타일을 확장하면서 지정된 이동 거리 만큼 주변 타일을 수집하는 함수
    //<param name="location">캐릭터가 서 있는 위치</param>
    //<param name="range">이동 가능한 거리</param>
    //<returns>이동 가능한 범위 내의 타일 리스트</returns>
    public List<OverlayTile> GetTilesInRange(Vector2Int location, int range)
    {
        //var startingTile = Managers.Instance.Map.map[location]; // 시작 위치의 타일 가져오기
        var startingTile = mapDict[location]; // 시작 위치의 타일 가져오기
        var inRangeTiles = new List<OverlayTile>(); // 최종 결과 리스트
        int stepCount = 0; // 현재 이동 거리 단계

        inRangeTiles.Add(startingTile); // 시작 타일은 항상 포함

        //BFS탐색 준비
        var tilesForPreviousStep = new List<OverlayTile>(); //이전 단계에서 확장된 타일들을 저장할 리스트
        tilesForPreviousStep.Add(startingTile); // 첫 단계는 시작 타일

        //범위만큼 반복하며 주변 타일 확장
        while (stepCount < range) //지정된 이동 거리만큼 반복
        {
            var surroundingTiles = new List<OverlayTile>(); // 이번 단계에서 확장될 타일들

            foreach (var item in tilesForPreviousStep) // 이전 단계의 모든 타일에 대해
            {
                // 해당 타일의 상하좌우 인접 타일들을 가져와서 리스트에 추가
                surroundingTiles.AddRange(GetSurroundingTiles(new Vector2Int(item.gridLocation.x, item.gridLocation.y)));
            }

            // 현재까지 수집된 타일들에 이번 단계 타일들을 추가
            inRangeTiles.AddRange(surroundingTiles);
            // 다음 단계의 기준이 될 타일들을 중복 제거 후 저장
            tilesForPreviousStep = surroundingTiles.Distinct().ToList();

            // 단계 증가
            stepCount++;
        }

        // 최종적으로 수집된 타일들을 중복 제거 후 반환
        return inRangeTiles.Distinct().ToList();
    }
    public void ClearObjects() { _cells.Clear(); }
    #endregion
}
