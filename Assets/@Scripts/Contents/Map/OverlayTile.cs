using System.Collections.Generic;
using UnityEngine;
using static Define;

public class OverlayTile :  InitBase
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

    public override bool Init()
    {
        if (base.Init() == false) 
            return false;

        return true;
    }

    private void Update()
    {
        // 마우스 왼쪽 클릭 시 타일을 숨김 (테스트용 또는 디버깅용 기능)
        if (Input.GetMouseButtonDown(0))
        {
            HideTile();
        }
    }

    public void HideTile()
    {
        //타일의 색상을 투명하게 설정
        gameObject.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0);
    }

    public void ShowTile()
    {
        //타일의 SpriteRenderer 색상을 불투명하게 설정
        gameObject.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 1);
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
            GetComponentsInChildren<SpriteRenderer>()[1].sortingOrder = gameObject.GetComponent<SpriteRenderer>().sortingOrder;
        }
    }

}
