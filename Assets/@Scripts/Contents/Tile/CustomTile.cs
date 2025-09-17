using UnityEngine;
using UnityEngine.Tilemaps;

#if UNITY_EDITOR
using System.IO;
using UnityEditor;
#endif

public class CustomTile : Tile
{
    [Space] 
    [Space]
    [Header("For not programmer")]
    public Define.EObjectType ObjectType;
    public int DataId;
    public string Name;

    //추가정보들
    public bool isStartPos = false;
    public bool isWayPoint = false;
}
