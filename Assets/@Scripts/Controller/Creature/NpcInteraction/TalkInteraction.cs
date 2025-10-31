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
        return true;
    }

    public void HandleOnClickEvent()
    {
        Debug.Log("Npc Interaction");
    }
}
