using DG.Tweening;
using UnityEngine;

public class ItemHolder : BaseObject
{
    private SpriteRenderer _spriteRenderer;
    private ParabolaMotion _parabolaMotion;

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        ObjectType = Define.EObjectType.ItemHolder;
        _spriteRenderer = gameObject.GetOrAddComponent<SpriteRenderer>();
        _parabolaMotion = gameObject.GetOrAddComponent<ParabolaMotion>();
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
        _spriteRenderer.DOFade(0, 1f).OnComplete(() =>
        {
            //if (_data != null)
            //{
            //    //TODO => Acquire Item
            //}

            Managers.Object.Despawn(this);
        });
    }
}
