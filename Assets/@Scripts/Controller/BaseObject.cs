using TMPro.EditorUtilities;
using Unity.VisualScripting;
using UnityEngine;
using static Define;

//Scene의 배치될 모든 오브젝트들의 조상 클래스
public class BaseObject : InitBase
{
    //모든 오브젝트들이 가질 기본적인 컴포넌트
    public EObjectType ObjectType { get; protected set; } = EObjectType.None;
    public CircleCollider2D Collider { get; private set; }
    public Animation Anim { get; private set; }
    public Rigidbody2D RigidBody { get; protected set; }

    //다른 오브젝트들도 고유 DataID가 있을 경우를 고려
    public int DataTemplateID { get; set; }

    //TODO (수정 필요) -> 오브젝트의 중앙 위치가 아닌 발 위치로 초기 설정되어있음
    public Vector3 CenterPosition { get { return transform.position + Vector3.up * ColliderRadius; } }
    public float ColliderRadius { get { return Collider != null ? Collider.radius : 0.0f; } }

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        RigidBody = GetComponent<Rigidbody2D>();
        Collider = gameObject.GetOrAddComponent<CircleCollider2D>();
        Anim = GetComponent<Animation>();

        return true;
    }

    protected virtual void OnDisable()
    {
        //TODO
    }

    public void TranslateEx(Vector3 dir)
    {
        transform.Translate(dir);
        
        //TODO 방향전환 코드
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
}
