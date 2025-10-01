using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameEventGameObjectListListener : GameEventListener<List<GameObject>>
{
    [SerializeField] private GameEventGameObjectList eventGameObjects = null;
    [SerializeField] private UnityEvent<List<GameObject>> response = null;

    public override GameEvent<List<GameObject>> Event => eventGameObjects;
    public override UnityEvent<List<GameObject>> Response => response;
}
