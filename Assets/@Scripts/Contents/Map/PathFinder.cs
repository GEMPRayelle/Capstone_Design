using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PathFinder
{
    private Dictionary<Vector2Int, OverlayTile> searchableTiles;

    public List<OverlayTile> FindPath(OverlayTile start, OverlayTile end, List<OverlayTile> inRangeTiles)
    {
        searchableTiles = new Dictionary<Vector2Int, OverlayTile>();

        //탐색할 후보 타일들 목록
        List<OverlayTile> openList = new List<OverlayTile>();
        //이미 탐색한 타일들 목록
        HashSet<OverlayTile> closedList = new HashSet<OverlayTile>();


        // 범위 내 클릭
        if (inRangeTiles.Count > 0 && inRangeTiles.Contains(end) == true)
        {
            foreach (var item in inRangeTiles)
            {
                searchableTiles.Add(item.grid2DLocation, Managers.Map.mapDict[item.grid2DLocation]);
            }
        }

        // 범위 밖 클릭
        if (inRangeTiles.Contains(end) == false && inRangeTiles.Count > 0)
        {
            // MouseController에서 처리
            return new List<OverlayTile>();
        }

        //시작 타일을 후보 목록에 추가
        openList.Add(start);

        //후보 타일이 타일이 남아있는 동안 계속 반복
        while (openList.Count > 0)
        {
            //F가 가장 낮은 타일 선택
            OverlayTile currentOverlayTile = openList.OrderBy(x => x.F).First();

            openList.Remove(currentOverlayTile); //후보 목록에서 제거후
            closedList.Add(currentOverlayTile); //탐색 완료 목록에 추가함

            //목적지에 도달했으면
            if (currentOverlayTile == end)
            {
                //경로 리턴
                return GetFinishedList(start, end);
            }

            //인접 타일 탐색
            foreach (var tile in GetNeightbourOverlayTiles(currentOverlayTile))
            {
                //이동 불가능한 타일이거나 이미 탐색했거나 z축 차이가 너무 크면 무시함
                if (tile.isBlocked || closedList.Contains(tile) ||
                    Mathf.Abs(currentOverlayTile.transform.position.z - tile.transform.position.z) > 1)
                {
                    continue;
                }

                //시작점에서 현재 타일까지의 거리
                tile.G = GetManhattenDistance(start, tile);
                //현태 타일에서 목적지까지의 거리
                tile.H = GetManhattenDistance(end, tile);
                //경로 추적을 위한 이전 타일 설정
                tile.previousTile = currentOverlayTile;

                //아직 후보 목록에 없으면 추가
                if (!openList.Contains(tile))
                {
                    openList.Add(tile);
                }
            }
        }

        //경로를 찾지 못했으면 빈 리스트를 리턴시킴
        return new List<OverlayTile>();
    }

    //end부터 start까지 다시 거슬러 올라가며 경로를 구성하는 함수
    private List<OverlayTile> GetFinishedList(OverlayTile start, OverlayTile end)
    {
        //최종 경로를 저장할 리스트
        List<OverlayTile> finishedList = new List<OverlayTile>();
        OverlayTile currentTile = end; //현재 타일을 목적지 타일로 초기화

        //시작 타일에 도달할때까지 반복
        while (currentTile != start)
        {
            //현재 타일을 경로 리스트에 추가
            finishedList.Add(currentTile);
            //현재 타일의 이전 타일로 이동(경로 추적)
            currentTile = currentTile.previousTile;
        }

        //start 타일은 루프에서 제외되므로 직접 추가하지 않음

        //경로가 목적지부터 시작점 순서로 되어있으므로 리스트를 뒤집음            
        finishedList.Reverse();

        //완성된 경로 리턴
        return finishedList;
    }

    //거리 계산 함수 (휴리스틱 계산)
    private int GetManhattenDistance(OverlayTile start, OverlayTile tile)
    {
        //맨해튼 거리 사용 (Grid기반 맵에서 가장 일반적인 휴리스틱 방식, 대각선 이동이 없을때 유리)
        return Mathf.Abs(start.gridLocation.x - tile.gridLocation.x) + Mathf.Abs(start.gridLocation.y - tile.gridLocation.y);
    }

    //타일로부터 인접한 타일을 리턴하는 함수
    private List<OverlayTile> GetNeightbourOverlayTiles(OverlayTile currentOverlayTile)
    {
        //전체 타일 맵 <Vec2Int, OverlayTile>로 세팅된 딕셔너리
        var map = Managers.Map.mapDict;
        //인접한 타일들을 저장할 리스트 생성
        List<OverlayTile> neighbours = new List<OverlayTile>();

        //현재 위치한 타일로부터 상하좌우 타일을 가져옴

        //right
        Vector2Int locationToCheck = new Vector2Int(currentOverlayTile.gridLocation.x + 1, currentOverlayTile.gridLocation.y);
        //맵에 해당하는 위치의 타일이 존재하면
        if (searchableTiles.ContainsKey(locationToCheck))
        {
            //인접 리스트에 추가
            neighbours.Add(searchableTiles[locationToCheck]);
        }

        //left
        locationToCheck = new Vector2Int(currentOverlayTile.gridLocation.x - 1, currentOverlayTile.gridLocation.y);
        if (searchableTiles.ContainsKey(locationToCheck))
        {
            neighbours.Add(searchableTiles[locationToCheck]);
        }

        //top
        locationToCheck = new Vector2Int(currentOverlayTile.gridLocation.x, currentOverlayTile.gridLocation.y + 1);
        if (searchableTiles.ContainsKey(locationToCheck))
        {
            neighbours.Add(searchableTiles[locationToCheck]);
        }

        //bottom
        locationToCheck = new Vector2Int(currentOverlayTile.gridLocation.x, currentOverlayTile.gridLocation.y - 1);
        if (searchableTiles.ContainsKey(locationToCheck))
        {
            neighbours.Add(searchableTiles[locationToCheck]);
        }

        //상하좌우 인접 타일 리스트 리턴
        return neighbours;
    }
}
