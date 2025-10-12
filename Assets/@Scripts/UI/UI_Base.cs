using System;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Connect;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 모든 UI관련 클래스들이 상속받을 조상 클래스
/// </summary>
public class UI_Base : InitBase
{
    protected Dictionary<Type, UnityEngine.Object[]> _objects = new Dictionary<Type, UnityEngine.Object[]>();

    private void Awake()
    {
        Init();
    }

    //UI를 하나씩 인스펙터창에서 드래그 드롭 하는 방식은 제외
    //이름으로 하나씩 찾아가서 binding으로 연결시켜주는걸 원칙으로함
    protected void Bind<T>(Type type) where T : UnityEngine.Object
    {
        string[] names = Enum.GetNames(type);
        UnityEngine.Object[] objects = new UnityEngine.Object[names.Length];
        //_objects 딕셔너리에 T타입에 대한 오브젝트 배열을 등록
        _objects.Add(typeof(T), objects);

        //enum 이름들을 순회하며 오브젝트들을 찾고 배열에 저장
        for (int i = 0; i < names.Length; i++)
        {
            if (typeof(T) == typeof(GameObject))
                objects[i] = Util.FindChild(gameObject, names[i], true);
            else
                objects[i] = Util.FindChild<T>(gameObject, names[i], true);

            if (objects[i] == null)
                Debug.Log($"Failed to bind({names[i]})");
        }
    }

    protected void BindObjects(Type type) { Bind<GameObject>(type); }
    protected void BindImages(Type type) { Bind<Image>(type); }
    protected void BindTexts(Type type) { Bind<TMP_Text>(type); }
    protected void BindButtons(Type type) { Bind<Button>(type); }
    protected void BindToggles(Type type) { Bind<Toggle>(type); }
    protected void BindSliders(Type type) { Bind<Slider>(type); }


    /// <summary>
    /// 인덱스를 통해 T 타입의 오브젝트를 가져오는 함수
    /// </summary>
    /// <typeparam name="T">T는 UnityEngine.Object를 상속하는 타입이여야 함</typeparam>
    /// <param name="idx">배열의 인덱스</param>
    /// <returns>idx에 해당하는 오브젝트를 배열에서 가져와서 T 타입으로 casting후 리턴</returns>
    protected T Get<T>(int idx) where T : UnityEngine.Object
    {
        UnityEngine.Object[] objects = null;
        //_objects 딕셔너리에 T 타입에 대응되는 오브젝트 배열을 찾는다
        if (_objects.TryGetValue(typeof(T), out objects) == false)
            //해당 타입의 오브젝트 배열이 존재 하지 않으면 null
            return null;

        return objects[idx] as T;
    }

    protected GameObject GetObject(int idx) { return Get<GameObject>(idx); }
    protected TMP_Text GetText(int idx) { return Get<TMP_Text>(idx); }
    protected Button GetButton(int idx) { return Get<Button>(idx); }
    protected Image GetImage(int idx) { return Get<Image>(idx); }
    protected Toggle GetToggle(int idx) { return Get<Toggle>(idx); }
    protected Slider GetSlider(int idx) { return Get<Slider>(idx); }

    /// <summary>
    /// 어떤 게임오브젝트(go)에다가 type이라는 이벤트를 추적하고 싶고 그 이벤트가 발생하면 action 이벤트를 실행하는 함수
    /// </summary>
    /// <param name="go">게임 오브젝트</param>
    /// <param name="action">이벤트가 발생시 실행시킬 함수</param>
    /// <param name="type">추적하고 싶은 이벤트 타입</param>
    public static void BindEvent(GameObject go, Action<PointerEventData> action = null, Define.EUIEvent type = Define.EUIEvent.Click)
    {
        UI_EventHandler evt = Util.GetOrAddComponent<UI_EventHandler>(go);

        switch (type)
        {
            case Define.EUIEvent.Click:
                evt.OnClickHandler -= action;
                evt.OnClickHandler += action;
                break;
            case Define.EUIEvent.PointerDown:
                evt.OnPointerDownHandler -= action;
                evt.OnPointerDownHandler += action;
                break;
            case Define.EUIEvent.PointerUp:
                evt.OnPointerUpHandler -= action;
                evt.OnPointerUpHandler += action;
                break;
            case Define.EUIEvent.Drag:
                evt.OnDragHandler -= action;
                evt.OnDragHandler += action;
                break;
            case Define.EUIEvent.PointerEnter: 
                evt.OnPointerEnterHandler -= action;
                evt.OnPointerEnterHandler += action;
                break;
            case Define.EUIEvent.PointerExit: 
                evt.OnPointerExitHandler -= action;
                evt.OnPointerExitHandler += action;
                break;
        }
    }
}
