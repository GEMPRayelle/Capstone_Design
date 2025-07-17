using UnityEngine;
using static Define;

public class Monster : Creature
{
    public Rigidbody2D target;
    public Rigidbody2D rigid;

    public float speed;

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
                        UpdateAITick = 0.0f;
                        break;
                    case ECreatureState.Attack:
                        UpdateAITick = 0.1f;
                        break;
                    case ECreatureState.Dead:
                        UpdateAITick = 0.0f;
                        break;

                }
            }
        }

    }
    public override bool Init()
    {
        if (base.Init() == false)
           return false;

        ObjectType = EObjectType.Monster;

        StartCoroutine(CoUpdateAI());

        return true;
    }

    public override void SetInfo(int templateID)
    {
        base.SetInfo(templateID);
        CreatureState = ECreatureState.Idle;
    }

    public void Start()
    {
        // 임시 테스트 코드
        CreatureState = ECreatureState.Attack;

        speed = 2.5f;
        rigid = GetComponent<Rigidbody2D>();

        GameObject player = GameObject.Find("Player");

        if (player != null)
            target = player.GetComponent<Rigidbody2D>();
    }

    protected override void UpdateIdle()
    {
        
    }

    protected override void UpdateAttack()
    {
        

    }

    protected override void UpdateDead()
    {

    }

    public void Update()
    {
        // 플레이어 따라가기(Test용)
        Vector2 dirVec = target.position - rigid.position;
        Vector2 nextVec = dirVec.normalized * speed * Time.deltaTime;

        rigid.MovePosition(rigid.position + nextVec);
        rigid.linearVelocity = Vector2.zero;
    }
}
