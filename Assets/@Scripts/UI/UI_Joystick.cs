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
    private Vector2 _touchPos; //조이스틱 터치 위치

    public override bool Init()
    {
        if (base.Init() == false) 
            return false;

        BindObjects(typeof(GameObjects));

        _background = GetObject((int)GameObjects.JoystickBG);
        _cursor = GetObject((int)GameObjects.JoystickCursor);
        _radius = _background.GetComponent<RectTransform>().sizeDelta.y / 5;

        gameObject.BindEvent(OnPointerDown, Define.EUIEvent.PointerDown);
        gameObject.BindEvent(OnPointerUp, Define.EUIEvent.PointerUp);
        gameObject.BindEvent(OnDrag, Define.EUIEvent.Drag);

        GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceCamera;
        GetComponent<Canvas>().worldCamera = Camera.main;

        return true;
    }

    #region Event Func
    public void OnPointerDown(PointerEventData eventData)
    {
        _touchPos = Input.mousePosition;

        Vector2 mouseWorldPos = Camera.main.WorldToScreenPoint(Input.mousePosition);
        _background.transform.position = mouseWorldPos;
        _cursor.transform.position = mouseWorldPos;

        Managers.Game.JoystickState = EJoystickState.PointerDown;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _background.transform.position = _touchPos;
        _cursor.transform.position = _touchPos;

        Managers.Game.MoveDir = Vector2.zero;
        Managers.Game.JoystickState= EJoystickState.PointerUp;
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 touchDir = (eventData.position - _touchPos);

        float moveDistance = Mathf.Min(touchDir.magnitude, _radius);
        Vector2 moveDir = touchDir.normalized;
        Vector2 newPosition = _touchPos + moveDir * moveDistance;

        Vector2 worldPos = Camera.main.ScreenToWorldPoint(newPosition);
        _cursor.transform.position = worldPos;

        Managers.Game.MoveDir = moveDir;
        Managers.Game.JoystickState = EJoystickState.Drag;
    }
    #endregion
}
