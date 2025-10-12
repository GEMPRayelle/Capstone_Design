using System;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 어떤 UI를 클릭또는 드래그 했을때 실행될 함수를 맵핑하는 클래스 (UI에다가 어떤 이벤트를 붙이고싶을때)
/// </summary>
public class UI_EventHandler : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler, IDragHandler, IPointerEnterHandler, IPointerExitHandler
{
    //원래 내부적으로 EventSystem 컴포넌트에서 기능을 수행
    public event Action<PointerEventData> OnClickHandler = null;//마우스 버튼 클릭했을때 발생하는 이벤트
    public event Action<PointerEventData> OnPointerDownHandler = null;
    public event Action<PointerEventData> OnPointerUpHandler = null;
    public event Action<PointerEventData> OnDragHandler = null;
    public event Action<PointerEventData> OnPointerEnterHandler = null; 
    public event Action<PointerEventData> OnPointerExitHandler = null;   

    /// <summary>
    /// 마우스를 클릭했을때 발생할 이벤트
    /// </summary>
    /// <param name="eventData">이벤트 발생시 실행시킬 함수</param>
    public void OnPointerClick(PointerEventData eventData)
    {
        OnClickHandler?.Invoke(eventData);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        OnPointerDownHandler?.Invoke(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        OnPointerUpHandler?.Invoke(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        OnDragHandler?.Invoke(eventData);
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        OnPointerEnterHandler?.Invoke(eventData);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        OnPointerExitHandler?.Invoke(eventData);
    }
}
