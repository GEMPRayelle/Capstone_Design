using Data;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;
using static Define;
using static UnityEngine.GraphicsBuffer;

public class ItemHolder : BaseObject
{
    private SpriteRenderer _spriteRenderer;
    private ParabolaMotion _parabolaMotion;
    private CircleCollider2D _circleCollider;
    private float exp;
    private bool istracking = false;
    private DropTableData _data;
    private float TransitionDuration = 0.7f; // 타겟 변경 시 전환 시간
    private LayerMask playerMask;
    private Ease TransitionEase = Ease.InCirc;

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        ObjectType = Define.EObjectType.ItemHolder;
        _spriteRenderer = gameObject.GetOrAddComponent<SpriteRenderer>();
        _parabolaMotion = gameObject.GetOrAddComponent<ParabolaMotion>();
        _circleCollider = gameObject.GetOrAddComponent<CircleCollider2D>();
        Physics2D.queriesHitTriggers = true;
        _circleCollider.isTrigger = true;
        _circleCollider.radius = 2.0f;
        _spriteRenderer.sortingOrder = 200;
        playerMask = 1 << LayerMask.NameToLayer("Player");
        return true;
    }

    public void SetInfo(int itemHolderId, int itemDataId, Vector2 pos)
    {
        _data = Managers.Data.DropTableDic[itemHolderId];
        exp = _data.RewardExp;
        //TODO 드랍할 아이템도 DataSheet로 연동
        _spriteRenderer.sprite = Managers.Resource.Load<Sprite>("wizard_shot.sprite");
        _parabolaMotion.SetInfo(0, transform.position, pos, endCallback: Arrived);
    }

    //아이템이 나오고 착지하는순간 천천히 사라지게함
    void Arrived()
    {
        _spriteRenderer.DOFade(0, 3f).OnComplete(() =>
        {

            //if (_data != null)
            //{
            //    //TODO => Acquire Item
            //}

            if (istracking == false)
                Managers.Object.Despawn(this);
        });
    }

    public void Update()
    {
        if (istracking == true)
            return;

        Collider2D player = Physics2D.OverlapCircle(transform.position, ITEM_DETECTION_DISTANCE, playerMask);

        if (player != null)
        {
            istracking = true;
            Managers.Game.Gauge += exp;
            DWMovetoTarget(player, TransitionDuration, TransitionEase);
        }
    }

    public void DWMovetoTarget(Collider2D Target, float transitionDuration, Ease transitionEase)
    {
        Vector3 startPos = transform.position;
        DOTween.To(() => 0f, x =>
        { // () => 0f -> 시작값 : 0, x => {} 매 프레임마다 실행되는 콜백(x는 0에서 1로 변화), 1f = 최종값
            if (Target != null)
            {
                Vector3 currentTargetPos = new Vector3(Target.transform.position.x, Target.transform.position.y, 0);
                transform.position = Vector3.Lerp(startPos, currentTargetPos, x);
            }
        }, 1f, transitionDuration) // 1f = x의 최종값, transitionDuration = 애니메이션 지속 시간
        .SetEase(transitionEase) // 변환 타입 설정, 위에 정의에 주석 볼것
        .OnComplete(() =>
        { // 끝나면
            Managers.Object.Despawn(this);
        });
    }
    
}
