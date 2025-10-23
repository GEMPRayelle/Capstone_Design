using UnityEngine;

/* 
Canvas (GameObject):
└─ Background(Iamge):

Canvas (GameObject):
└─ CutSceneManager:
     └─ CutSceneGUI
          ├─ NamePanal (캐릭터의 이름, Button)
          │     └─ NameText (Text)
          └─ ChatPanal (대화 내용, Button)
                └─ ChatText (Text)

Canvas : [CutSceneManager가 아래 2개의 프리팹을 로드해서 소유함]
└─ CutScene (컷신용 데이터) -> Prefab
└─ CutSceneGUI (Empty Object) -> Prefab
      ├─ NamePanal (캐릭터의 이름, Button)
      │     └─ NameText (Text)
      └─ ChatPanal (대화 내용, Button)
            └─ ChatText (Text)

 */

public class UI_TalkingScene : UI_Base
{
    enum GameObjects
    {
        
    }
     
    enum Buttons
    {
        NamePanel,
        ChatPanel
    }

    enum Texts
    {
        NameText,
        ChatText
    }

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        return true;
    }


}
