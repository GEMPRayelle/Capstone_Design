using Spine;
using Spine.Unity;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using static Define;

//Scene의 배치될 모든 오브젝트들의 조상 클래스
public class BaseObject : InitBase
{
    //모든 오브젝트들이 가질 기본적인 컴포넌트
    public EObjectType ObjectType { get; protected set; } = EObjectType.None;
    public CircleCollider2D Collider { get; private set; }
    //public Animation Anim { get; private set; }
    public SkeletonAnimation SkeletonAnim { get; private set; }
    public Rigidbody2D RigidBody { get; protected set; }

    //다른 오브젝트들도 고유 DataID가 있을 경우를 고려
    public int DataTemplateID { get; set; }

    //TODO (수정 필요) -> 오브젝트의 중앙 위치가 아닌 발 위치로 초기 설정되어있음
    public Vector3 CenterPosition { get { return transform.position + Vector3.up * ColliderRadius; } }
    public float ColliderRadius { get { return Collider != null ? Collider.radius : 0.0f; } }

    #region Spine 전용으로 추가한 프로퍼티들
    bool _lookLeft = true;
    public bool LookLeft
    {
        get { return _lookLeft; }
        set
        {
            _lookLeft = value;
            Flip(!value);
        }
    }
    #endregion

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        RigidBody = GetComponent<Rigidbody2D>();
        Collider = gameObject.GetOrAddComponent<CircleCollider2D>();
        //Anim = GetComponent<Animation>();
        SkeletonAnim = GetComponent<SkeletonAnimation>();

        return true;
    }

    protected virtual void OnDisable()
    {
        //TODO
    }

    public void TranslateEx(Vector3 dir)
    {
        transform.Translate(dir);

        if (dir.x < 0)
            LookLeft = true;
        else if (dir.x > 0)
            LookLeft = false;
    }

    #region Animation
    protected virtual void UpdateAnimation()
    {

    }
    #endregion

    #region Helper Func
    /// <summary>
    /// 오브젝트 자기 자신과 obj간의 거리를 반환시키는 함수
    /// </summary>
    public Vector3 GetDistanceTargetFromObj(GameObject obj)
    {
        Vector3 position = gameObject.transform.position;

        //TODO 거리 계산 작성
        
        return position;
    }
    #endregion

    #region Virtual Func
    public virtual void OnDamaged() { }
    public virtual void OnDead() { }
    #endregion

    #region Spine
    protected virtual void SetSpineAnimation(string dataLabel, int sortingOrder)
    {
        if (SkeletonAnim == null)
            return;

        SkeletonAnim.skeletonDataAsset = Managers.Resource.Load<SkeletonDataAsset>(dataLabel);
        SkeletonAnim.Initialize(true);

        // Register AnimEvent
        if (SkeletonAnim.AnimationState != null)
        {
            SkeletonAnim.AnimationState.Event -= OnAnimEventHandler;
            SkeletonAnim.AnimationState.Event += OnAnimEventHandler;
        }

        // Spine SkeletonAnimation은 SpriteRenderer 를 사용하지 않고 MeshRenderer을 사용함
        // 그렇기떄문에 2D Sort Axis가 안먹히게 되는데 SortingGroup을 SpriteRenderer,MeshRenderer을 같이 계산함.
        SortingGroup sg = Util.GetOrAddComponent<SortingGroup>(gameObject);
        sg.sortingOrder = sortingOrder;
    }

    public void PlayAnimation(int trackIndex, string AnimName, bool loop)
    {
        if (SkeletonAnim == null)
            return;

        SkeletonAnim.AnimationState.SetAnimation(trackIndex, AnimName, loop);
    }

    public void AddAnimation(int trackIndex, string AnimName, bool loop, float delay)
    {
        if (SkeletonAnim == null)
            return;

        SkeletonAnim.AnimationState.AddAnimation(trackIndex, AnimName, loop, delay);
    }

    public void Flip(bool flag)
    {
        if (SkeletonAnim == null)
            return;

        SkeletonAnim.Skeleton.ScaleX = flag ? -1 : 1;
    }

    public virtual void OnAnimEventHandler(TrackEntry trackEntry, Spine.Event e)
    {
        Debug.Log("OnAnimEventHandler");
    }
    #endregion
}
