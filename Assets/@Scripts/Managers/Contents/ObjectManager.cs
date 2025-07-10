using System.Collections.Generic;
using UnityEngine;
using static Define;

//오브젝트들의 스폰과 디스폰을 관리할 클래스
public class ObjectManager
{
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

    public T Spawn<T>(Vector3 position, int templateID) where T : BaseObject
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
            //Creature 객체를 생성후 Creature컴포넌트를 가져와서

            //Switch문을 통해 CreatureType을 다시 검사
            //Player, Monster타입인지에 따라 Root생성을 따로 함

            //Creature에 기본값을 세팅
        }

        //오브젝트를 스폰하고 그 T타입에 obj를 리턴
        return obj as T;
    }

    public void Despawn<T>(T obj) where T : BaseObject
    {
        EObjectType objectType = obj.ObjectType;

        if (obj.ObjectType == EObjectType.Creature)
        {
            //TODO
            //Spawn함수와 동일하게 타입 검사후 제거하는 작업
        }

        Managers.Resource.Destroy(obj.gameObject);
    }
}
