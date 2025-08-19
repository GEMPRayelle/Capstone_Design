using Unity.VisualScripting;
using UnityEngine;
using static Define;

public class UI_BattleBarWorldSpace : UI_Base
{
    private enum Texts
    {
        Text_Hp,
    }

    private enum Sliders
    {
        Slider_HP,
    }

    private int _hp;
    private int _maxHP;
    private Canvas _canvas;

    public override bool Init()
    {
        if (base.Init() == false) 
            return false;

        _canvas = GetComponent<Canvas>();
        _canvas.sortingOrder = SortingLayers.UI_HpBar;

        BindTexts(typeof(Texts));
        BindSliders(typeof(Sliders));

        return true;
    }

    public void SetInfo(int hp, int maxHp)
    {
        _hp = hp;
        _maxHP = maxHp;

        RefreshUI();
    }

    public void RefreshUI()
    {
        GetSlider((int)Sliders.Slider_HP).value = (float)_hp / _maxHP;
        GetText((int)Texts.Text_Hp).text = $"{_hp:N0}";
    }
}
