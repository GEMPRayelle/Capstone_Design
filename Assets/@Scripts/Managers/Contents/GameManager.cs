using System;
using UnityEngine;
using static Define;

public class GameManager : MonoBehaviour
{

    #region Player
    private Vector2 _moveDir; //플레이어가 이동하려는 방향
    public Vector2 MoveDir
    {
        get { return _moveDir; }
        set
        {
            _moveDir = value;
            //_moveDir에 변화가 있는 사실을 알려줌
            OnMoveDirChanged?.Invoke(value);
        }
    }

    private EJoystickState _joystickState;
    public EJoystickState JoystickState
    {
        get { return _joystickState; }
        set
        {
            _joystickState = value;
            OnJoystickStateChanged?.Invoke(_joystickState);
        }
    }

    private EPlayerState _playerState;
    public EPlayerState PlayerState
    {
        get { return _playerState; }
        set 
        {
            _playerState = value;
            OnPlayerStateChanged?.Invoke(_playerState);
        }
    }

    public void InverserPlayerState()
    {
        if (PlayerState == EPlayerState.Servant)
            PlayerState = EPlayerState.Master;
        else
            PlayerState = EPlayerState.Servant;
    }
    #endregion

    #region Action
    public event Action<Vector2> OnMoveDirChanged; //moveDir에 관련 있다면 콜백함수를 등록해야 함
    public event Action<Define.EJoystickState> OnJoystickStateChanged; //조이스틱에 상태 변화에 대해 관심이 있으면 콜백함수 등록
    public event Action<Define.EPlayerState> OnPlayerStateChanged; // 플레이어 상태 변화에 관심 있으면 콜백함수 등록
   
    #endregion
}
