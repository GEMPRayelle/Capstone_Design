using Spine.Unity;
using UnityEngine;
using static Define;

public class Monster : Creature
{
    public Rigidbody2D target;
    public Rigidbody2D rigid;
    
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
                        UpdateAITick = 0.5f;
                        break;
                    case ECreatureState.Attack:
                        UpdateAITick = 0.02f;
                        break;
                    case ECreatureState.Move:
                        UpdateAITick = 0.02f;
                        break;
                    case ECreatureState.Dead:
                        UpdateAITick = 1.0f;
                        break;

                }
            }
        }

    }
    public override bool Init()
    {
        if (base.Init() == false)
           return false;

        CreatureType = ECreatureType.Monster;
        CreatureState = ECreatureState.Idle;

        Speed = 2.5f;
        rigid = GetComponent<Rigidbody2D>();
        SkeletonAnimation skeletonAnim = GetComponent<SkeletonAnimation>();
        skeletonAnim.GetComponent<MeshRenderer>().sortingOrder = SortingLayers.MONSTER;
        Collider.radius = 0.25f;
        Collider.offset = transform.InverseTransformPoint(transform.position + (Vector3.right * 0.5f + Vector3.up) * ColliderRadius);
        
        GameObject player = GameObject.Find("@Players");

        if (player != null)
            target = player.transform.GetChild(0).GetComponent<Rigidbody2D>();

        StartCoroutine(CoUpdateAI());

        return true;
    }

    public override void SetInfo(int templateID)
    {
        base.SetInfo(templateID);

        CreatureState = ECreatureState.Idle;
    }

    public void Update()
    {
        
    }

    protected override void UpdateIdle()
    {
        CreatureState = ECreatureState.Move;
    }

    protected override void UpdateMove()
    {
        // 플레이어 따라가기(Test용)
        Vector2 dirVec = target.position - rigid.position;
        Vector2 nextVec = dirVec.normalized * Speed * Time.deltaTime;

        rigid.MovePosition(rigid.position + nextVec);
        rigid.linearVelocity = Vector2.zero;

        if (dirVec.x < 0.1)
            LookLeft = true;
        else if (dirVec.x > 0.1)
            LookLeft = false;
        //else
        //    ;

    }
    protected override void UpdateDead()
    {

    }

   


}
