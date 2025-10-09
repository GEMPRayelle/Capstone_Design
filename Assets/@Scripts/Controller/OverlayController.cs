using System.Collections.Generic;
using UnityEngine;

public class OverlayController : InitBase
{
    public Dictionary<Color, List<OverlayTile>> coloredTiles;
    //public GameConfig gameConfig;

    //So all the other files don't need the gameConfig.
    public Color AttackRangeColor;
    public Color MoveRangeColor;
    public Color BlockedTileColor;

    //색상 타입 정리
    public enum TileColors
    {
        MovementColor,      // 이동 가능 범위
        AttackRangeColor,   // 공격 범위
        AttackColor         // 공격 대상
    }

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        coloredTiles = new Dictionary<Color, List<OverlayTile>>();

        return true;
    }

    //타일 색상을 제거하는 함수
    public void ClearTiles(Color? color = null)
    {
        //특정 색상만 제거하는 경우
        if (color.HasValue)
        {
            if (coloredTiles.ContainsKey(color.Value))
            {
                var tiles = coloredTiles[color.Value];
                coloredTiles.Remove(color.Value);
                foreach (var coloredTile in tiles)
                {
                    coloredTile.HideTile();
                    
                    foreach (var usedColors in coloredTiles.Keys)
                    {
                        foreach (var usedTile in coloredTiles[usedColors])
                        {
                            if (coloredTile.grid2DLocation == usedTile.grid2DLocation)
                                coloredTile.ShowTile(usedColors);
                        }
                    }
                }
            }
        }
        //모든 색상을 제거하는 경우
        else
        {
            // 모든 타일을 숨기고 Dictionary 초기화
            foreach (var item in coloredTiles.Keys)
            {
                foreach (var colouredTile in coloredTiles[item])
                    colouredTile.HideTile();
            }

            coloredTiles.Clear();
        }
    }

    //Color multiple tile. 
    public void ColorTiles(Color color, List<OverlayTile> overlayTiles)
    {
        ClearTiles(color);// 기존 같은 색상 제거

        foreach (var tile in overlayTiles)
        {
            tile.ShowTile(color);

            //이동할 수 없는 타일은 특별한 색상으로 표시
            if (tile.isBlocked)
                tile.ShowTile(BlockedTileColor);
        }

        coloredTiles.Add(color, overlayTiles);
    }

    //Color only one tile. 
    public void ColorSingleTile(Color color, OverlayTile tile)
    {
        //ClearTiles(color);
        tile.ShowTile(color);

        if (tile.isBlocked)
            tile.ShowTile(BlockedTileColor);


        var list = new List<OverlayTile>();
        list.Add(tile);

        if (!coloredTiles.ContainsKey(color))
            coloredTiles.Add(color, list);
        else
            coloredTiles[color].AddRange(list);

    }
}
