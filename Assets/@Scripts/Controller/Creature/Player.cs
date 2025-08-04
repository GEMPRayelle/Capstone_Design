using Spine.Unity;
using System;
using UnityEngine;
using static Define;

public class Player : Creature
{
    Vector2 _moveDir = Vector2.zero;
    public EPlayerState PlayerState; //Master, Servant 상태를 관리
    private LayerMask MonsterLayer = 1 << 7; // 탐지 할 monster layer
    private float DeteactionRadius = 5.0f; // 탐지 범위
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

        StartCoroutine(CoUpdateAI());

        return true;
    }

    public override void SetInfo(int templateID)
    {
        base.SetInfo(templateID);

        //State
        CreatureState = ECreatureState.Idle;
    }

    public override ECreatureState CreatureState
    {
        get { return base.CreatureState; }
        set
        {
            if (_creatureState != value)
            {
                base.CreatureState = value;
                switch (value)
                {
                    case ECreatureState.Idle:
                        UpdateAITick = 0.2f;
                        break;
                    case ECreatureState.Skill:
                        UpdateAITick = 1.0f;
                        break;
                    case ECreatureState.Attack:
                        UpdateAITick = 1.0f;
                        break;
                    case ECreatureState.Move:
                        UpdateAITick = 0.2f;
                        break;
                    case ECreatureState.Dead:
                        UpdateAITick = 1.0f;
                        break;

                }
            }
        }

    }

    protected override void UpdateIdle()
    {
        base.UpdateIdle();
        CheckEnemy(); // Idle -> Attack 체크

        //TODO 몬스터 탐색 및 공격하는 코드 구현
        //범위 기반으로 몬스터를 찾아서
        //Creature creature = CheckEnemy(PLAYER_SEARCH_DISTANCE, Managers.Object.monsters) as creature;
        //if (creature != null)
        //{
        //    Target = creature;
        //    CreatureState = ECreatureState.Move;
        //    //TODO 상태지정
        //    return;
        //}
    }

    protected override void UpdateMove()
    {
        base.UpdateMove();
        CheckEnemy(); // move -> Attack 체크
    }

    protected override void UpdateSkill()
    {
        base.UpdateSkill();
        CheckEnemy(); // Skill상태에서 적이 근처에 계속 있다면 유지
    }

    protected override void UpdateAttack()
    {
        base.UpdateAttack();
        CheckEnemy();
    }

    private void CheckEnemy() // 근처에 적이 있다면 Attack으로 바꿔주는 함수
    {
        if (PlayerState == EPlayerState.Master) // 마스터는 적 체크 필요 x
            return;

        if (activePlayerState == EPlayerState.Master) // 태그 눌러서 현재 활성화된게 마스터면
            return;                                   // 서번트여도 체크 필요 X

        if (DetectMonster() == true) // 실제 근처 적 체크 함수
        {
            // 적 발견 시 공격 상태로 전환
            CreatureState = ECreatureState.Attack;
            Debug.Log("플레이어가 적을 공격 중...");

        }
        else // 근처에 적이 없다면
        {
            disappearMonster(); 
        }
    }

    // 근처에 몬스터 있다면 true 아니면 false 리턴
    private bool DetectMonster()
    {
        Collider2D monster = Physics2D.OverlapCircle(transform.position, DeteactionRadius, MonsterLayer);

        if (monster != null)
            return true;

        return false;

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

        transform.TranslateEx(_moveDir * Time.deltaTime * Speed);
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
