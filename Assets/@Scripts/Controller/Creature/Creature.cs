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
    public virtual ECreatureState CreatureState
    {
        get { return _creatureState; }
        set
        {
            if (_creatureState != value)
            {
                _creatureState = value;
                //UpdateAnimation();
            }
        }
    }

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        ObjectType = EObjectType.Creature;
        CreatureState = ECreatureState.Idle;

        return true;
    }

    public virtual void SetInfo(int templateID)
    {

    }

    protected override void UpdateAnimation()
    {
        switch (CreatureState)
        {
            case ECreatureState.Idle:
                break;
            case ECreatureState.Attack:
                break;
            case ECreatureState.Skill:
                break;
            case ECreatureState.Move:
                break;
            case ECreatureState.Dead:
                RigidBody.simulated = false;
                break;
            default:
                break; 
        }
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

        if(UpdateAITick > 0)
            yield return new WaitForSeconds(UpdateAITick);
        else
            yield return null;
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
