using UnityEngine;
using UnityEngine.Events;

public class GameEventListener : MonoBehaviour
{
    [Tooltip("Event to register with.")]
    public GameEvent Event;

    [Tooltip("Response to invoke when Event is raised.")]
    public UnityEvent Response;

    private GameEventListenerInternal listener = new GameEventListenerInternal();

    private void OnEnable()
    {
        if (Event != null)
        {
            listener.RegisterEvent(Event, Response);
        }
    }

    private void OnDisable()
    {
        if (Event != null)
        {
            listener.UnregisterEvent();
        }
    }
}

public abstract class GameEventListener<T> : MonoBehaviour
{
    public abstract GameEvent<T> Event { get; set; }
    public abstract UnityEvent<T> Response { get; set; }

    private GameEventListenerInternal<T> listener = new GameEventListenerInternal<T>();

    private void OnEnable()
    {
        if (Event != null)
        {
            listener.RegisterEvent(Event, Response);
            Debug.Log($"RegisterEvent called for {Event.name}");
        }
        else
        {
            Debug.LogError($"Event is NULL on {gameObject.name}!");
        }
    }

    private void OnDisable()
    {
        if (Event != null)
        {
            listener.UnregisterEvent();
        }
    }
}
