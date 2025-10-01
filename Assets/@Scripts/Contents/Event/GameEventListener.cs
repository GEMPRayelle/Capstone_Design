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
    public abstract GameEvent<T> Event { get; }
    public abstract UnityEvent<T> Response { get; }

    private GameEventListenerInternal<T> listener = new GameEventListenerInternal<T>();

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
