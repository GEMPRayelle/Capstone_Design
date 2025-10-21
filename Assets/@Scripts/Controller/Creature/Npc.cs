using Data;
using Spine.Unity;
using UnityEngine;
using static Define;

public interface INpcInteraction
{
    public void SetInfo(Npc owner); 
    public void HandleOnClickEvent(); //Npc 클릭시 실행할 이벤트
    public bool CanInteract(); //npc 종류에 따라 상호작용이 가능한지 식별
}

public class Npc : BaseObject
{
    public NpcData Data { get; set; }
    public ENpcType NpcType { get { return Data.NpcType; } }
    public INpcInteraction Interaction { get; private set; }

    private SkeletonAnimation _skeletonAnim;
    private UI_NpcInteraction _ui;

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        ObjectType = EObjectType.Npc;
        return true;
    }

    private void Update()
    {
        if(Interaction != null && !Interaction.CanInteract())
        {
            _ui.gameObject.SetActive(false);
        }
        else
        {
            _ui.gameObject.SetActive(true);
        }
    }

    public void SetInfo(int dataId)
    {
        //Data = Managers.Data.NpcDict[dataId];
        gameObject.name = $"{Data.DataId}_{Data.Name}";

        #region Spine Animation
        SetSpineAnimation(Data.SkeletonDataID, SortingLayers.NPC);
        PlayAnimation(0, AnimName.IDLE, true);
        #endregion

        //Npc상호작용을 위한 버튼
        GameObject button = Managers.Resource.Instantiate("UI_NpcInteraction", gameObject.transform);
        button.transform.localPosition = new Vector3(0f, 3);
        _ui = button.GetComponent<UI_NpcInteraction>();
        _ui.SetInfo(DataTemplateID, this);

        //실행하고 있는 npc에 따라 처리
        switch (Data.NpcType)
        {
            //Quest타입이면
            case ENpcType.Quest:
                //QuestInteraction을 붙여서
                //Interaction = new QuestInteraction();
                break;
            //대화 형식의 타입이면
            case ENpcType.Communication:
                //Interaction = new Communication();
                break;

        }

        //초기화하는 부분을 실행
        Interaction?.SetInfo(this);
    }

    public virtual void OnClickEvent()
    {
        Interaction?.HandleOnClickEvent();
    }
}
