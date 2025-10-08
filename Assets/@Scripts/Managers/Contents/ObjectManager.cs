using NUnit.Framework.Interfaces;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditorInternal.Profiling.Memory.Experimental;
using UnityEngine;
using UnityEngine.Events;
using static Define;

//오브젝트들의 스폰과 디스폰을 관리할 클래스
public class ObjectManager
{
    public HashSet<Player> Players { get; } = new HashSet<Player>();
    public HashSet<Monster> Monsters { get; } = new HashSet<Monster>();
    public HashSet<Projectile> Projectiles { get; } = new HashSet<Projectile>();

    public HashSet<AttackEffect> attackEffects { get; } = new HashSet<AttackEffect>();
    public HashSet<EffectBase> Effects { get; } = new HashSet<EffectBase>();
    public HashSet<ItemHolder> ItemHolders { get; } = new HashSet<ItemHolder>();
    public HashSet<OverlayTile> OverlayTiles { get; } = new HashSet<OverlayTile>();
    public HashSet<GameEventListener> GameEventListeners { get; } = new HashSet<GameEventListener>();
    public HashSet<GameEventGameObjectListener> GameEventGameObjectListeners { get; } = new HashSet<GameEventGameObjectListener>();
    public HashSet<GameEventGameObjectListListener> GameEventGameObjectListListeners { get; } = new HashSet<GameEventGameObjectListListener>();

    public List<Player> LivingPlayerList
    {
        get { return Players.Where(player => player.IsAlive).ToList(); }
    }
    public List<Monster> LivingMonsterList
    {
        get { return Monsters.Where(monster => monster.IsAlive).ToList(); }
    }

    #region Roots
    public Transform PlayerRoot { get { return GetRootTransform("@Players"); } }
    public Transform MonsterRoot { get { return GetRootTransform("@Monsters"); } }
    public Transform ProjectileRoot { get { return GetRootTransform("@Projectiles"); } }
    public Transform EffectRoot { get { return GetRootTransform("@Effects"); } }
    public Transform ItemHolderRoot { get { return GetRootTransform("@ItemHolders"); } }
    public Transform OverlayTileRoot { get { return GetRootTransform("@OverlayTiles"); } }
    public Transform ListenerRoot { get { return GetRootTransform("@Listeners"); } }

    //각각의 오브젝트들을 모을 Root 오브젝트를 생성
    public Transform GetRootTransform(string name)
    {
        //name이라는 이름의 root(GameObject)를 찾는다
        GameObject root = GameObject.Find(name);
        if (root == null)//없다면
            //오브젝트를 새로 생성
            root = new GameObject { name = name };

        //그 오브젝트의 transform 리턴
        return root.transform;
    }
    #endregion

    public void ShowDamageFont(Vector2 position, float damage, Transform parent, bool isCiritical = false)
    {
        //Pooling 방식
        GameObject go = Managers.Resource.Instantiate("DamageFont", pooling: true);
        DamageFont damageText = go.GetComponent<DamageFont>();
        damageText.SetInfo(position, damage, parent, isCiritical);
    }

    public GameObject SpawnGameObject(Vector3 position, string prefabName)
    {
        GameObject go = Managers.Resource.Instantiate(prefabName, pooling: true);
        go.transform.position = position;
        return go;
    }

    public GameObject SpawnTileObject(string tileName, Transform tilePosition)
    {
        //임시로 ToString처리
        GameObject go = Managers.Resource.Instantiate(tileName, tilePosition);
        return go;
    }

    // 리스너 인스턴스 생성하는 함수
    public void InstantiateListener()
    {
        // Addressable에 등록된 모든 리소스 키 가져오기
        // 실제로는 "_Listener"로 끝나는 것들만 필요하므로
        // Addressable Label을 사용하는게 좋아보임

        // 방법 1: 미리 정의된 리스너 이름 리스트 사용
        string[] listenerNames = GetAllListenerNames(); // 이 함수는 아래에서 구현

        foreach (string listenerName in listenerNames)
        {
            // 리스너 생성
            GameObject go = Managers.Resource.Instantiate(listenerName, ListenerRoot);

            if (go == null)
            {
                Debug.LogWarning($"Failed to instantiate listener: {listenerName}");
                continue;
            }

            // GameEventListener 타입 확인 및 추가
            GameEventListener basicListener = go.GetComponent<GameEventListener>();
            if (basicListener != null)
            {
                if (basicListener.Response == null)
                {
                    basicListener.Response = new UnityEvent();
                }
                GameEventListeners.Add(basicListener);
                Debug.Log($"Added GameEventListener: {listenerName}");
                continue;
            }

            // GameEventGameObjectListener 타입 확인 및 추가
            GameEventGameObjectListener goListener = go.GetComponent<GameEventGameObjectListener>();
            if (goListener != null)
            {
                if (goListener.Response == null)
                {
                    goListener.Response = new UnityEvent<GameObject>();
                }

                GameEventGameObjectListeners.Add(goListener);
                Debug.Log($"Added GameEventGameObjectListener: {listenerName}");
                continue;
            }

            // GameEventGameObjectListListener 타입 확인 및 추가, 젤 적게 쓰는거 밑에 두기(탐색 제일 오래 걸리니까)
            GameEventGameObjectListListener goListListener = go.GetComponent<GameEventGameObjectListListener>();
            if (goListListener != null)
            {
                if (goListListener.Response == null)
                {
                    goListListener.Response = new UnityEvent<List<GameObject>>();
                }

                GameEventGameObjectListListeners.Add(goListListener);
                Debug.Log($"Added GameEventGameObjectListListener: {listenerName}");
                continue;
            }

            Debug.LogWarning($"Listener has no recognized component: {listenerName}");
        }

        SetListenerResponse();
    }

