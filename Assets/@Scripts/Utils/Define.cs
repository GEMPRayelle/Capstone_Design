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
        Monster,
        Projectile //발사체
    }

    public enum ECreatureType 
    {
        None,
        Player,
        Monster,
        Npc
    }


    public enum ECreatureState
    {
        None,
        Idle,
        Move,
        Attack,
        Skill,
        OnDamaged,
        Dead
    }

    public enum EPlayerState
    {
        None,
        Master,
        Servant
    }

    public enum EJoystickState
    {
        PointerDown,
        PointerUp,
        Drag
    }

    public enum EPlayerMoveState
    {
        None,
        ForceMove,
    }

    public enum ELayer
    {
        UI = 5,
        Player = 6,
        Monster = 7,
    }


    public static class SortingLayers
    {
        public const int UI_TAGBTN = 600;
        public const int UI_JOYSTICK = 500;
        public const int CREATURE = 300;
        public const int PLAYER = 350;
        public const int MONSTER = 300;
    }

    public static class AnimName
    {
        public const string IDLE = "idle";
        public const string ATTACK_A = "attack_a";
        public const string ATTACK_B = "attack_b";
        public const string SKILL_A = "skill_a";
        public const string MOVE = "move";
        public const string DEAD = "dead";
    }


    //Hard Coding
    public const int HERO_WIZARD_ID = 201000;
    public const int HERO_KNIGHT_ID = 201001;
    public const int HERO_LION_ID = 201003;
    public const int HERO_VAMPIRE_ID = 201004;

    public const int MONSTER_SLIME_ID = 202001;
    public const int MONSTER_SPIDER_COMMON_ID = 202002;
    public const int MONSTER_WOOD_COMMON_ID = 202004;
    public const int MONSTER_GOBLIN_ARCHER_ID = 202005;
    public const int MONSTER_BEAR_ID = 202006;
}
