using Spine.Unity;
using System;
using UnityEngine;
using static Define;

public class Player : Creature
{
    Vector2 _moveDir = Vector2.zero;
    public EPlayerState PlayerState; //Master, Servant 상태를 관리
    private EPlayerState activePlayerState;
    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        CreatureType = ECreatureType.Player;
        CreatureState = ECreatureState.Idle;
        Speed = 5.0f; //임시 하드코딩

        Managers.Game.OnJoystickStateChanged -= HandleOnJoystickStateChanged;
        Managers.Game.OnJoystickStateChanged += HandleOnJoystickStateChanged;

        Managers.Game.OnMoveDirChanged -= HandleOnMoveDirChanged;
        Managers.Game.OnMoveDirChanged += HandleOnMoveDirChanged;

        Managers.Game.OnPlayerStateChanged -= HandleOnPlayerStateChanged;
        Managers.Game.OnPlayerStateChanged += HandleOnPlayerStateChanged;


        Collider.isTrigger = true;
        RigidBody.simulated = false;
        PlayerState = EPlayerState.None;

        SkeletonAnimation skeletonAnim = GetComponent<SkeletonAnimation>();
        skeletonAnim.GetComponent<MeshRenderer>().sortingOrder = SortingLayers.PLAYER;

        return true;
    }



    public override void SetInfo(int templateID)
    {
        base.SetInfo(templateID);

        //State
        CreatureState = ECreatureState.Idle;
    }

    private void Update()
    {
        if (activePlayerState != PlayerState)
            return;

        transform.TranslateEx(_moveDir * Time.deltaTime * Speed);
    }

    private void HandleOnJoystickStateChanged(EJoystickState joystickState)
    {
        switch (joystickState)
        {
            case EJoystickState.PointerDown:
                CreatureState = ECreatureState.Move;
                break;
            case EJoystickState.Drag:
                break;
            case EJoystickState.PointerUp:
                CreatureState = ECreatureState.Idle;
                break;
            default:
                break;
        }
    }

    private void HandleOnMoveDirChanged(Vector2 dir)
    {
        _moveDir = dir;
        // Debug.Log(dir);

        //if(dir != Vector2.zero)
        //{
        //    float angle = Mathf.Atan2(-dir.x, +dir.y) * 180 / Mathf.PI;
        //    Pivot.eulerAngles = new Vector3(0,0,angle);
        //}
    }

    private void HandleOnPlayerStateChanged(EPlayerState playerstate)
    {
        switch (playerstate)
        {
            case EPlayerState.None:
                break;
            case EPlayerState.Master:
                activePlayerState = EPlayerState.Master;
                break;
            case EPlayerState.Servant:
                activePlayerState = EPlayerState.Servant;
                break;
        }
    }

}
