using System;
using UnityEngine;
using UnityEngine.UI;
using static Define;

public class UI_TagGauge : UI_Base
{
    enum Sliders
    {
        UI_Slider,
    }

    private Slider _tagGauge;

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        BindSliders(typeof(Sliders));

        gameObject.GetComponent<Canvas>().sortingOrder = SortingLayers.UI_TagGauge;

        _tagGauge = GetSlider((int)Sliders.UI_Slider);

        Managers.Game.OnGaugeChanged -= HandleGaugeChanged;
        Managers.Game.OnGaugeChanged += HandleGaugeChanged;


        return true;
    }

    #region Event Func
    private void HandleGaugeChanged(float percent)
    {
        _tagGauge.value = percent;
    }
    #endregion
}
