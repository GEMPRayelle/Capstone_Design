using System.Collections.Generic;
using UnityEngine;

public class MapManager
{
    public GameObject Map { get; private set; }
    public string MapName { get; private set; }
    public Grid CellGrid { get; private set; }

    public Dictionary<Vector3Int, BaseObject> _cells = new Dictionary<Vector3Int, BaseObject>();

    public Vector3Int World2Cell(Vector3 worldPos) { return CellGrid.WorldToCell(worldPos); }
    public Vector3 Cell2World(Vector3 cellPos) { return CellGrid.WorldToCell(cellPos); }

    public void LoadMap(string mapName)
    {
        //TODO 맵 로딩시 할 작업들
    }
}
