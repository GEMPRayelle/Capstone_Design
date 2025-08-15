using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static Define;

//오브젝트들의 스폰과 디스폰을 관리할 클래스
public class ObjectManager
{
    public Player Players { get; private set; }

    public HashSet<Player> players { get; } = new HashSet<Player>();
    public HashSet<Monster> monsters { get; } = new HashSet<Monster>();
    public HashSet<Projectile> Projectiles { get; } = new HashSet<Projectile>();
    public HashSet<AttackEffect> attackEffects { get; } = new HashSet<AttackEffect>();
    public HashSet<ItemHolder> ItemHolders { get; } = new HashSet<ItemHolder>();

    #region Roots
    public Transform PlayerRoot { get { return GetRootTransform("@Players"); } }
    public Transform MonsterRoot { get { return GetRootTransform("@Monsters"); } }
    public Transform ProjectileRoot { get { return GetRootTransform("@Projectiles"); } }
    public Transform EffectRoot { get { return GetRootTransform("@Effects"); } }
    public Transform ItemHolderRoot { get { return GetRootTransform("@ItemHolders"); } }

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

    public T Spawn<T>(Vector3 position, int templateId) where T : BaseObject
    {
        string prefabName = typeof(T).Name;//컴포넌트의 오브젝트 이름을 가져옴

        //오브젝트가 Instantiate될때 Awake에서 Init으로 초기화가 진행
        GameObject go = Managers.Resource.Instantiate(prefabName);
        go.name = prefabName;
        go.transform.position = position;

        //오브젝트를 아래에서 타입에 따라 구분
        BaseObject obj = go.GetComponent<BaseObject>();

        //Creature 타입의 오브젝트라면
        if (obj.ObjectType == EObjectType.Creature)
        {
            //TODO
            Creature creature = go.GetComponent<Creature>();
            switch (creature.CreatureType) 
            {
                case ECreatureType.Player:
                    obj.transform.parent = PlayerRoot;
                    Player player = creature as Player;
                    players.Add(player);
                    player.SetInfo(templateId);
                    break;
                case ECreatureType.Monster:
                    obj.transform.parent = MonsterRoot;
                    Monster monster = creature as Monster;
                    monsters.Add(monster);
                    monster.SetInfo(templateId);
                    break;
            }

            //Creature에 기본값을 세팅
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
        }
        else if (obj.ObjectType == EObjectType.ItemHolder)
        {
            obj.transform.parent = ItemHolderRoot;

            ItemHolder itemHolder = go.GetOrAddComponent<ItemHolder>();
            ItemHolders.Add(itemHolder);
            // 아이템의 setinfo는 예외적으로 소환하는 몬스터의 OnDead에서 구현
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
            switch (creature.CreatureType)
            {
                case ECreatureType.Player:
                    Player player = creature as Player;
                    players.Remove(player);
                    break;
                case ECreatureType.Monster:
                    Monster monster = creature as Monster;
                    monsters.Remove(monster);
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
