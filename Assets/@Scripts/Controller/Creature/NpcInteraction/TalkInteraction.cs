using UnityEngine;

public class TalkInteraction : INpcInteraction
{
    private Npc _owner;

    public void SetInfo(Npc owner)
    {
        _owner = owner;
    }

    public bool CanInteract()
    {
        throw new System.NotImplementedException();
    }

    public void HandleOnClickEvent()
    {
        Debug.Log("Npc Interaction");
    }
}
