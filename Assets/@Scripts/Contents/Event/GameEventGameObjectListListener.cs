using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameEventGameObjectListListener : GameEventListener<List<GameObject>>
{
    [SerializeField] private GameEventGameObjectList eventGameObjects = null;
    [SerializeField] private UnityEvent<List<GameObject>> response = null;

    public override GameEvent<List<GameObject>> Event
    {
        get => eventGameObjects;
        set => eventGameObjects = value as GameEventGameObjectList;
    }

    public override UnityEvent<List<GameObject>> Response
    {
        get => response;
        set => response = value;
    }
}