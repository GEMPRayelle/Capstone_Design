using UnityEngine;
using UnityEngine.Events;

public class GameEventGameObjectListener : GameEventListener<GameObject>
{
    [SerializeField] private GameEventGameObject eventGameEvent = null;
    [SerializeField] private UnityEvent<GameObject> response = null;

    public override GameEvent<GameObject> Event
    {
        get => eventGameEvent;
        set => eventGameEvent = value as GameEventGameObject;
    }

    public override UnityEvent<GameObject> Response
    {
        get => response;
        set => response = value;
    }
}