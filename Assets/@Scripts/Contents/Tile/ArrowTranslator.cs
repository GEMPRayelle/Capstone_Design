using UnityEngine;
using static Define;

public class ArrowTranslator
{
    public ArrowDirection ArrowDirection;

    //경로 상의 타일 3개 (이전, 현재, 다음)를 비교해서 방향을 계산하고 그에 맞는 ArrowDirection 리턴
    public ArrowDirection TranslateDirection(OverlayTile previousTile, OverlayTile currentTile, OverlayTile futureTile)
    {
        bool isFinal = futureTile == null; //다음 타일이 없으면 마지막 타일임

        //방향 벡터 계산 : pastDirection과 futureDirection이 다르면 꺾이는 지점이므로, 두 벡터를 더해서 대각선 방향을 유추

        // 이전 타일 → 현재 타일 방향 벡터
        Vector2Int pastDirection = previousTile != null
            ? (Vector2Int)(currentTile.gridLocation - previousTile.gridLocation) : new Vector2Int(0, 0);

        // 현재 타일 → 다음 타일 방향 벡터
        Vector2Int futureDirection = futureTile != null
            ? (Vector2Int)(futureTile.gridLocation - currentTile.gridLocation) : new Vector2Int(0, 0);

        // 방향이 꺾이는 경우: 두 벡터를 더해서 대각선 방향 계산
        Vector2Int direction = pastDirection != futureDirection
            ? pastDirection + futureDirection : futureDirection;

        //직선 방향 처리 (중간경로)
        if (direction == new Vector2(0, 1) && !isFinal) return ArrowDirection.Up;
        if (direction == new Vector2(0, -1) && !isFinal) return ArrowDirection.Down;
        if (direction == new Vector2(1, 0) && !isFinal) return ArrowDirection.Right;
        if (direction == new Vector2(-1, 0) && !isFinal) return ArrowDirection.Left;

        //꺾이는 방향 처리 (대각선)
        if (direction == new Vector2(1, 1))
            return pastDirection.y < futureDirection.y ? ArrowDirection.BottomLeft : ArrowDirection.TopRight;
        if (direction == new Vector2(-1, 1))
            return pastDirection.y < futureDirection.y ? ArrowDirection.BottomRight : ArrowDirection.TopLeft;
        if (direction == new Vector2(1, -1))
            return pastDirection.y > futureDirection.y ? ArrowDirection.TopLeft : ArrowDirection.BottomRight;
        if (direction == new Vector2(-1, -1))
            return pastDirection.y > futureDirection.y ? ArrowDirection.TopRight : ArrowDirection.BottomLeft;

        //마지막 타일 처리
        if (direction == new Vector2(0, 1) && isFinal) return ArrowDirection.UpFinished;
        if (direction == new Vector2(0, -1) && isFinal) return ArrowDirection.DownFinished;
        if (direction == new Vector2(-1, 0) && isFinal) return ArrowDirection.LeftFinished;
        if (direction == new Vector2(1, 0) && isFinal) return ArrowDirection.RightFinished;

        //방향을 계산 할 수 없거나 예외적인 경우
        return ArrowDirection.None;
    }
}
