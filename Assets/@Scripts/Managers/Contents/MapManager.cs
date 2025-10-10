using NUnit;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.VisualScripting;
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

        MouseController = Managers.Resource.Instantiate("MouseController");

        //타일 위치와 OverlayTile을 저장할 딕셔너리 초기화
        mapDict = new Dictionary<Vector2Int, OverlayTile>();

        Map = map;
        MapName = mapName;
        CellGrid = map.GetComponent<Grid>();

        ParseCollisionData(map, mapName);

        // _collision 배열을 순회하면서 OverlayTile 생성
        int xCount = _MaxX - _MinX + 1;
        int yCount = _MaxY - _MinY + 1;

        for (int y = 0; y < yCount; y++)
        {
            for (int x = 0; x < xCount; x++)
            {
                //해당 위치에 타일이 존재하는지 확인 (None == 벽이 None)
                if (_collision[x, y] == ECellCollisionType.None)
                {
                    // 배열 인덱스를 Grid 좌표로 변환
                    // y는 읽을 때 역순 변환 필요(역순으로 저장되서)
                    int GridX = _MinX + x;
                    int GridY = _MaxY - y; // y축 역순 변환
                    Vector2Int GridPos = new Vector2Int(GridX, GridY);

                    //해당 위치에 OverlayTile이 없으면 생성
                    if (!mapDict.ContainsKey(GridPos))
                    {
                        //OverlayTile 프리팹을 생성하고 부모 컨테이너에 배치
                        var overlayTile = Managers.Object.SpawnTileObject("OverlayTile", Managers.Object.OverlayTileRoot);
                        OverlayTile tile = overlayTile.GetComponent<OverlayTile>();
                        Managers.Object.OverlayTiles.Add(tile);

                        // Grid 좌표를 실제 월드 좌표로 변환
                        Vector3 worldPosition = CellGrid.GetCellCenterWorld(new Vector3Int(GridX, GridY, 0));

                        //OverlayTile 위치 설정 (Grid좌표에서 변환된 월드 좌표 사용)
                        overlayTile.transform.position = new Vector3(worldPosition.x, worldPosition.y, 0);

                        //OverlayTile의 렌더링 순서 설정, 테스트 위해 맨 위로
                        overlayTile.GetComponent<SpriteRenderer>().sortingOrder = 11;

                        //OverlayTile의 그리드 위치 정보 저장 (그리드 좌표)
                        overlayTile.gameObject.GetComponent<OverlayTile>().gridLocation = new Vector3Int(GridX, GridY, 0);

                        //딕셔너리에 그리드 좌표와 OverlayTile을 매핑해 저장
                        mapDict.Add(GridPos, tile);
                    }
                }
            }
        }

        SpawnObjectByTileData(map, mapName);
        Managers.Object.InstantiateListener();
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

    void SpawnObjectByTileData(GameObject map, string mapName, string tileMap = "Tilemap_Object")
    {
        Tilemap tm = Util.FindChild<Tilemap>(map, tileMap, true);

        if (tm != null)
            tm.gameObject.SetActive(false);

        for (int y = tm.cellBounds.yMax; y >= tm.cellBounds.yMin; y--)
        {
            for (int x = tm.cellBounds.xMin; x <= tm.cellBounds.xMax; x++)
            {
                //맵을 전부 순회해서 CustomTile이 있는지 탐색
                Vector3Int cellPos = new Vector3Int(x, y, 0);
                CustomTile tile = tm.GetTile(cellPos) as CustomTile;

                if (tile == null)
                    continue;

                Grid grid = tm.GetComponent<Grid>();
                //존재한다면 오브젝트 타입에 따라 각 로직 실행
                if (tile.ObjectType == Define.EObjectType.Env)
                {
                    //Vector3 worldPos = Cell2World(cellPos);
                    //Env env = Managers.Object.Spawn<Env>(worldPos, tile.DataId);
                    //env.SetCellPos(cellPos, true);
                }
                else
                {
                    if (tile.ObjectType == Define.EObjectType.Monster)
                    {
                        Vector3 worldPos = grid.GetCellCenterWorld(cellPos);
                        Monster monster = Managers.Object.Spawn<Monster>(worldPos, tile.DataId);
                        mapDict.TryGetValue(new Vector2Int(cellPos.x,cellPos.y), out monster.currentStandingTile);
                        monster.currentStandingTile.isBlocked = true;
                        monster.SetCellPos(cellPos, grid, true);
                    }

                    else if(tile.ObjectType == Define.EObjectType.Player)
                    {
                        Vector3 worldPos = grid.GetCellCenterWorld(cellPos);
                        Player player = Managers.Object.Spawn<Player>(worldPos, tile.DataId);
                        mapDict.TryGetValue(new Vector2Int(cellPos.x, cellPos.y), out player.currentStandingTile);
                        player.currentStandingTile.isBlocked = true;
                        player.SetCellPos(cellPos, grid, true);
                        player.PlayerType = EPlayerType.Order; // Order 생성
                        CameraController camera = Camera.main.GetOrAddComponent<CameraController>();
                        camera.Target = player;

                    }
                    else if (tile.ObjectType == Define.EObjectType.Npc)
                    {

                    }
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
            if (Mathf.Abs(mapDict[TileToCheck].transform.position.z - mapDict[originTile].transform.position.z) <= 1 && mapDict[TileToCheck].isBlocked == false)
                surroundingTiles.Add(mapDict[TileToCheck]);
        }

        //왼쪽 타일 확인
        TileToCheck = new Vector2Int(originTile.x - 1, originTile.y);
        if (mapDict.ContainsKey(TileToCheck))
        {
            if (Mathf.Abs(mapDict[TileToCheck].transform.position.z - mapDict[originTile].transform.position.z) <= 1 && mapDict[TileToCheck].isBlocked == false)
                surroundingTiles.Add(mapDict[TileToCheck]);
        }

        //위쪽 타일 확인
        TileToCheck = new Vector2Int(originTile.x, originTile.y + 1);
        if (mapDict.ContainsKey(TileToCheck))
        {
            if (Mathf.Abs(mapDict[TileToCheck].transform.position.z - mapDict[originTile].transform.position.z) <= 1 && mapDict[TileToCheck].isBlocked == false)
                surroundingTiles.Add(mapDict[TileToCheck]);
        }

        //아래쪽 타일 확인
        TileToCheck = new Vector2Int(originTile.x, originTile.y - 1);
        if (mapDict.ContainsKey(TileToCheck))
        {
            if (Mathf.Abs(mapDict[TileToCheck].transform.position.z - mapDict[originTile].transform.position.z) <= 1 && mapDict[TileToCheck].isBlocked == false)
                surroundingTiles.Add(mapDict[TileToCheck]);
        }

        //조건을 만족하는 인접 타일 리스트 반환
        return surroundingTiles;
    }

    #region Helper Func
    /// <summary>
    /// 주어진 시작 타일에서 지정된 범위 내의 모든 이동 가능한 타일을 반환합니다.
    /// 
    /// 알고리즘:
    /// 1. 시작점을 중심으로 BFS 방식으로 확산
    /// 2. 각 단계마다 이웃 타일들을 탐색
    /// 3. 이동 비용과 높이 차이를 계산하여 실제 이동 가능성 판단
    /// 4. 남은 이동력이 0 이상인 타일만 결과에 포함
    /// 
    /// 사용 예시:
    /// - 플레이어 캐릭터의 이동 범위 시각화
    /// - AI의 이동 가능 위치 계산
    /// - 스킬 범위 계산 (장애물 무시 옵션 활용)
    /// </summary>
    /// <param name="startingTile">탐색을 시작할 기준 타일 (캐릭터의 현재 위치)</param>
    /// <param name="range">최대 이동 범위 (캐릭터의 이동력)</param>
    /// <param name="ignoreObstacles">장애물을 무시할지 여부 (스킬이나 특수 능력에서 사용)</param>
    /// <param name="walkThroughAllies">아군 캐릭터를 통과할 수 있는지 여부</param>
    /// <returns>이동 가능한 모든 타일들의 리스트 (중복 제거됨)</returns>
    public List<OverlayTile> GetTilesInRange(OverlayTile startingTile, int range, 
        bool ignoreObstacle = false, bool walkThroughAllies = true)
    {
        //var startingTile = mapDict[location]; // 시작 위치의 타일 가져오기
        startingTile.remainingMovement = range; // 시작 타일의 남은 이동력을 최대 범위로 설정
        var inRangeTiles = new List<OverlayTile>(); // 최종 결과 리스트 : 이동 가능한 모든 타일
        int stepCount = 0; // 현재 이동 거리 단계 (0 to range)

        inRangeTiles.Add(startingTile); // 시작 타일은 항상 포함 (자기 자신도 "이동 가능한" 위치)

        //BFS탐색 준비
        var tilesForPreviousStep = new List<OverlayTile> { startingTile }; //이전 단계에서 확장된 타일들을 저장할 리스트

        //범위만큼 반복하며 주변 타일 확장
        while (stepCount < range) //지정된 이동 거리만큼 반복
        {
            var surroundingTiles = new List<OverlayTile>(); // 이번 단계에서 확장될 타일들

            foreach (var item in tilesForPreviousStep) // 이전 단계의 모든 타일에 대해
            {
                int moveCost = !ignoreObstacle ? item.GetMoveCost() : 1;

                var newNeighbours = Managers.Map.GetNeighbourTiles(item,
                    new List<OverlayTile>(), ignoreObstacle, walkThroughAllies, item.remainingMovement);

                foreach(var tile in newNeighbours)
                {
                    //높이 차이로 인한 추가 이동 비용 계산
                    int heightDifference = CalculateHeightCost(ignoreObstacle, item, tile);

                    //이 타일에 도달했을때의 남은 이동력 계산
                    // 공식: 현재 타일의 남은 이동력 - 목표 타일의 기본 비용 - 높이 비용
                    var remainingMovement = item.remainingMovement - tile.GetMoveCost() - heightDifference;

                    if (remainingMovement > tile.remainingMovement)
                        tile.remainingMovement = remainingMovement;
                }

                //남은 이동력이 0 이상인 타일만 현재 단계 결과에 추가
                surroundingTiles.AddRange(newNeighbours.Where(x => x.remainingMovement >= 0).ToList());
                // 해당 타일의 상하좌우 인접 타일들을 가져와서 리스트에 추가
                //surroundingTiles.AddRange(GetSurroundingTiles(new Vector2Int(item.gridLocation.x, item.gridLocation.y)));
            }

            // 현재까지 수집된 타일들에 이번 단계 타일들을 추가
            inRangeTiles.AddRange(surroundingTiles);
            // 다음 단계의 기준이 될 타일들을 중복 제거 후 저장
            tilesForPreviousStep = surroundingTiles.Distinct().ToList();

            // 단계 증가
            stepCount++;
        }

        foreach (var item in inRangeTiles)
        {
            item.remainingMovement = 0;
        }

        // 최종적으로 수집된 타일들을 중복 제거 후 반환
        // Distinct(): 같은 타일이 여러 번 추가되는 것을 방지
        return inRangeTiles.Distinct().ToList();
    }

    /// <summary>
    /// 높이(z) 차이로 인한 추가 이동 비용을 계산하는 함수
    /// </summary>
    /// <param name="ignoreObstacle">장애물 무시 여부</param>
    /// <param name="item">start Position Tile</param>
    /// <param name="tile">dest Position Tile</param>
    /// <returns></returns>
    private int CalculateHeightCost(bool ignoreObstacle, OverlayTile item, OverlayTile tile)
    {
        //dest와 start의 높이 차이 ( 양수: 올라가는 것, 음수: 내려가는 것)
        int heightDifference = tile.gridLocation.z - item.gridLocation.z;
        
        // 최종 높이 비용 결정:
        // 1. 장애물 무시 모드(!ignoreObstacles = false)면 높이 비용도 0
        // 2. 올라가는 경우(heightDifference > 0)만 비용 발생
        // 3. 내려가거나 같은 높이면 비용 0
        heightDifference = !ignoreObstacle && heightDifference > 0 ? heightDifference : 0;
        return heightDifference;
    }

    /// <summary>
    /// 주어진 타일 주변의 모든 이웃 타일을 리턴하는 함수
    /// 
    /// * 여러조건을 고려하여 접근 가능한 타일만 반환:
    /// - 장애물 무시 여부
    /// - 아군 통과 가능 여부  
    /// - 남은 이동 범위
    /// - 높이 차이 제한
    /// </summary>
    /// <returns>접근 가능한 이웃 타일들의 리스트</returns>
    public List<OverlayTile> GetNeighbourTiles(OverlayTile targetObjectTile, 
        List<OverlayTile> searchableTiles,
        bool ignoreObstacle = false, 
        bool walkThroughAllies = true, 
        int remainRange = 10)
    {
        //탐색할 타일 범위 설정용 딕셔너리
        Dictionary<Vector2Int, OverlayTile> tileToSearch = new Dictionary<Vector2Int, OverlayTile>();

        if(searchableTiles.Count > 0)
        {
            //특정 범위만 검색하는 경우 (이동 범위 내에서)
            foreach (var tile in searchableTiles)
            {
                tileToSearch.Add(tile.grid2DLocation, tile);
            }
        }
        else
        {
            //전체 맵을 검색하는 경우
            tileToSearch = mapDict;
        }

        List<OverlayTile> neighbours = new List<OverlayTile>();

        if (targetObjectTile != null) 
        {
            //상하좌우로 이웃 타일 검색
            foreach (var direction in Util.GetDirection())
            {
                Vector2Int locationToCheck = targetObjectTile.grid2DLocation + direction;

                //각 방향의 타일이 접근 가능한지 검증
                
            }
        }

        return neighbours;
    }

    //Neighbour 타일이 접근 가능한지 검증하는 함수
    private void ValidateNeighbour(OverlayTile currentOverlayTile, 
        bool ignoreObstacles,
        bool walkThroughAllies,
        Dictionary<Vector2Int, OverlayTile> tilesToSearch,
        List<OverlayTile> neighbours,
        Vector2Int locationToCheck,
        int remainingRange)
    {
        bool canAccessLocation = false;

        //검사할 위치에 타일이 존재하는지
        if (tilesToSearch.ContainsKey(locationToCheck))
        {
            OverlayTile tile = tilesToSearch[locationToCheck];

            bool isBlocked = tile.isBlocked;
            bool isActiveCharacter = //tile.activeCharacter != null  &&
                Managers.Turn.activeCharacter != null;
            bool canWalkThroughAllies = walkThroughAllies;

            //접근 가능 조건 판단
            if (ignoreObstacles ||                  // 장애물 무시 모드이거나
                (!ignoreObstacles && !isBlocked) || // 장애물이 없거나
                canWalkThroughAllies)               // 아군 통과 가능한 경우
            {
                //이동 cost 검사(남은 이동 cost 코스트로 갈 수 있는가?)
                //TODO 
                //if ( <= remainingRange || ignoreObstacles)
                //{
                //    canAccessLocation = true;
                //}
            }

            //최종 접근 가능한 판단 및 높이(z) 제한 검사
            if (canAccessLocation)
            {
                //인공적인 점프 높이 제한 : 1칸 높이 차이까지만 이동 가능
                //캐릭터가 너무 높은 절벽을 오르내리지 못하도록 제한
                if (Mathf.Abs(currentOverlayTile.gridLocation.z - tile.gridLocation.z) <= 1)
                    neighbours.Add(tilesToSearch[locationToCheck]);
            }
        }
    }

    /// <summary>
    /// 지정된 위치를 중심으로 하는 N×N 그리드의 타일들을 반환하는 함수
    /// </summary>
    /// <param name="location">중심이 될 위치</param>
    /// <param name="gridSize">그리드 크기 (3 이상의 홀수만 허용)</param>
    /// <returns>N×N 그리드 범위 내의 타일 리스트</returns>
    public List<OverlayTile> GetTilesInRangeNxN(Vector2Int location, int gridSize)
    {
        // 유효성 검사: 3 이상의 홀수만 허용
        if (gridSize < 3 || gridSize % 2 == 0)
        {
            Debug.LogError($"GetTilesInGrid: gridSize는 3 이상의 홀수여야 합니다. 입력된 값: {gridSize}");
            return new List<OverlayTile>();
        }

        var gridTiles = new List<OverlayTile>();

        // 중심으로부터의 반경 계산 (예: 3x3이면 반경 1, 5x5면 반경 2)
        int radius = gridSize / 2;

        // 그리드 범위 계산
        int startX = location.x - radius;
        int endX = location.x + radius;
        int startY = location.y - radius;
        int endY = location.y + radius;

        // N×N 그리드 범위의 모든 타일을 확인
        for (int y = startY; y <= endY; y++)
        {
            for (int x = startX; x <= endX; x++)
            {
                Vector2Int tilePos = new Vector2Int(x, y);

                // 해당 위치에 타일이 존재하는지 확인
                if (mapDict.ContainsKey(tilePos))
                {
                    gridTiles.Add(mapDict[tilePos]);
                }
            }
        }

        return gridTiles;
    }
    public void ClearObjects() { _cells.Clear(); }
    #endregion
}