    // 각 리스너에 콜백에 대응으로 실행될 함수 등록 (처음 초기화 부분)
    public void SetListenerResponse()
    {
        string[] listenerNames = GetAllListenerNames();

        foreach (string listenerName in listenerNames)
        {
            // GameEventListener 타입 처리
            GameEventListener basicListener = GameEventListeners
                .FirstOrDefault(l => l.gameObject.name == listenerName);
            if (basicListener != null)
            {
                SetBasicListenerResponse(basicListener, listenerName);
                continue;
            }

            // GameEventGameObjectListener 타입 처리
            GameEventGameObjectListener goListener = GameEventGameObjectListeners
                .FirstOrDefault(l => l.gameObject.name == listenerName);
            if (goListener != null)
            {
                SetGameObjectListenerResponse(goListener, listenerName);
                continue;
            }

            // GameEventGameObjectListListener 타입 처리
            GameEventGameObjectListListener goListListener = GameEventGameObjectListListeners
                .FirstOrDefault(l => l.gameObject.name == listenerName);
            if (goListListener != null)
            {
                SetGameObjectListListenerResponse(goListListener, listenerName);
                continue;
            }

            Debug.LogWarning($"Listener not found in any collection: {listenerName}");
        }
    }

    // 각 리스너에 콜백에 대응으로 실행될 함수 등록 (플레이어 부분)
    //public void SetPlayerListenerResponse(GameObject player)
    //{
    //    string[] listenerNames = GetPlayerListenerNames();

    //    foreach (string listenerName in listenerNames)
    //    {
    //        // GameEventListener 타입 처리
    //        GameEventListener basicListener = GameEventListeners
    //            .FirstOrDefault(l => l.gameObject.name == listenerName);
    //        if (basicListener != null)
    //        {
    //            SetBasicListenerResponse(basicListener, listenerName);
    //            continue;
    //        }

    //        // GameEventGameObjectListener 타입 처리
    //        GameEventGameObjectListener goListener = GameEventGameObjectListeners
    //            .FirstOrDefault(l => l.gameObject.name == listenerName);
    //        if (goListener != null)
    //        {
    //            SetGameObjectListenerResponse(goListener, listenerName);
    //            continue;
    //        }

    //        // GameEventGameObjectListListener 타입 처리
    //        GameEventGameObjectListListener goListListener = GameEventGameObjectListListeners
    //            .FirstOrDefault(l => l.gameObject.name == listenerName);
    //        if (goListListener != null)
    //        {
    //            SetGameObjectListListenerResponse(goListListener, listenerName);
    //            continue;
    //        }

    //        Debug.LogWarning($"Listener not found in any collection: {listenerName}");
    //    }
    //}

    // GameEventListener에 대한 Response 설정
    private void SetBasicListenerResponse(GameEventListener listener, string listenerName)
    {
        if (listener.Response == null)
        {
            listener.Response = new UnityEvent();
        }

        // 리스너 이름에 따라 적절한 콜백 등록
        switch (listenerName)
        {
            case "AllmoveFinish_Listener":
                // listener.Response.AddListener(턴 종료 버튼 활성화 함수); 턴 종료 버튼 활성화 함수 등록
                Debug.Log($"Set response for {listenerName}");
                break;
            // 다른 기본 이벤트 리스너 추가
            default:
                Debug.LogWarning($"No response defined for basic listener: {listenerName}");
                break;
        }
    }

    // GameEventGameObjectListener에 대한 Response 설정
    private void SetGameObjectListenerResponse(GameEventGameObjectListener listener, string listenerName)
    {
        if (listener.Response == null)
        {
            listener.Response = new UnityEvent<GameObject>();
        }

        // 리스너 이름에 따라 적절한 콜백 등록
        switch (listenerName)
        {
            case "moveFinish_Listener":
                listener.Response.AddListener(Managers.Turn.OnCreatureMoveFinish);
                Debug.Log($"Set response for {listenerName}");
                break;

            // 필요 시 추가
            //case "playerSpawn_Listener":
            //    listener.Response.AddListener(Managers.Object.SetPlayerListenerResponse);
            //    Debug.Log($"Set response for {listenerName}");
            //    break;

            // 다른 GameObject 이벤트 리스너 추가
            default:
                Debug.LogWarning($"No response defined for GameObject listener: {listenerName}");
                break;
        }
    }

