using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Pool;

/// <summary>
/// 오브젝트를 활성화,비활성화 상태로 바꾸면서 객체를 사용 = Pooling (메모리에서 해제 x)
/// </summary>
internal class Pool
{
    private GameObject _prefab;
    private IObjectPool<GameObject> _pool;

    private Transform _root;

    public Transform Root //Pooling될 오브젝트들이 배치될 Root
    {
        get
        {
            if (_pool == null)
            {
                GameObject go = new GameObject { name = $@"{_prefab.name}" };
                _root = go.transform;
            }
            return _root;
        }
    }

    public Pool(GameObject prefab)
    {
        _prefab = prefab;
        //Pool을 생성
        _pool = new ObjectPool<GameObject>(OnCreate, OnGet, OnRelease, OnDestroy);
    }

    public void Push(GameObject go)
    {
        if (go.activeSelf)
            _pool.Release(go);
    }

    public GameObject Pop()
    {
        return _pool.Get();
    }

    #region Helper Func
    /// <summary>
    /// 오브젝트를 처음 만들때
    /// </summary>
    private GameObject OnCreate()
    {
        GameObject go = GameObject.Instantiate(_prefab);
        go.transform.SetParent(Root);
        go.name = _prefab.name;
        return go;
    }

    /// <summary>
    /// 오브젝트를 킬때
    /// </summary>
    private void OnGet(GameObject go)
    {
        go.SetActive(true);
    }

    /// <summary>
    /// 오브젝트를 끌때
    /// </summary>
    private void OnRelease(GameObject go)
    {
        go.SetActive(false);
    }

    /// <summary>
    /// 완벽하게 삭제해야 할때
    /// </summary>
    private void OnDestroy(GameObject go)
    {
        GameObject.Destroy(go);
    }
    #endregion
}

public class PoolManager
{
    private Dictionary<string, Pool> _pools = new Dictionary<string, Pool>();

    /// <summary>
    /// Pool 저장소에서 꺼내오기
    /// </summary>
    public GameObject Pop(GameObject prefab)
    {
        //프리팹의 이름을 그대로 키값으로 사용
        if (_pools.ContainsKey(prefab.name) == false)
            CreatePool(prefab);

        return _pools[prefab.name].Pop();
    }

    /// <summary>
    /// Pool 저장소에 반납 
    /// </summary>
    public bool Push(GameObject go)
    {
        if (_pools.ContainsKey(go.name) == false)
            return false;

        _pools[go.name].Push(go);
        return true;
    }

    /// <summary>
    /// Pool 저장소를 초기화
    /// </summary>
    public void Clear()
    {
        _pools.Clear();
    }

    private void CreatePool(GameObject original)
    {
        Pool pool = new Pool(original);
        _pools.Add(original.name, pool);
    }
}
