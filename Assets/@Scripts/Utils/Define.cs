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
        Default = 0,
        TransparentFX = 1,
        IgnoreRaycast = 2,
        Dummy1 = 3,
        Water = 4,
        UI = 5,
        Player = 6,
        Monster = 7,
        Env = 8,
        Obstacle = 9,
        Projectile = 10,
    }

    public enum ESkillSlot
    {
        Default,
        Env,
        A,
        B
    }

    public enum EEffectSize
    {
        CircleSmall,
        CircleNormal,
        CircleBig,
        ConeSmall,
        ConeNormal,
        ConeBig,
    }


    public static class SortingLayers
    {
        public const int UI_TAGBTN = 600;
        public const int UI_JOYSTICK = 500;
        public const int CREATURE = 300;
        public const int PLAYER = 350;
        public const int PROJECTILE = 310;
        public const int MONSTER = 300;
    }

    public static class AnimName
    {
        public const string IDLE = "idle";
        public const string MOVE = "move";
        public const string DEAD = "dead";
        public const string ATTACK_A = "attack";
        public const string ATTACK_B = "attack";
        public const string SKILL_A = "skill";
        public const string SKILL_B = "skill";
        public const string DAMAGED = "hit";
        public const string EVENT_ATTACK_A = "event_attack";
        public const string EVENT_ATTACK_B = "event_attack";
        public const string EVENT_SKILL_A = "event_attack";
        public const string EVENT_SKILL_B = "event_attack";
    }


    //Hard Coding
    public const float PLAYER_SEARCH_DISTANCE = 5.0f;

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
