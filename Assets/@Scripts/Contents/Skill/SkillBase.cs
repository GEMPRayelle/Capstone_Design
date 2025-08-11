using Spine;
using System.Collections;
using UnityEngine;
using Event = Spine.Event;

//모든 스킬들의 조상클래스
public abstract class SkillBase : InitBase
{
    public Creature Owner { get; protected set; } //스킬을 사용한 주인
    public float RemainCoolTime { get; set; } //스킬의 쿨타임
    public Data.SkillData SkillData { get; protected set; } //스킬 DataSheet

    //Initialize
    public override bool Init()
    {
        if (base.Init() == false) 
            return false;

        return true;
    }

    //생성자 역할
    public virtual void SetInfo(Creature owner, int skillTemplateID)
    {
        Owner = owner;
        SkillData = Managers.Data.SkillDic[skillTemplateID];

        //애니메이션 이벤트, 애니메이션 타이밍에 맞춰서 이벤트 실행
        if (Owner.SkeletonAnim != null && Owner.SkeletonAnim.AnimationState != null)
        {
            Owner.SkeletonAnim.AnimationState.Event -= OnOwnerAnimEventHandler;
            Owner.SkeletonAnim.AnimationState.Event += OnOwnerAnimEventHandler;
        }
    }

    //스킬이 비활성화 될시
    private void OnDisable()
    {
        if (Managers.Game == null) return;
        if (Owner.IsValid() == false) return;
        if (Owner.SkeletonAnim == null) return;
        if (Owner.SkeletonAnim.AnimationState == null) return;

        Owner.SkeletonAnim.AnimationState.Event -= OnOwnerAnimEventHandler;
    }

    //스킬 사용
    public virtual void DoSkill()
    {
        // 준비된 스킬에서 해제
        if (Owner.Skills != null)
            Owner.Skills.ActiveSkills.Remove(this);

        //공격 속도
        float timeScale = 1.0f;

        if (Owner.Skills.DefaultSkill == this)
            Owner.PlayAnimation(0, SkillData.AnimName, false).TimeScale = timeScale;
        else
            Owner.PlayAnimation(0, SkillData.AnimName, false).TimeScale = 1;

        StartCoroutine(CoCountdownCooldown());
    }

    //스킬 카운팅 코루틴
    private IEnumerator CoCountdownCooldown()
    {
        RemainCoolTime = SkillData.CoolTime;
        yield return new WaitForSeconds(SkillData.CoolTime); //쿨타임만큼 대기
        RemainCoolTime = 0;//끝나면 초기화

        // 준비된 스킬에 추가
        if (Owner.Skills != null)
            Owner.Skills.ActiveSkills.Add(this);
    }

    //스킬 캔슬
    public virtual void CancelSkill()
    {

    }

    protected virtual void GenerateProjectile(Creature owner, Vector3 spawnPos)
    {
        Projectile projectile = Managers.Object.Spawn<Projectile>(spawnPos, SkillData.ProjectileId);

        //충돌에 제외할 layer들을 bit Flag로 만듬
        LayerMask excludeMask = 0;
        excludeMask.AddLayer(Define.ELayer.Default);
        excludeMask.AddLayer(Define.ELayer.Projectile);
        excludeMask.AddLayer(Define.ELayer.Env);
        excludeMask.AddLayer(Define.ELayer.Obstacle);

        //Exception
        switch (owner.CreatureType)
        {
            case Define.ECreatureType.Player:
                excludeMask.AddLayer(Define.ELayer.Player);//자기 자신과는 충돌하면 안됨
                break;
            case Define.ECreatureType.Monster:
                excludeMask.AddLayer(Define.ELayer.Monster);
                break;
        }

        projectile.SetSpawnInfo(Owner, this, excludeMask);
    }

    protected virtual void MeeleeAttack(Creature owner, Vector3 spawnPos)
    {
        // 현재 데이터 시트가 없어 templateID = 0
        AttackEffect skilleffect = Managers.Object.Spawn<AttackEffect>(spawnPos, 0);
        LayerMask excludeMask = 0;
        excludeMask.AddLayer(Define.ELayer.Default);
        excludeMask.AddLayer(Define.ELayer.Projectile);
        excludeMask.AddLayer(Define.ELayer.Env);
        excludeMask.AddLayer(Define.ELayer.Obstacle);
        excludeMask.AddLayer(Define.ELayer.Player);

        skilleffect.SetSpawnInfo(Owner, this, excludeMask);
    }

    //애니메이션이 끝났을때 상태를 바꾸는 방식이 아닌
    //항상 상시로 현재의 CreatureState를 Skill상태로 판별하고 거기서 스킬을 수행
    private void OnOwnerAnimEventHandler(TrackEntry trackEntry, Event e)
    {
        // 다른스킬의 애니메이션 이벤트도 받기 때문에 자기꺼만 써야함
        if (trackEntry.Animation.Name == SkillData.AnimName)
            OnAttackEvent();
    }
    protected abstract void OnAttackEvent();
}
