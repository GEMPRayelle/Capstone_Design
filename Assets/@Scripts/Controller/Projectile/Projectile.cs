using System;
using System.Collections;
using UnityEngine;
using static Define;

//발차체
public class Projectile : BaseObject
{
    public Creature Owner { get; private set; } //발사체를 쏜 주인
    public SkillBase Skill { get; private set; } //사용한 스킬
    public Data.ProjectileData ProjectileData { get; private set; }
    public ProjectileMotionBase ProjectileMotion { get; private set; }

    private SpriteRenderer _spriteRenderer;

    public override bool Init()
    {
        if (base.Init() == false) 
            return false;

        ObjectType = Define.EObjectType.Projectile;
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _spriteRenderer.sortingOrder = SortingLayers.PROJECTILE;

        return true;
    }

    public void SetInfo(int templateId)
    {
        ProjectileData = Managers.Data.ProjectileDic[templateId];
        _spriteRenderer.sprite = Managers.Resource.Load<Sprite>(ProjectileData.ProjectileSpriteName);

        //Debugging
        if (_spriteRenderer.sprite == null)
        {
            Debug.LogWarning($"Projectile Sprite Missing {ProjectileData.ProjectileSpriteName}");
            return;
        }
    }

    /// <summary>
    /// 발사체의 스폰 관련 값 세팅을 하는 함수
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

        if (ProjectileMotion != null)
            Destroy(ProjectileMotion);

        string componentName = ProjectileData.ComponentName;
        ProjectileMotion = gameObject.AddComponent(Type.GetType(componentName)) as ProjectileMotionBase;

        StraightMotion straightMotion = ProjectileMotion as StraightMotion;
        if (straightMotion != null)
            straightMotion.SetInfo(ProjectileData.DataId, owner.CenterPosition, owner.Target.CenterPosition, () => { Managers.Object.Despawn(this); });

        ParabolaMotion parabolaMotion = ProjectileMotion as ParabolaMotion;
        if (parabolaMotion != null)
            parabolaMotion.SetInfo(ProjectileData.DataId, owner.CenterPosition, owner.Target.CenterPosition, () => { Managers.Object.Despawn(this); });

        StartCoroutine(CoReserveDestroy(5.0f));
    }


    //무엇인가 부딪치면
    private void OnTriggerEnter2D(Collider2D collision)
    {
        BaseObject target = collision.GetComponent<BaseObject>();
        if (target.IsValid() == false)
            return;

        //타겟에게 데미지 적용
        target.OnDamaged(Owner, Skill);
        Managers.Object.Despawn(this);
    }

    private IEnumerator CoReserveDestroy(float lifeTime)//투사체 파괴 예약
    {
        yield return new WaitForSeconds(lifeTime);
        Managers.Object.Despawn(this);
    }
}
