using TMPro;
using UnityEngine;
using static Define;

public class Player : Creature
{
    Vector2 _moveDir = Vector2.zero;
    public bool NeedArrange { get; set; }
    public float Speed { get; set; } = 5.0f;

    public Transform Pivot { get; private set; }
    public Transform Destination { get; private set; }

    public override ECreatureState CreatureState
    {
        get { return _creatureState; }
        set 
        {
            if (_creatureState != value)
            {
                base.CreatureState = value;
            }
        }
    }

    EPlayerMoveState _playerMoveState = EPlayerMoveState.None;
    public EPlayerMoveState PlayerMoveState
    {
        get { return _playerMoveState; }
        private set
        {
            _playerMoveState = value;
            switch (value)
            {
                case EPlayerMoveState.ForceMove:
                    NeedArrange = true;
                    break;
            }
        }
    }

    public Transform PlayerDest
    {
        get
        {
            Player player = Managers.Object.Players;

            return player.Destination;
        }
    }

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        ObjectType = EObjectType.Creature;

        Managers.Game.OnJoystickStateChanged -= HandleOnJoystickStateChanged;
        Managers.Game.OnJoystickStateChanged += HandleOnJoystickStateChanged;

        Managers.Game.OnMoveDirChanged -= HandleOnMoveDirChanged;
        Managers.Game.OnMoveDirChanged += HandleOnMoveDirChanged;

        Pivot = Util.FindChild<Transform>(gameObject, "Pivot", true);
        Destination = Util.FindChild<Transform>(gameObject, "Destination", true);

        Collider.isTrigger = true;
        RigidBody.simulated = false;

        StartCoroutine(CoUpdateAI());

        return true;
    }

    public override void SetInfo(int templateID)
    {
        base.SetInfo(templateID);

        //State
        CreatureState = ECreatureState.Idle;
    }

    //강제 이동
    public void ForceMove(Vector3 position)
    {
        transform.position = position;
    }

    private void Update()
    {
        Vector3 dir = _moveDir * Time.deltaTime * Speed;
        Vector3 newPos = transform.position + dir;

        transform.position = newPos;
    }

    private void HandleOnJoystickStateChanged(EJoystickState joystickState)
    {
        switch (joystickState)
        {
            case EJoystickState.PointerDown:
                PlayerMoveState = EPlayerMoveState.ForceMove;
                break;
            case EJoystickState.Drag:
                PlayerMoveState = EPlayerMoveState.ForceMove;
                break;
            case EJoystickState.PointerUp:
                PlayerMoveState = EPlayerMoveState.None;
                break;
            default:
                break;
        }
    }

    private void HandleOnMoveDirChanged(Vector2 dir)
    {
        _moveDir = dir;

        if(dir != Vector2.zero)
        {
            float angle = Mathf.Atan2(-dir.x, +dir.y) * 180 / Mathf.PI;
            Pivot.eulerAngles = new Vector3(0,0,angle);
        }
    }
}
