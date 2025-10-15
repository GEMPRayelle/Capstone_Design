using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using static Define;
using static UnityEngine.EventSystems.EventTrigger;

// 일단 적 이동 관리하는 컨트롤러
public class MovementController : InitBase
{
    public Creature activeCharacter; // 현재 턴에 움직이는 캐릭터

    public float speed = 5.0f;
    public bool enableAutoMove = true;
    public bool moveThroughAllies = true;

    //public GameEvent endTurnEvent;

    //public GameEvent actionCompleted;

    private List<OverlayTile> path = new List<OverlayTile>();
    private bool isMoving = false;

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        return true;
    }

    // Update is called once per frame
    void Update()
    {
        if (activeCharacter && !activeCharacter.IsAlive)
        {
            ResetMovementManager();
        }


        if (path.Count > 0 && isMoving)
        {
            MoveAlongPath();
        }
    }

    //Resets movement mode when movement has Finished or is Cancelled. 
    public void ResetMovementManager()
    {
        isMoving = false;
        activeCharacter.CharacterMoved();
        path = new List<OverlayTile>();
    }

    //Move along a set path.
    private void MoveAlongPath()
    {
        var step = speed * Time.deltaTime;

        var zIndex = path[0].transform.position.z;
        activeCharacter.transform.position = Vector3.MoveTowards(activeCharacter.transform.position, path[0].transform.position, step);

        if (Vector3.Distance(activeCharacter.transform.position, path[0].transform.position) < 0.0001f)
        {
            //last tile
            if (path.Count == 1)
                PositionCharacterOnTile(activeCharacter, path[0]);

            path.RemoveAt(0);
        }

        if (path.Count == 0)
        {
            ResetMovementManager();


            //if (actionCompleted)
            //    actionCompleted.Raise();

            //if (enableAutoMove)
            //{
            //    if (endTurnEvent)
            //        endTurnEvent.Raise();
            //}
        }
    }


    //Link character to tile once movement has finished
    public void PositionCharacterOnTile(Creature character, OverlayTile tile)
    {
        if (tile != null)
        {
            character.transform.position = new Vector3(tile.transform.position.x, tile.transform.position.y + 0.0001f, tile.transform.position.z);
            character.currentStandingTile = tile;
            tile.isBlocked = true;
        }
    }

    //Movement event receiver for the AI
    public void MoveCharacterCommand(List<GameObject> pathToFollow)
    {
        if (activeCharacter)
        {
            isMoving = true;

            if (pathToFollow.Count > 0)
                path = pathToFollow.Select(x => x.GetComponent<OverlayTile>()).ToList();
        }
    }


}
