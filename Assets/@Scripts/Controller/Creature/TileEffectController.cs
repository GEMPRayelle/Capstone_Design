using UnityEngine;
using UnityEngine.Playables;
using static ControllerManager;
using static Define;

public class TileEffectController : InitBase
{
    private ArrowTranslator arrowTranslator; // 경로 방향 화살표 계산기
    private SharedPlayerState PlayerState; // 공유 데이터

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        PlayerState = Managers.Controller.PlayerState;
        arrowTranslator = new ArrowTranslator();

        return true;
    }

    #region MouseController->Update에서 직접 참조 실행 함수
    public void ClearArrows() // 이동 범위 내 타일들 화살표 방향 초기화
    {
        foreach (var item in PlayerState.rangeFinderTiles)
        {
            Managers.Map.mapDict[item.grid2DLocation].SetSprite(ArrowDirection.None);
        }
    }
    public void ShowPathToTile(OverlayTile targetTile) // 인자로 들어온 타일까지 길 화살표 표시
    {
        // path 실제 계산
        PlayerState.path = PlayerState._pathFinder.FindPath(PlayerState.creature.currentStandingTile, targetTile, PlayerState.rangeFinderTiles);

        // 경로 상의 타일에 방향 화살표 설정
        for (int i = 0; i < PlayerState.path.Count; i++)
        {
            var previousTile = i > 0 ? PlayerState.path[i - 1] : PlayerState.creature.currentStandingTile;
            var futureTile = i < PlayerState.path.Count - 1 ? PlayerState.path[i + 1] : null;
            var arrow = arrowTranslator.TranslateDirection(previousTile, PlayerState.path[i], futureTile);
            PlayerState.path[i].SetSprite(arrow);
        }
    }

    public void ShowSkillRangeTile()
    {
        foreach (var item in PlayerState.SkillRangeTiles)
        {
            item.HighlightTileBlue();
        }
    }
    #endregion

    #region GameEvent를 통해 실행되는 함수
    public void HideAllRangeTiles() // 이동 범위 내 타일 가리기
    {
        foreach (var tile in PlayerState.rangeFinderTiles)
        {
            tile.HideTile();
            tile.SetSprite(ArrowDirection.None);
        }

        foreach (var tile in PlayerState.SkillRangeTiles)
        {
            tile.HideTile();
            tile.SetSprite(ArrowDirection.None);
        }
    }

    public void HideSkillRangeTiles() // 스킬 범위 타일 가리기
    {
        foreach (var skillTile in PlayerState.SkillRangeTiles)
        {
            skillTile.HideTile();
        }
    }




    // Order 캐릭터 주변 스폰 가능한 타일 하이라이트
    public void HighlightSpawnTile()
    {
        PlayerState.rangeFinderTiles = Managers.Map.GetTilesInRangeNxN( // GetTilesInRangeNxN에서 range는 3이상 홀수가 되어야 함!!! 
            new Vector2Int(PlayerState.creature.currentStandingTile.gridLocation.x, PlayerState.creature.currentStandingTile.gridLocation.y), 5);

        // 계산된 타일들을 시각적으로 표시
        foreach (var tile in PlayerState.rangeFinderTiles)
        {
            if (tile.isBlocked == true)
                continue;

            tile.HighlightTileRed();
        }

    }

    public void ShowRangeTiles() // 이동 범위 내 타일 중 진짜 이동 가능한 타일 보여주는 함수
    {
        foreach (var item in PlayerState.rangeFinderTiles)
        {
            var path = PlayerState._pathFinder.FindPath(PlayerState.creature.currentStandingTile, item, PlayerState.rangeFinderTiles);

            if (path.Count == 0)
                item.HideTile();
            else if (path.Count > 0)
                item.ShowTile();
        }
    }
    #endregion
}
