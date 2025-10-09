using System.Collections.Generic;
using UnityEngine;
using static Define;

public class OverlayTile : InitBase
{
    public int G; //start to current distance
    public int H; //current to end distance (
    public int F { get { return G + H; } } //최종 스코어

    public bool isBlocked; //지나가지 못하는 블럭

    public OverlayTile previousTile; //이전에 지나간 타일
    public Vector3Int gridLocation; //3d 상에서 그리드 위치
    public Vector2Int grid2DLocation { get { return new Vector2Int(gridLocation.x, gridLocation.y); } } //2D좌표로 변환 (경로 탐색에 사용)

    //방향 화살표 스프라이트 리스트 (ArrowDirection enum 순서대로 저장)
    public List<Sprite> arrows;
    public TileData tileData;

    [HideInInspector]
    public int remainingMovement;

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        HideTile();

        return true;
    }


    public void HideTile()
    {
        //타일의 색상을 투명하게 설정
        gameObject.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0);

        SetArrowSprite(ArrowDirection.None);
    }

    public void ShowTile()
    {
        //타일의 SpriteRenderer 색상을 불투명하게 설정
        gameObject.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 1);
    }

    public void ShowTile(Color color)
    {
        gameObject.GetComponent<SpriteRenderer>().color = color;
    }

    public void HighlightTileBlue()
    {
        gameObject.GetComponent<SpriteRenderer>().color = new Color(0, 0, 1, 1);
    }

    public void HighlightTileRed()
    {
        gameObject.GetComponent<SpriteRenderer>().color = new Color(1, 0, 0, 1);
    }

    public void SetSprite(ArrowDirection d)
    {
        if (d == ArrowDirection.None)
            //화살표를 숨김 (투명 처리)
            GetComponentsInChildren<SpriteRenderer>()[1].color = new Color(1, 1, 1, 0);
        else
        {
            //화살표를 표시하고 해당 방향의 스프라이트로 설정
            GetComponentsInChildren<SpriteRenderer>()[1].color = new Color(1, 1, 1, 1);
            GetComponentsInChildren<SpriteRenderer>()[1].sprite = arrows[(int)d];

            //화살표의 렌더링 순서를 타일과 동일하게 설정
            GetComponentsInChildren<SpriteRenderer>()[1].sortingOrder = gameObject.GetComponent<SpriteRenderer>().sortingOrder + 1;
        }
    }

    /// <summary>
    /// 경로 표시용 화살표 스프라이트를 설정하는 메서드
    /// 캐릭터의 이동 경로를 시각적으로 표시하는 데 사용됩니다.
    /// 
    /// 화살표 표시 시나리오:
    /// 1. 플레이어가 이동 경로를 계획할 때
    /// 2. AI의 이동 경로를 미리 보여줄 때  
    /// 3. 리플레이나 튜토리얼에서 이동 설명시
    /// </summary>
    /// <param name="d">화살표 방향 (ArrowDirection 열거형)</param>
    public void SetArrowSprite(ArrowDirection d)
    {
        var arrow = GetComponentsInChildren<SpriteRenderer>()[1];

        if (d == ArrowDirection.None)
        {
            // 화살표 숨기기: 완전 투명으로 설정
            arrow.color = new Color(1, 1, 1, 0);
        }
        else
        {
            // 화살표 표시: 불투명으로 설정하고 해당 방향 스프라이트 적용
            arrow.color = new Color(1, 1, 1, 1);

            // ArrowDirection 열거형 값을 배열 인덱스로 사용
            // 예: ArrowDirection.Up = 0 → arrows[0] = 위쪽 화살표
            arrow.sprite = arrows[(int)d];

            // 주석 처리된 코드: 렌더링 순서 설정
            // 필요시 타일보다 화살표가 위에 렌더링되도록 설정 가능
            // arrow.sortingOrder = GetComponent<SpriteRenderer>().sortingOrder;
        }
    }

    /// <summary>
    /// 이 타일의 이동 비용을 반환하는 메서드
    /// 지형별로 다른 이동 비용을 제공
    /// 
    /// Example:
    /// - 평지: 1 (기본)
    /// - 숲: 2 (이동하기 어려움)
    /// - 늪지대: 3 (매우 느림)
    /// - 도로: 1 (빠른 이동)
    /// - 산: 2 (험한 지형)
    /// 
    /// tileData가 없으면 기본값 1을 반환하여 오류 방지
    /// </summary>
    /// <returns>(int)이 타일의 이동 비용</returns>
    public int GetMoveCost() => tileData != null ? tileData.moveCost : 1;
}
