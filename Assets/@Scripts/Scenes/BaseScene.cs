using UnityEngine;
using UnityEngine.EventSystems;
using static Define;

//기본적인 Scene들이 상속받을 조상 클래스
public abstract class BaseScene : InitBase
{
    //초기 Scene타입은 Unknown이지만 상속을 받아서 수정이 필요
    public EScene SceneType { get; protected set; } = EScene.UnKnown;

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        Object obj = FindFirstObjectByType<EventSystem>();

        //인위적으로 EventSystem을 생성
        if (obj == null)
        {
            GameObject go = new GameObject() { name = "@EventSystem" };
            go.AddComponent<EventSystem>();
            go.AddComponent<StandaloneInputModule>();
        }

        return true;
    }

    /// <summary>
    /// Scene을 밀어주는 추상 함수
    /// </summary>
    public abstract void Clear();
}
