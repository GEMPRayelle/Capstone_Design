using Data;
using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using static Define;

public class Creature : BaseObject
{
    public float Speed { get; protected set; } = 1.0f;
    public ECreatureType CreatureType { get; protected set; } = ECreatureType.None;
    protected ECreatureState _creatureState = ECreatureState.None;
    protected EPlayerState activePlayerState { get; private set; } = EPlayerState.None; // 현재 활성화된 플레이어 스테이트(마스터, 서번트)에 대한 정보. 가져오기만 하면 됨



    public Data.CreatureData CreatureData { get; protected set; }

    public virtual ECreatureState CreatureState
    {
        get { return _creatureState; }
        set
        {
            //Creature의 State가 바뀔때마다 UpdateAnimation을 자동으로 호출하여 애니메이션이 재생되도록함
            if (_creatureState != value)
            {
                _creatureState = value;
                //TODO -> UpdateAnimation 구현
                UpdateAnimation();
            }
        }
    }

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        ObjectType = EObjectType.Creature;
        CreatureState = ECreatureState.Idle;

        Managers.Game.OnPlayerStateChanged -= HandleOnPlayerStateChanged;
        Managers.Game.OnPlayerStateChanged += HandleOnPlayerStateChanged;

        return true;
    }

    //001_SkeletonData -> Knight
    public virtual void SetInfo(int templateID)
    {
        DataTemplateID = templateID;

        //데이터를 긁어오지 못했을경우 예외처리없이 크래시처리
        if (CreatureType == ECreatureType.Player)
            CreatureData = Managers.Data.HeroDic[templateID];
        else
            CreatureData = Managers.Data.MonsterDic[templateID];
        
        //Name
        gameObject.name = $"{CreatureData.DataId}_{CreatureData.DescriptionTextID}";

        //Collider
        Collider.offset = new Vector2(CreatureData.ColliderOffsetX, CreatureData.ColliderOffstY);
        Collider.radius = CreatureData.ColliderRadius;

        //Spine
        SetSpineAnimation(CreatureData.SkeletonDataID, SortingLayers.CREATURE);

        //State
        CreatureState = ECreatureState.Idle;

    }

    protected override void UpdateAnimation()
    {
        switch (CreatureState)
        {
            case ECreatureState.Idle:
                PlayAnimation(0, AnimName.IDLE, true);
                break;
            case ECreatureState.Attack:
                PlayAnimation(0, AnimName.ATTACK_A, true);
                break;
            case ECreatureState.Skill:
                PlayAnimation(0, AnimName.SKILL_A, true);
                break;
            case ECreatureState.Move:
                PlayAnimation(0, AnimName.MOVE, true);
                break;
            case ECreatureState.Dead:
                PlayAnimation(0, AnimName.DEAD, true);
                RigidBody.simulated = false;
                break;
            default:
                break; 
        }
    }

    // PlayerState 변환 시 작동하는 함수
    private void HandleOnPlayerStateChanged(EPlayerState playerstate) 
    {
        switch (playerstate)
        {
            case EPlayerState.None:
                break;
            case EPlayerState.Master:
                activePlayerState = EPlayerState.Master;
                ChangedMaster();
                break;
            case EPlayerState.Servant:
                activePlayerState = EPlayerState.Servant;
                ChangedServent();
                break;
        }
    }

    protected virtual void ChangedMaster() // 마스터로 변경됬을 때 공통 로직 구현
    {

    }

    protected virtual void ChangedServent() // 서번트로 변경 됬을 때 공통 로직 구현
    {

    }




    #region Battle
    public override void OnDamaged()
    {
        base.OnDamaged();
    }
    #endregion

    #region AI
    public float UpdateAITick { get; protected set; } = 0.0f;

    protected IEnumerator CoUpdateAI()
    {
        while (true)
        {
            switch (CreatureState)
            {
                case ECreatureState.Idle:
                    UpdateIdle();
                    break;
                case ECreatureState.Move:
                    UpdateMove();
                    break;
                case ECreatureState.Attack:
                    UpdateAttack();
                    break;
                case ECreatureState.Skill:
                    UpdateSkill();
                    break;
                case ECreatureState.OnDamaged:
                    UpdateOnDamaged();
                    break;
                case ECreatureState.Dead:
                    UpdateDead();
                    break;
            }

            if (UpdateAITick > 0)
                yield return new WaitForSeconds(UpdateAITick);
            else
                yield return null;
        }
        
    }

    protected virtual void UpdateIdle() { }
    protected virtual void UpdateMove() { }
    protected virtual void UpdateAttack() { }
    protected virtual void UpdateSkill() { }
    protected virtual void UpdateOnDamaged() { }
    protected virtual void UpdateDead() { }
    #endregion

    #region Wait
    protected Coroutine _coWait;
    protected void StartWait(float seconds)
    {
        CancleWait();
        _coWait = StartCoroutine(CoWait(seconds));
    }

    IEnumerator CoWait(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        _coWait = null;
    }

    protected void CancleWait()
    {
        if (_coWait != null)
            StopCoroutine(_coWait);
        _coWait = null;
    }
    #endregion
}
