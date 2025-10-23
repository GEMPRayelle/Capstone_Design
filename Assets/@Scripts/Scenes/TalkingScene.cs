using UnityEngine;
using static Define;

public class TalkingScene : BaseScene
{
    public override bool Init()
    {
        if (base.Init() == false) 
            return false;

        SceneType = EScene.TalkingScene;

        #region UI

        Managers.UI.ShowBaseUI<UI_TalkingScene>();
        
        #endregion

        return true;
    }

    public override void Clear()
    {
    }
}
