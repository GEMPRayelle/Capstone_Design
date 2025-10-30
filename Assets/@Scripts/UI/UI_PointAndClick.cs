using UnityEngine;
using UnityEngine.EventSystems;

public class UI_PointAndClick : UI_Base
{
    public Creature creature; //이동할 오브젝트
    
    private Vector2Int touchPos;

    private OverlayTile _startPosition; //
    private OverlayTile _destPosition; //

    public override bool Init()
    {
        if (base.Init() == false)
            return false;


        gameObject.BindEvent(OnPointerDown, type: Define.EUIEvent.PointerDown);

        return true;
    }

    public void OnPointerDown(PointerEventData evt)
    {
        //1. 기존 위치 Cell정보
        _startPosition = creature.currentStandingTile;
        //2. 이동 할 위치 Cell 정보
        _destPosition = Managers.Map.mapDict[touchPos];
        //3. MoveTo를 연속으로 해서 Path경로를 뽑아옴
        //4. LerpMove 실행
    }
}
