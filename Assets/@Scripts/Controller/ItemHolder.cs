using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;

public class ItemHolder : BaseObject
{
    private SpriteRenderer _spriteRenderer;
    private ParabolaMotion _parabolaMotion;
    private CircleCollider2D _circleCollider;
    private float exp = 50.0f;
    private bool istracking = false;

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        ObjectType = Define.EObjectType.ItemHolder;
        _spriteRenderer = gameObject.GetOrAddComponent<SpriteRenderer>();
        _parabolaMotion = gameObject.GetOrAddComponent<ParabolaMotion>();
        _circleCollider = gameObject.GetOrAddComponent<CircleCollider2D>();

        _circleCollider.isTrigger = true;
        _circleCollider.radius = 2.0f;
        _spriteRenderer.sortingOrder = 200;
        return true;
    }

    public void SetInfo(int itemHolderId, int itemDataId, Vector2 pos)
    {
        //_data = Managers.Data.ItemDic[itemDataId];
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

    private void OnTriggerEnter2D(Collider2D collision)
    {
        AttackEffect effect = collision.GetComponent<AttackEffect>();
        Creature player = effect.Owner;
        if (player.IsValid() == false) 
            return;

        istracking = true;
        Managers.Game.Gauge += exp;

        Managers.Object.Despawn(this);

        // 플레이어 방향으로 부드럽게 이동
        //transform.DOMove(player.CenterPosition, 1f)
        //    .SetEase(Ease.InOutSine)
        //    .OnComplete(() =>
        //    {
        //        Managers.Object.Despawn(this);
        //    });



    }
}
