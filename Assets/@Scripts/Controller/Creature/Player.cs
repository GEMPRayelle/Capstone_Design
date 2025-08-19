using Data;
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
    private float nearestDistanceSqr = float.MaxValue; // Distance 검사할때 사용하는 minDistance 값

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
        //TODO -> 플레이어 이동 사유에 따라 State전환

        CheckEnemy(PLAYER_SEARCH_DISTANCE); // Idle -> Attack 체크
    }

    protected override void UpdateMove()
    {
        base.UpdateMove();
        CheckEnemy(PLAYER_SEARCH_DISTANCE); // move -> Attack 체크
    }

    protected override void UpdateSkill()
    {
        base.UpdateSkill();
        CheckEnemy(PLAYER_SEARCH_DISTANCE); // Skill상태에서 적이 근처에 계속 있다면 유지
    }

    protected override void UpdateAttack()
    {
        base.UpdateAttack();
        CheckEnemy(PLAYER_SEARCH_DISTANCE);
    }

    protected override void UpdateDead()
    {
        base.UpdateDead();
    }
    private void CheckEnemy(float detectionRadius) // 근처에 적이 있다면 Attack으로 바꿔주는 함수
    {
        if (PlayerState == EPlayerState.Master) // 마스터는 적 체크 필요 x
            return;

        if (activePlayerState == EPlayerState.Master) // 태그 눌러서 현재 활성화된게 마스터면
            return;                                   // 서번트여도 체크 필요 X

        if (DetectMonster(detectionRadius, Managers.Object.monsters) == true) // 실제 근처 적 체크 함수
        {
            // 적 발견 시 공격 상태로 전환 -> 스킬 상태로 전환
            CreatureState = ECreatureState.Skill;

        }
        else // 근처에 적이 없다면
        {
            disappearMonster();
        }
    }
    #endregion


    // 근처에 몬스터 있다면 true 아니면 false 리턴
    private bool DetectMonster(float detectionRadius, HashSet<Monster> monsters)
    {

        // 범위 기반 탐색
        //Collider2D monster = Physics2D.OverlapCircle(transform.position, detectionRadius, MonsterLayer);

        //if (monster != null)
        //{
        //    return true;
        //}

        //return false;

        // hashSet에서 가장 가까운 적 탐색
        if (monsters.Count == 0)
            return false;

        nearestDistanceSqr = float.MaxValue;
        Creature target = null;

        foreach (Monster monster in monsters) // 몬스터마다
        {
            float distanceSqr = (monster.transform.position - transform.position).sqrMagnitude; // 거리 계산

            if (distanceSqr >= detectionRadius) // 거리가 일정 범위 이상이면 무시
                continue;

            if (distanceSqr < nearestDistanceSqr)
            {
                nearestDistanceSqr = distanceSqr;
                target = monster;
            }
        }

        if (target == null)
        {
            return false;
        }

        else
        {
            Target = target;
            return true;
        }

    }

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

        else if (CreatureState == ECreatureState.Attack || CreatureState == ECreatureState.Skill)
        {
            transform.Translate(_moveDir * Time.deltaTime * Speed);
        }
        //Debug.Log(CreatureState);
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
