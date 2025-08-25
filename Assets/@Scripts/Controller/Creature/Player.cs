using Data;
using Newtonsoft.Json.Serialization;
using Spine.Unity;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using static Define;

public class Player : Creature
{
    public bool isNeedArrange { get; set; }//정리가 필요한지

    Vector2 _moveDir = Vector2.zero;
    public EPlayerState PlayerState; //Master, Servant 상태를 관리

    public override ECreatureState CreatureState
    {
        get { return base.CreatureState; }
        set
        {
            if (_creatureState != value)
            {
                base.CreatureState = value;
            }
        }
    }

    EPlayerMoveState _playerMoveState = EPlayerMoveState.None;
    public EPlayerMoveState PlayerMoveState //player가 움직이고 있는 사유
    {
        get { return _playerMoveState; }
        private set
        {
            _playerMoveState = value;
            switch (value)
            {
                case EPlayerMoveState.CollectEnv:
                    isNeedArrange = true;
                    break;
                case EPlayerMoveState.TargetMonster:
                    isNeedArrange = true;
                    break;
                case EPlayerMoveState.ForceMove:
                    isNeedArrange = true;
                    break;
            }
        }
    }

    private UI_BattleBarWorldSpace _battleBarUI;

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        CreatureType = ECreatureType.Player;
        PlayerState = EPlayerState.None;

        _battleBarUI = GetComponentInChildren<UI_BattleBarWorldSpace>();

        Speed = 5.0f; //임시 하드코딩

        Physics2D.queriesHitTriggers = true;
        Managers.Game.OnJoystickStateChanged -= HandleOnJoystickStateChanged;
        Managers.Game.OnJoystickStateChanged += HandleOnJoystickStateChanged;

        Managers.Game.OnMoveDirChanged -= HandleOnMoveDirChanged;
        Managers.Game.OnMoveDirChanged += HandleOnMoveDirChanged;

        Collider.isTrigger = true;
        RigidBody.simulated = true;

        StartCoroutine(CoUpdateAI());

        return true;
    }

    public override void SetInfo(int templateID)
    {
        base.SetInfo(templateID);

        //State
        CreatureState = ECreatureState.Idle;

        UpdateHpText();

        RigidBody.linearDamping = 0; //마찰력 설정
    }

    public override void OnDamaged(BaseObject attacker, SkillBase skill)
    {
        base.OnDamaged(attacker, skill);

        UpdateHpText();
    }

    protected void UpdateHpText()
    {
        _battleBarUI.SetInfo((int)Hp, (int)MaxHp.Value);
    }

    public void ResetHp()
    {
        Hp = MaxHp.Value;

        UpdateHpText();
    }

    #region AI
    protected override void UpdateIdle()
    {
        //0. 이동 상태라면 강제 이동
        if (PlayerMoveState == EPlayerMoveState.ForceMove)
        {
            CreatureState = ECreatureState.Move;
            return;
        }

        if (PlayerState == EPlayerState.Master) // 마스터는 적 체크 필요 x
            return;

        if (activePlayerState != PlayerState) // 현재 활성화된 playerState와 다르면 행동 X
            return;

        //2. 몬스터 탐색 및 사냥
        //BaseObject를 반환하기 때문에 Creature로 다시 캐스팅
        Creature creature = FindClosetObjectInRange(PLAYER_SEARCH_DISTANCE, Managers.Object.monsters) as Creature;
        if (creature != null)
        {
            Target = creature;
            CreatureState = ECreatureState.Move;
            PlayerMoveState = EPlayerMoveState.TargetMonster;
            return;
        }
    }

    protected override void UpdateMove()
    {
        //0. 이동 상태라면 강제 변경
        if (PlayerMoveState == EPlayerMoveState.ForceMove)
        {
            CreatureState = ECreatureState.Move;
            return;
        }

        if (PlayerState == EPlayerState.Master) // 마스터는 적 체크 필요 x
        {
            if (_moveDir == Vector2.zero) // 조이스틱으로 움직이지않으면 
                CreatureState = ECreatureState.Idle; // Idle 상태

            return;
        }

        if (activePlayerState != PlayerState) // 현재 활성화된 playerState와 다르면 행동 X
            return;

        //1. 주면에 몬스터가 있다면
        Creature creature = FindClosetObjectInRange(PLAYER_SEARCH_DISTANCE, Managers.Object.monsters) as Creature;

        if (creature != null) 
        {
            Target = creature;
            CreatureState = ECreatureState.Skill;
            PlayerMoveState = EPlayerMoveState.TargetMonster;
            return;
        }
        else
        {
            disappearMonster();
        }
    }

    protected override void UpdateSkill()
    {
        base.UpdateSkill();

        if (PlayerMoveState == EPlayerMoveState.ForceMove)
        {
            CreatureState = ECreatureState.Move;
            return;
        }

        if (Target.IsValid() == false)
        {
            CreatureState = ECreatureState.Move;
            return;
            
        }

        //CheckEnemy(PLAYER_SEARCH_DISTANCE); // Skill상태에서 적이 근처에 계속 있다면 유지
    }

    protected override void UpdateAttack()
    {

    }

    protected override void UpdateDead()
    {
        base.UpdateDead();
    }
    #endregion


    // 근처에 몬스터 있다면 true 아니면 false 리턴
    

    // 근처 몬스터 없을 때 실행되는 함수
    private void disappearMonster()
    {
        // 이동 입력에 따라 state를 idle이나 move로 변경
        if (_moveDir == Vector2.zero)
        {
            CreatureState = ECreatureState.Idle;
        }
        else
        {
            CreatureState = ECreatureState.Move;
        }
    }

    private void Update()
    {
        if (activePlayerState != PlayerState) // 현재 active된 PlayerState와 다르면 무시
        {
            return;
        }

        if (CreatureState == ECreatureState.Move)
        {
            transform.TranslateEx(_moveDir * Time.deltaTime * Speed);
        }

    }

    private void HandleOnJoystickStateChanged(EJoystickState joystickState)
    {
        if (CreatureState == ECreatureState.Attack) // 공격중일 땐 move로 변경 x
            return;

        if (activePlayerState != PlayerState) // 현재 활성화된 PlayerState랑 다르다면 무시
            return;

        switch (joystickState)
        {
            case EJoystickState.PointerDown:
                PlayerMoveState = EPlayerMoveState.ForceMove;
                break;
            case EJoystickState.Drag:
                PlayerMoveState = EPlayerMoveState.ForceMove;
                break;
            case EJoystickState.PointerUp:
                PlayerMoveState = EPlayerMoveState.None;//강제 이동 해제
                break;
            default:
                break;
        }
    }

    private void HandleOnMoveDirChanged(Vector2 dir)
    {
        if (activePlayerState != PlayerState) // 현재 활성화된 PlayerState랑 다르다면 무시
            return;

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
            CreatureState = ECreatureState.Idle;
        }

        else
        {

        }

    }

    protected override void ChangedServent() // 마스터->서번트 변경 시 로직
    {
        base.ChangedServent(); // Creature가 해야되는 공통 로직 호출
        if (PlayerState == EPlayerState.Master)
        {
            CreatureState = ECreatureState.Idle;
        }

        else
        {

        }
    }

}
