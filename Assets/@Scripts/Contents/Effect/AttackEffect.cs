using System.Collections;
using UnityEngine;
using static Define;

public class AttackEffect : BaseObject
{
    public Creature Owner { get; private set; } //이펙트를 쏜 주인
    public SkillBase Skill { get; private set; } //사용한 스킬
    private SpriteRenderer _spriteRenderer;

    private float _animationDuration = 0.3f;

    // 공격 효과 점점 크게
    private Vector3 _startScale = new Vector3(0.2f, 0.2f, 1f);
    private Vector3 _endScale = new Vector3(1.5f, 1.5f, 1f);

    // AnimationCurve : 시간에 따른 값의 변화를 부드럽게 조절, 
    // EaseInOut : 부드러운 시작과 끝
    private AnimationCurve _scaleCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f); // 인자 : 시작x, 시작y, 목표x, 목표y

    private Coroutine _animationCoroutine;

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

    public void Start()
    {

        SetRotationToTarget(Owner.Target.transform);
        // 애니메이션 시작
        StartSlashAnimation();
    }

    /// 칼질 애니메이션을 시작합니다
    private void StartSlashAnimation()
    {
        // 시작 스케일과 회전 설정
        transform.localScale = _startScale;

        // 애니메이션 실행
        if (_animationCoroutine != null)
            StopCoroutine(_animationCoroutine);
        _animationCoroutine = StartCoroutine(CoSlashAnimation());
    }

    public void SetRotationToTarget(Transform target)
    {
        if (target == null)
            return;

        // 2D에서 타겟 방향 계산
        Vector2 direction = (target.position - Owner.transform.position).normalized;
        // 2D에서 방향에 따라 회전값 계산
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        // 스프라이트가 왼쪽을 향하고 있다고 가정하고 조정 (필요에 따라 각도 조정)
        transform.rotation = Quaternion.Euler(0, 0, angle - 160.0f);
    }


    /// 칼질 애니메이션 코루틴
    private IEnumerator CoSlashAnimation()
    {
        float elapsed = 0f;
        Vector3 startScale = _startScale;
        Vector3 targetScale = _endScale;

        // 알파값도 조절하여 더 자연스럽게
        Color startColor = _spriteRenderer.color;
        Color endColor = new Color(startColor.r, startColor.g, startColor.b, 1.0f);

        while (elapsed < _animationDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / _animationDuration;  // 0~1 선형 증가 t값
            float curveT = _scaleCurve.Evaluate(t); // 0~1 비선형(부드러운 시작과 끝) 증가 t값

            // 스케일 애니메이션
            transform.localScale = Vector3.Lerp(startScale, targetScale, curveT);

            // 알파 애니메이션 (페이드 아웃)
            Color currentColor = Color.Lerp(startColor, endColor, t);
            _spriteRenderer.color = currentColor;

            yield return null;
        }

        // 최종값 설정
        transform.localScale = targetScale;
        _spriteRenderer.color = endColor;

        // 페이드 아웃 시작
        StartCoroutine(CoFadeOut());
    }

    /// 이펙트 페이드 아웃
    private IEnumerator CoFadeOut()
    {
        float fadeTime = 0.2f;
        float elapsed = 0f;
        Color startColor = _spriteRenderer.color;
        Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0f);

        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeTime;
            _spriteRenderer.color = Color.Lerp(startColor, endColor, t);
            yield return null;
        }

        _spriteRenderer.color = endColor;
    }



    /// <summary>
    /// 애니메이션 설정을 런타임에 변경할 수 있는 함수
    /// </summary>
    /// <param name="duration">애니메이션 지속시간</param>
    /// <param name="startScale">시작 크기</param>
    /// <param name="endScale">끝 크기</param>
    public void SetAnimationSettings(float duration, Vector3 startScale, Vector3 endScale)
    {
        _animationDuration = duration;
        _startScale = startScale;
        _endScale = endScale;
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