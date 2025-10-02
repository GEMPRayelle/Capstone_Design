using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
[CreateAssetMenu(fileName = "GameEvent", menuName = "GameEvent/GameEvent", order = 1)]
public class GameEvent : ScriptableObject
{
    private readonly List<GameEventListenerInternal> _eventListeners
        = new List<GameEventListenerInternal>();

    public void Raise()
    {
        Raise(null);
    }

    // 이벤트를 호출하는 메서드 (콜백 포함)
    public void Raise(Action onRaised)
    {
        // 특정 이벤트 이름은 로그 출력 제외
        if (name != "CorsorEvent" && name != "FocusedOnNewTile")
            Debug.Log(this.name);

        // 등록된 리스너들을 역순으로 호출
        for (int i = _eventListeners.Count - 1; i >= 0; i--)
            _eventListeners[i].OnEventRaised();

        // 이벤트 호출 후 콜백 실행
        onRaised?.Invoke();
    }

    public void UnregisterListener(GameEventListenerInternal listener)
    {
        if (_eventListeners.Contains(listener))  // 있으면 제거
            _eventListeners.Remove(listener);
    }

    public void RegisterListener(GameEventListenerInternal listener)
    {
        if (!_eventListeners.Contains(listener))  // 없으면 추가
            _eventListeners.Add(listener);
    }
}

[Serializable]
public class GameEvent<T> : ScriptableObject
{
    private readonly List<GameEventListenerInternal<T>> _eventListeners =
        new List<GameEventListenerInternal<T>>();

    public void Raise(T parameter)
    {
        Raise(parameter, null);
    }

    public void Raise(T parameter, Action onRaised)
    {
        // 특정 이벤트 이름은 로그 출력 제외
        if (name != "FocusOnTile")
            Debug.Log(this.name);

        // 등록된 리스너들을 역순으로 호출
        for (int i = _eventListeners.Count - 1; i >= 0; i--)
            _eventListeners[i].OnEventRaised(parameter);

        // 이벤트 호출 후 콜백 실행
        onRaised?.Invoke();
    }

    public void UnregisterListener(GameEventListenerInternal<T> listener)
    {
        if (_eventListeners.Contains(listener))  // 있으면 제거
            _eventListeners.Remove(listener);
    }

    public void RegisterListener(GameEventListenerInternal<T> listener)
    {
        if (!_eventListeners.Contains(listener))  // 없으면 추가
            _eventListeners.Add(listener);
    }
}
