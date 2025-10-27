using Data;
using Newtonsoft.Json.Serialization;
using Spine.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using static Define;

public class Player : Creature
{
    public bool isNeedArrange { get; set; }//정리가 필요한지

    // Order인지 공격형(Offensive)인지
    EPlayerType _playerType = EPlayerType.None;
    public EPlayerType PlayerType
    {
        get { return _playerType; } 
        set
        {
            if (_playerType != value)
            {
                _playerType = value;
            }
        }
    }
    
    Vector2 _moveDir = Vector2.zero;

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

        ObjectType = EObjectType.Player;

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

    private void Update()
    {
        transform.TranslateEx(_moveDir * Time.deltaTime * Speed);
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
        if (PlayerMoveState == EPlayerMoveState.ForceMove && _moveDir != Vector2.zero)
        {
            CreatureState = ECreatureState.Move;
            return;
        }
    }

    protected override void UpdateMove()
    {
        //0. 이동 상태라면 강제 변경
        if (PlayerMoveState == EPlayerMoveState.ForceMove && _moveDir != Vector2.zero)
        {
            CreatureState = ECreatureState.Move;
            return;
        }

        //TODO 나중에 조이스틱으로 움직이려고 할때 수정이 필요함(Grid 이동때문에 임시 주석처리)
        //if (_moveDir == Vector2.zero) // 조이스틱으로 움직이지않으면 
        //    CreatureState = ECreatureState.Idle; // Idle 상태
    }

    protected override void UpdateSkill()
    {
        base.UpdateSkill();

        if (PlayerMoveState == EPlayerMoveState.ForceMove && _moveDir != Vector2.zero)
        {
            CreatureState = ECreatureState.Move;
            return;
        }

        if (Target.IsValid() == false && _moveDir != Vector2.zero)
        {
            CreatureState = ECreatureState.Move;
            return;
        }
    }

    protected override void UpdateAttack()
    {

    }

    protected override void UpdateDead()
    {
        base.UpdateDead();
    }
    #endregion


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
                PlayerMoveState = EPlayerMoveState.None;//강제 이동 해제
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

    public void PlayerAttack()
    {
        GetSkillRangeTilesPlayer(); // 실제 공격 전 공격 범위 구하기
        //NormalAttack or Skill
        if (Skills.CurrentSkill != Skills.DefaultSkill) // 현재 사용하는 스킬이 있다면
        {
            // Skill
        }
        else // 준비한 스킬이 없다면
        {
            List<Creature> AttackableMonster = new List<Creature>();
            // NormalAttack
            foreach (Creature monster in Managers.Turn.activeMonsterList)
            {
                if (SkillRangeTiles.Contains(monster.currentStandingTile))
                {
                    AttackableMonster.Add(monster);
                }

            }

            AttackableMonster.Sort((a, b) => a.Hp.CompareTo(b.Hp)); // 체력 낮은 순으로 sorting

            if (AttackableMonster.Count > 0)
            {
                Target = AttackableMonster.First();
            }

        }
    }
}
