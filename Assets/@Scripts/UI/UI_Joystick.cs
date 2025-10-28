using Unity.VisualScripting;
using UnityEditor.Profiling.Memory.Experimental;
using UnityEngine;
using UnityEngine.EventSystems;
using static Define;

public class UI_Joystick : UI_Base
{
    enum GameObjects
    {
        JoystickBG, //조이스틱 백그라운드 이미지
        JoystickCursor //조이스틱 커서 이미지
    }

    private GameObject _background;
    private GameObject _cursor;
    private float _radius; //조이스틱 반지름
    private Vector2 _touchPos; //조이스틱 최초 터치 위치

    public override bool Init()
    {
        if (base.Init() == false) 
            return false;

        BindObjects(typeof(GameObjects));

        gameObject.GetComponent<Canvas>().sortingOrder = SortingLayers.UI_JOYSTICK;

        _background = GetObject((int)GameObjects.JoystickBG);
        _cursor = GetObject((int)GameObjects.JoystickCursor);
        _radius = _background.GetComponent<RectTransform>().sizeDelta.y / 5; //커서가 밖에 나가지 않도록 억제
        
        gameObject.BindEvent(OnPointerDown, type: Define.EUIEvent.PointerDown); 
        gameObject.BindEvent(OnPointerUp, type: Define.EUIEvent.PointerUp);
        gameObject.BindEvent(OnDrag, type: Define.EUIEvent.Drag);

        //Joystick이 UI화면 전체를 먹고 있는걸 예방하기 위해 Overlay가 아닌 Camera로 세팅
        GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceCamera;
        GetComponent<Canvas>().worldCamera = Camera.main;

        return true;
    }

    #region Event Func
    public void OnPointerDown(PointerEventData eventData)
    {
        _background.transform.position = eventData.position;
        _cursor.transform.position = eventData.position;
        _touchPos = eventData.position;//원래 위치 기억

        Managers.Game.JoystickState = EJoystickState.PointerDown;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _cursor.transform.position = _touchPos;//원래 위치로 복원

        Managers.Game.MoveDir = Vector2.zero;
        Managers.Game.JoystickState= EJoystickState.PointerUp;
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 touchDir = (eventData.position - _touchPos); //현재 위치에서 최초 위치를 뺌

        //최초 누른 위치와 현재 위치의 거리는 _radius를 넘지는 못함
        float moveDistance = Mathf.Min(touchDir.magnitude, _radius);
        Vector2 moveDir = touchDir.normalized;
        Vector2 newPosition = _touchPos + moveDir * moveDistance; //처음 누른 위치에서 moveDir 방향으로 moveDistance만큼 이동
        _cursor.transform.position = newPosition;//커서 위치 갱신

        Managers.Game.MoveDir = moveDir;//이동하려는 방향을 GameManager에게 전달 
        Managers.Game.JoystickState = EJoystickState.Drag;
    }
    #endregion
}
