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

    #region Roots
    public Transform PlayerRoot { get { return GetRootTransform("@Players"); } }
    public Transform MonsterRoot { get { return GetRootTransform("@Monsters"); } }

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
            //TODO
        }
        else if(obj.ObjectType == EObjectType.Env)
        {
            //TODO
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
            //TODO
        }
        else if (obj.ObjectType == EObjectType.Env)
        {
            //TODO
        }

        Managers.Resource.Destroy(obj.gameObject);
    }
}
