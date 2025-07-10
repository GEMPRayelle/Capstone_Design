using UnityEngine;

/// <summary>
/// 상수값 또는 Enum 변수들을 모아놓은 클래스
/// </summary>
public static class Define
{
    //씬의 타입
    public enum EScene
    {
        UnKnown,
        TitleScene,
        GameScene,
        DevScene,
    }

    //UI 이벤트 타입
    public enum EUIEvent
    {
        Click,//클릭
        PointerDown,//마우스 누르는순간
        PointerUp,//마우스 때는 순간
        Drag,//누르면서 드래그
    }

    //Sound 타입
    public enum ESound
    {
        Bgm,
        Effect,
        Max,
    }

    //Object들의 타입
    public enum EObjectType
    {
        None,
        Creature,//생명체
        Env,//주변과 상호작용할 오브젝트
    }
}