    // GameEventGameObjectListListener에 대한 Response 설정
    private void SetGameObjectListListenerResponse(GameEventGameObjectListListener listener, string listenerName)
    {
        if (listener.Response == null)
        {
            listener.Response = new UnityEvent<List<GameObject>>();
        }

        // 리스너 이름에 따라 적절한 콜백 등록
        switch (listenerName)
        {
            // 필요 시 추가
            // 다른 GameObject List 이벤트 리스너 추가
            default:
                Debug.LogWarning($"No response defined for GameObject List listener: {listenerName}");
                break;
        }
    }
    // 모든 리스너 이름을 반환하는 헬퍼 함수
    private string[] GetAllListenerNames()
    {
        // 일단 하드코딩
        return new string[]
        {
            "moveFinish_Listener",
            // "playerSpawn_Listener"
            "AllmoveFinish_Listener"
            // 필요한 리스너들 추가
        };

        // TODO : Addressable Label 사용
        // Addressable에서 "Listener" 라벨이 붙은 것들을 찾아서 반환
        // 이 경우 ResourceManager에 Label로 검색하는 함수가 필요
    }


    public T Spawn<T>(Vector3 position, int templateId) where T : BaseObject
    {
        string prefabName = typeof(T).Name;//컴포넌트의 오브젝트 이름을 가져옴

        //오브젝트가 Instantiate될때 Awake에서 Init으로 초기화가 진행
        GameObject go = Managers.Resource.Instantiate(prefabName);
        go.name = prefabName;
        go.transform.position = position;

        //오브젝트를 아래에서 타입에 따라 구분
        BaseObject obj = go.GetComponent<BaseObject>();

        //플레이어 타입의 오브젝트라면
        if (obj.ObjectType == EObjectType.Player)
        {
            obj.transform.parent = PlayerRoot;
            Player player = go.GetComponent<Player>();
            Players.Add(player);
            player.SetInfo(templateId);
        }
        //몬스터 타입의 오브젝트라면
        else if (obj.ObjectType == EObjectType.Monster)
        {
            obj.transform.parent = MonsterRoot;
            Monster monster = go.GetComponent<Monster>();
            Monsters.Add(monster);
            monster.SetInfo(templateId);
        }
        else if (obj.ObjectType == EObjectType.Projectile)
        {
            obj.transform.parent = ProjectileRoot;
            Projectile projectile = go.GetComponent<Projectile>();
            Projectiles.Add(projectile);

            projectile.SetInfo(templateId);
        }
        else if(obj.ObjectType == EObjectType.Env)
        {
            //TODO
        }
        else if (obj.ObjectType == EObjectType.Effect)
        {
            obj.transform.parent = EffectRoot;
            AttackEffect attackEffect = go.GetComponent<AttackEffect>();
            attackEffects.Add(attackEffect);
            attackEffect.SetInfo(templateId);
            // TODO Effect만 처리되게
            //EffectBase effect = go.GetComponent<EffectBase>();
            //Effects.Add(effect);
        }
        else if (obj.ObjectType == EObjectType.ItemHolder)
        {
            obj.transform.parent = ItemHolderRoot;

            ItemHolder itemHolder = go.GetOrAddComponent<ItemHolder>();
            ItemHolders.Add(itemHolder);
            // 아이템의 setinfo는 소환하는 몬스터의 OnDead에서 구현
        }
        
        //오브젝트를 스폰하고 그 T타입에 obj를 리턴
        return obj as T;
    }

    public void Despawn<T>(T obj) where T : BaseObject
    {
        EObjectType objectType = obj.ObjectType;

        if (obj.ObjectType == EObjectType.Creature)
        {
            Creature creature = obj.GetComponent<Creature>();
            switch (creature.ObjectType)
            {
                case EObjectType.Player:
                    Player player = creature as Player;
                    Players.Remove(player);
                    break;
                case EObjectType.Monster:
                    Monster monster = creature as Monster;
                    Monsters.Remove(monster);
                    break;
            }
        }
        else if (obj.ObjectType == EObjectType.Projectile)
        {
            Projectile projectile = obj as Projectile;
            Projectiles.Remove(projectile);
        }
        else if (obj.ObjectType == EObjectType.Env)
        {
            //TODO
        }
        else if (obj.ObjectType == EObjectType.ItemHolder)
        {
            ItemHolder itemHolder = obj as ItemHolder;
            ItemHolders.Remove(itemHolder);
        }

        Managers.Resource.Destroy(obj.gameObject);
    }
}
