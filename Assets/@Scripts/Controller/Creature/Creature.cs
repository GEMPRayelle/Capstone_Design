using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using static Define;

public class Creature : BaseObject
{
    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        return true;
    }

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

    public virtual void SetInfo(int templateID)
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
}
