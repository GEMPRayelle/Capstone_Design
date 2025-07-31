using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;
using UnityEngine.WSA;
using static Define;

public class UI_TagBtn : UI_Base
{
    enum GameObjects
    {
        TagBG, // 백그라운드 이미지
        TagButton // 버튼 이미지
    }

    private GameObject _bg;
    private GameObject _button;

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        BindObjects(typeof(GameObjects));

        gameObject.GetComponent<Canvas>().sortingOrder = SortingLayers.UI_TAGBTN;

        _bg = GetObject((int)GameObjects.TagBG);
        _button = GetObject((int)GameObjects.TagButton);

        gameObject.BindEvent(OnClick, type: Define.EUIEvent.Click);

        return true;
    }

    #region Event Func
    public void OnClick(PointerEventData eventData)
    {
        Managers.Game.InverserPlayerState();
    }

    #endregion
}
