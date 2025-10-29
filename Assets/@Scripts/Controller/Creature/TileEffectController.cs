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
        foreach (var item in PlayerState.creature.MovementRangeTiles)
        {
            Managers.Map.mapDict[item.grid2DLocation].SetSprite(ArrowDirection.None);
        }
    }
    public void ShowPathToTile(OverlayTile targetTile) // 인자로 들어온 타일까지 길 화살표 표시
    {
        // path 실제 계산
        PlayerState.path = PlayerState._pathFinder.FindPath(PlayerState.creature.currentStandingTile, targetTile, PlayerState.creature.MovementRangeTiles);

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
        foreach (var item in PlayerState.creature.SkillRangeTiles)
        {
            item.HighlightTileBlue();
        }
    }
    #endregion

    #region GameEvent를 통해 실행되는 함수
    public void HideAllRangeTiles() // 이동 범위 내 타일 가리기
    {
        ClearArrows();

        foreach (var tile in PlayerState.creature.MovementRangeTiles)
        {
            tile.HideTile();
            tile.SetSprite(ArrowDirection.None);
        }

        foreach (var tile in PlayerState.creature.SkillRangeTiles)
        {
            tile.HideTile();
            tile.SetSprite(ArrowDirection.None);
        }
    }

    public void HideSkillRangeTiles() // 스킬 범위 타일 가리기
    {
        foreach (var skillTile in PlayerState.creature.SkillRangeTiles)
        {
            skillTile.HideTile();
        }
    }
    public void HideMovementRangeTiles()
    {
        foreach (var moveTile in PlayerState.creature.MovementRangeTiles)
        {
            moveTile.HideTile();
        }
    }

    // Order 캐릭터 주변 스폰 가능한 타일 하이라이트
    public void HighlightSpawnTile() // Order는 처음 이동 타일을 임시적으로 소환타일로 씀
    {
        PlayerState.creature.MovementRangeTiles = Managers.Map.GetTilesInRangeNxN( // GetTilesInRangeNxN에서 range는 3이상 홀수가 되어야 함!!! 
            new Vector2Int(PlayerState.creature.currentStandingTile.gridLocation.x, PlayerState.creature.currentStandingTile.gridLocation.y), 5);

        // 계산된 타일들을 시각적으로 표시
        foreach (var tile in PlayerState.creature.MovementRangeTiles)
        {
            if (tile.isBlocked == true)
                continue;

            tile.HighlightTileRed();
        }

    }

    public void ShowMovementRangeTiles() // 이동 범위 내 타일 중 진짜 이동 가능한 타일 보여주는 함수
    {
        foreach (var item in PlayerState.creature.MovementRangeTiles)
        {
            var path = PlayerState._pathFinder.FindPath(PlayerState.creature.currentStandingTile, item, PlayerState.creature.MovementRangeTiles);

            if (path.Count == 0)
                item.HideTile();
            else if (path.Count > 0)
                item.ShowTile();
        }
    }

    #endregion
}
