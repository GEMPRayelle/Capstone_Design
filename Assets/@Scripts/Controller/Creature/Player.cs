using Spine.Unity;
using System;
using UnityEngine;
using static Define;

public class Player : Creature
{
    Vector2 _moveDir = Vector2.zero;
    public EPlayerState PlayerState; //Master, Servant 상태를 관리
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

    protected override void ChangedMaster() // 서번트->마스터 변경 시 로직
    {
        base.ChangedMaster(); // Creature가 해야되는 공통 로직 호출
        if (PlayerState == EPlayerState.Servant)
        {
            Managers.Game.OnJoystickStateChanged -= HandleOnJoystickStateChanged;
            CreatureState = ECreatureState.Idle;
        }

        else
        {
            Managers.Game.OnJoystickStateChanged -= HandleOnJoystickStateChanged;
            Managers.Game.OnJoystickStateChanged += HandleOnJoystickStateChanged;
        }

    }

    protected override void ChangedServent() // 마스터->서번트 변경 시 로직
    {
        base.ChangedServent(); // Creature가 해야되는 공통 로직 호출
        if (PlayerState == EPlayerState.Master)
        {
            Managers.Game.OnJoystickStateChanged -= HandleOnJoystickStateChanged;
            CreatureState = ECreatureState.Idle;
        }

        else
        {
            Managers.Game.OnJoystickStateChanged -= HandleOnJoystickStateChanged;
            Managers.Game.OnJoystickStateChanged += HandleOnJoystickStateChanged;
        }
    }
    

}
