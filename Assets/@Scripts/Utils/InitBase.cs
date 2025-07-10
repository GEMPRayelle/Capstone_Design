using UnityEngine;

/// <summary>
/// 모든 오브젝트들의 기본적인 초기화를 담당하는 조상 클래스
/// </summary>
public class InitBase : MonoBehaviour
{
    protected bool _init = false; //초기화 여부
    public virtual bool Init()
    {
        if (_init) //한번이라도 초기화를 했으면
            return false; //실패

        _init = true; //그게 아닐시 초기화 진행
        return true;
    }

    //자식 클래스에서 기본적으로 Awake를 만들어주지 않았다면
    private void Awake()
    {
        //기본적으로 Init을 호출
        Init();
    }
}
