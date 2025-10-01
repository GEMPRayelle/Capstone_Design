using UnityEngine;
using UnityEngine.Events;

public class GameEventGameObjectListener : GameEventListener<GameObject>
{
    [SerializeField] private GameEventGameObject eventGameEvent = null;
    [SerializeField] private UnityEvent<GameObject> response = null;

    public override GameEvent<GameObject> Event => eventGameEvent;
    public override UnityEvent<GameObject> Response => response;
}
