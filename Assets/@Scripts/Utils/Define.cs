using Spine;
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
        DormitoryScene,
        TalkingScene
    }

    //UI 이벤트 타입
    public enum EUIEvent
    {
        Click,//클릭
        PointerDown,//마우스 누르는순간
        PointerUp,//마우스 때는 순간
        Drag,//누르면서 드래그
        OnValueUpdate,
        PointerEnter,
        PointerExit
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
        Player,
        Env,//주변과 상호작용할 오브젝트
        Monster,
        Npc,
        Projectile, //발사체
        Effect,
        ItemHolder,
        Tile
    }

    public enum ECreatureState
    {
        None,
        Idle,
        Move,
        Attack,
        Skill,
        OnDamaged, //CC상태
        Dead
    }

    public enum ENpcType
    {
        None,
        StartPosition,
        Guild,
        Portal,
        Waypoint,
        BlackSmith,
        Training,
        TreasureBox,
        Dungeon,
        Quest,
        GoldStorage,
        WoodStorage,
        MineralStorage,
        ExChange,
        RuneStone,
        Talking,
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
        TargetMonster,//몬스터를 찾아서 전투
        CollectEnv,//주변과 상호작용
        ReturnToPos,//어딘가 정해진 위치로 이동해야할때
        ForceMove,//강제 이동
        ForcePath//전체를 탐색하더라도 찾아가게함
    }
    public enum EPlayerControlState
    {
        None, // 조종 X 상태
        Move, // 이동 명령 대기 중 상태(이동 중 아님)
        Spawn, // 스폰 중일때
        Skill, // Skill만 쓸 수 있을 때
        UI, // UI 소환됬을 때

    }

    public enum EPlayerType
    {
        None,
        Order, //오더
        Offensive,
        GoingFirst, //선공, 자신의 턴이 종료될 시 행동
        GoingSecond, //후공, 적의 턴이 종료될 시 행동
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
        Effect = 11,
    }

    public enum ESkillSlot
    {
        Default,
        Env,
        A,
        B
    }

    public enum EIndicatorType
    {
        None,
        Cone,
        Rectangle,
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

    public enum EStatModType
    {
        Add,//덧셈
        PercentAdd, //%덧셈
        PercentMult, //%곱셈
    }

    public enum EEffectType
    {
        Buff,
        Debuff,
        CrowdControl,
    }

    public enum EEffectSpawnType
    {
        Skill, //지속시간이 있는 기본적인 이펙트
        External, //외부(장판스킬)에서 이펙트를 관리(지속시간에 영향을 받지않음)
    }

    public enum EEffectClearType
    {
        TimeOut,//시간초과로 인한 Effect종료
        ClearSkill,//정화 스킬로 인한 Effect종료
        TriggerOutAoE,//AoE스킬을 벗어난 경우 종료
        EndOfAirbone,//에어본이 끝난 경우 호출되는 종료
    }

    public enum ArrowDirection 
    {
        None = 0,           // 방향 없음 (화살표 숨김)
        Up = 1,             // 위쪽 방향
        Down = 2,           // 아래쪽 방향
        Left = 3,           // 왼쪽 방향
        Right = 4,          // 오른쪽 방향
        TopLeft = 5,        // ↖ 대각선 방향
        BottomLeft = 6,     // ↙ 대각선 방향
        TopRight = 7,       // ↗ 대각선 방향
        BottomRight = 8,    // ↘ 대각선 방향
        UpFinished = 9,     // 마지막 타일에서 위쪽 방향
        DownFinished = 10,  // 마지막 타일에서 아래쪽 방향
        LeftFinished = 11,  // 마지막 타일에서 왼쪽 방향
        RightFinished = 12  // 마지막 타일에서 오른쪽 방향
    }

    public enum ECellCollisionType
    {
        None, //갈 수 있는 곳
        SemiWall, //카메라만 갈 수 있는 곳
        Wall, //지나갈 수 없는 곳
    }

    public enum TurnSorting
    {
        ConstAttribute, //고정 속성
        CTB, //Charge Turn Battle
    }

    public enum TurnPhase //턴 단계 구분
    {
        PlayerMovement, //캐릭터 이동
        PlayerAction, //턴 종료 버튼 누를 시 공격/스킬 처리
        EnemyTurn, //적 턴 진행
    }


    public static class SortingLayers
    {
        public const int UI_GAMESCENE = 1;
        public const int UI_JOYSTICK = 500;
        public const int UI_HpBar = 400;
        public const int SPELL_INDICATOR = 200;
        public const int CREATURE = 350;
        public const int PROJECTILE = 310;
        public const int SKILL_EFFECT = 310;
        public const int PLAYER = 300;
        public const int MONSTER = 300;
        public const int NPC = 300;
    }

    public static class AnimName
    {
        public const string IDLE = "idle";
        public const string MOVE = "move";
        public const string DEAD = "dead";
        public const string ATTACK = "attack";
        public const string ATTACK_A = "attack_a";
        public const string ATTACK_B = "attack_b";
        public const string SKILL = "skill";
        public const string SKILL_A = "skill_a";
        public const string SKILL_B = "skill_b";
        public const string DAMAGED = "hit";
        public const string EVENT_ATTACK_A = "event_attack";
        public const string EVENT_ATTACK_B = "event_attack";
        public const string EVENT_SKILL_A = "event_attack";
        public const string EVENT_SKILL_B = "event_attack";
    }

    public const float EFFECT_SMALL_RADIUS = 2.5f;
    public const float EFFECT_NORMAL_RADIUS = 4.5f;
    public const float EFFECT_BIG_RADIUS = 5.5f;

    //Hard Coding
    public const float PLAYER_SEARCH_DISTANCE = 4.0f;
    public const float MONSTER_SEARCH_DISTANCE = 8.0f;
    public const float ITEM_DETECTION_DISTANCE = 10.0f;
    public const int HERO_WIZARD_ID = 201000;
    public const int HERO_KNIGHT_ID = 201001;
    public const int HERO_LION_ID = 201003;
    public const int HERO_VAMPIRE_ID = 201004;

    public const int MONSTER_SLIME_ID = 202001;
    public const int MONSTER_SPIDER_COMMON_ID = 202002;
    public const int MONSTER_WOOD_COMMON_ID = 202004;
    public const int MONSTER_GOBLIN_ARCHER_ID = 202005;
    public const int MONSTER_BEAR_ID = 202006;

    public const char MAP_TOOL_WALL = '0';//벽이라서 갈 수 없음
    public const char MAP_TOOL_NONE = '1';//모두가 가능
    public const char MAP_TOOL_SEMI_WALL = '2';//카메라만 가능

    public const string OUTLINE_KEYWORD = "_USE8NEIGHBOURHOOD_ON";
}
