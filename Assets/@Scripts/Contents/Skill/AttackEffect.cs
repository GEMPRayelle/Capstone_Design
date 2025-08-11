using System;
using System.Collections;
using UnityEngine;
using static Define;

public class AttackEffect : BaseObject
{
    public Creature Owner { get; private set; } //이펙트를 쏜 주인
    public SkillBase Skill { get; private set; } //사용한 스킬

    private SpriteRenderer _spriteRenderer;

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        ObjectType = Define.EObjectType.Effect;
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _spriteRenderer.sortingOrder = SortingLayers.PROJECTILE;

        return true;
    }

    public void SetInfo(int templateId)
    {
        // TODO : 스킬 이펙트용 데이터? (EX projectile, 주인공 주위 도는것, 칼 휘둘러치기 등) 
        //EffectData = Managers.Data.EffectData[templateId];
        _spriteRenderer.sprite = Managers.Resource.Load<Sprite>("meelee_attack.sprite");

        //Debugging
        if (_spriteRenderer.sprite == null)
        {
            Debug.LogWarning($"Projectile Sprite Missing meelee_attack.sprite");
            return;
        }
    }

    /// <summary>
    /// 이펙트의 스폰 관련 값 세팅을 하는 함수
    /// </summary>
    /// <param name="owner">누가 스킬을 사용하는지</param>
    /// <param name="skill">어떤 스킬로 공격하는지</param>
    /// <param name="layer">충돌에서 제외할 레이어는 무엇인지</param>
    public void SetSpawnInfo(Creature owner, SkillBase skill, LayerMask layer)
    {
        Owner = owner;
        Skill = skill;

        // Rule
        Collider.excludeLayers = layer;

        StartCoroutine(CoReserveDestroy(1.0f));
    }


    //무엇인가 부딪치면
    private void OnTriggerEnter2D(Collider2D collision)
    {
        BaseObject target = collision.GetComponent<BaseObject>();
        if (target.IsValid() == false)
            return;

        //target.OnDamaged(Owner, Skill);
    }

    private IEnumerator CoReserveDestroy(float lifeTime) //이펙트 파괴 예약
    {
        yield return new WaitForSeconds(lifeTime);
        Managers.Object.Despawn(this);
    }
}
