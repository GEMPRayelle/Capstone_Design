using Spine;
using Spine.Unity;
using UnityEngine;
using UnityEngine.Rendering;
using static Define;

//Scene의 배치될 모든 오브젝트들의 조상 클래스
public class BaseObject : InitBase
{
    public int ExtraCells { get; set; } = 0;//그리드기반 최적화에서 쓸 추가Cell단위

    //모든 오브젝트들이 가질 기본적인 컴포넌트
    public EObjectType ObjectType { get; protected set; } = EObjectType.None;
    public CircleCollider2D Collider { get; private set; }
    //public Animation Anim { get; private set; }
    public SkeletonAnimation SkeletonAnim { get; private set; }
    public MeshRenderer meshRenderer { get; private set; }
    public Material materialInstance { get; private set; }
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
        meshRenderer = GetComponent<MeshRenderer>();
        materialInstance = Instantiate(meshRenderer.sharedMaterial);
        meshRenderer.material = materialInstance;

        return true;
    }

    protected virtual void OnDisable()
    {
        if (SkeletonAnim == null)
            return;
        if (SkeletonAnim.AnimationState == null)
            return;

        //비활성화 될시 이벤트 제거
        SkeletonAnim.AnimationState.Event -= OnAnimEventHandler;
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

    //타겟 위치에 따라 방향 전환을 시키도록하는 함수
    public void LookAtTarget(BaseObject target)
    {
        Vector2 dir = target.transform.position - transform.position;
        if (dir.x < 0)
            LookLeft = true;
        else
            LookLeft = false;
    }

    public void SetOutline(bool isCheck)
    {
        if (materialInstance == null) return;

        if (isCheck)
        {
            materialInstance.EnableKeyword(OUTLINE_KEYWORD);
            materialInstance.SetFloat("_OutlineWidth", 3.0f);
        }
        else
        {
            materialInstance.DisableKeyword(OUTLINE_KEYWORD);
            materialInstance.SetFloat("_OutlineWidth", 0.0f);
        }
    }
    #endregion

    #region Virtual Func
    public virtual void OnDamaged(BaseObject attacker, SkillBase skill) { }
    public virtual void OnDead(BaseObject attacker, SkillBase skill) { }
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

    public TrackEntry PlayAnimation(int trackIndex, string animName, bool loop)
    {
        if (SkeletonAnim == null)
            return null;

        TrackEntry entry = SkeletonAnim.AnimationState.SetAnimation(trackIndex, animName, loop);

        if (animName == AnimName.DEAD)
            entry.MixDuration = 0;
        else
            entry.MixDuration = 0.2f;

        return entry;
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

    #region Map
    public bool LerpCellPosCompleted { get; protected set; }
    Vector3Int _cellPos;
    public Vector3Int CellPos //Grid상에서 오브젝트 위치
    {
        get { return _cellPos; }
        protected set
        {
            _cellPos = value;
        }
    }

    public void SetCellPos(Vector3Int cellPos, Grid grid, bool forceMove = false)
    {
        CellPos = cellPos;
        LerpCellPosCompleted = false;

        //true일경우 cell위치와 transform위치랑 완벽하게 일치하게 됨
        if (forceMove)//forceMove가 true라면
        {
            //바로 그 위치로 이동함
            transform.position = grid.GetCellCenterWorld(CellPos);
            //보간처리를 무시하고 바로 이동하게함
            LerpCellPosCompleted = true;
        }
    }

    //정확하게 Cell위치에 있지 않은 물체를 자연스럽게 이동시킴
    public void LerpToCellPos(float moveSpeed)
    {
        //true라면 이동이 끝난 상태라 Lerp를 할 필요 없음
        if (LerpCellPosCompleted)
            return;

        //World좌표와 Grid좌표 사이를 보간해야함
        Vector3 destPos = Managers.Map.Cell2World(CellPos);
        Vector3 dir = destPos - transform.position;

        if (dir.x < 0)
            LookLeft = true;
        else
            LookLeft = false;
        if (dir.magnitude < 0.01f)
        {
            transform.position = destPos;
            LerpCellPosCompleted = true;
            return;
        }

        //실제 이동
        float moveDist = Mathf.Min(dir.magnitude, moveSpeed * Time.deltaTime);
        transform.position += dir.normalized * moveDist;
    }
    #endregion
}
