using UnityEngine;
using System.Collections.Generic;
using Object = UnityEngine.Object;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.AddressableAssets;
using System;

/// <summary>
/// Addressable에 등록할 리소스들과 등록된 리소스들을 관리할 클래스
/// </summary>
public class ResourceManager
{
    //최종적으로 메모리에 들고있을 리소스와 리소스를 관리할 handle
    private Dictionary<string, UnityEngine.Object> _resources = new Dictionary<string, UnityEngine.Object>();
    private Dictionary<string, AsyncOperationHandle> _handles = new Dictionary<string, AsyncOperationHandle>();

    #region Load Reosurce -> Addressable에 등록된 리소스들을 하나씩 꺼내서 사용
    public T Load<T>(string key) where T : Object
    {
        if (_resources.TryGetValue(key, out Object resource))
            return resource as T;

        //Sprite 타입 리소스인데 그 이름 뒤에 .sprite가 없다면
        if (typeof(T) == typeof(Sprite) && key.Contains(".sprite") == false)
        {
            //이름뒤에 .sprite를 추가하여 리턴
            if (_resources.TryGetValue($"{key}.sprite", out resource))
                return resource as T;
        }

        return null;
    }

    //로드한 리소스가 프리팹이라면 Instantiate로 사용
    public GameObject Instantiate(string key, Transform parent = null, bool pooling = false)
    {
        GameObject prefab = Load<GameObject>(key);
        if (prefab == null)
        {
            //1. 로드 실패가 떴을시 Addressable Group에 추가가 되었는지 1차적으로 검사
            Debug.LogError($"Failed to load prefab : {key}");
            return null;
        }

        if (pooling)
            return Managers.Pool.Pop(prefab);

        if (prefab == null)
            Debug.Log($"{prefab}이 없음");

        GameObject go = Object.Instantiate(prefab, parent);
        go.name = prefab.name;

        return go;
    }

    public void Destroy(GameObject go)
    {
        if (go == null)
            return;

        if (Managers.Pool.Push(go))
            return;

        Object.Destroy(go);
    }
    #endregion

    #region Addressable
    //초반에 필요한 리소스들을 전부 메모리에 들고 시작

    private void LoadAsync<T>(string key, Action<T> callback = null) where T : UnityEngine.Object
    {
        // Cache
        if (_resources.TryGetValue(key, out Object resource))
        {
            callback?.Invoke(resource as T);
            return;
        }

        string loadKey = key;
        //스프라이트만 특이한 이름으로 바꿔주는 처리를 함
        if (key.Contains(".sprite"))
            loadKey = $"{key}[{key.Replace(".sprite", "")}]";

        //그게 아니라면 일반적으로 로드할때는 LoadAssetAsync함수 사용
        var asyncOperation = Addressables.LoadAssetAsync<T>(loadKey);
        //LoadAssetAsync가 handle을 반환하면 
        //Completed쪽에 로딩이 완료되면 실행되었으면 하는 함수를 넣어줌
        asyncOperation.Completed += (op) =>
        {
            //리소스랑 handle을 추가
            _resources.Add(key, op.Result);
            _handles.Add(key, asyncOperation);
            callback?.Invoke(op.Result);
        };
    }

    /// <summary>
    /// 처음 시작시 에셋들을 전부 로드하는 함수
    /// </summary>
    /// <typeparam name="T">로드할 리소스의 타입</typeparam>
    /// <param name="label">Addressable 그룹에서 로드 하고 싶은 label</param>
    /// <param name="callback">string,int,int 3가지 인자를 받아주는 콜백 함수만 기입</param>
    public void LoadAllAsync<T>(string label, Action<string, int, int> callback) where T : UnityEngine.Object
    {
        //label에 해당하는 리소스들의 목록을 전부 모아온다
        var opHandle = Addressables.LoadResourceLocationsAsync(label, typeof(T));
        //그 목록들을 하나씩 순회함
        opHandle.Completed += (op) =>
        {
            int loadCount = 0;//현재 로딩된 에셋 갯수
            int totalCount = op.Result.Count;//로딩해야할 에셋의 총 갯수

            foreach (var result in op.Result)
            {
                //스프라이트를 제대로 불러오지 못하는 불상사 및 버그때문에 따로 검사
                if (result.PrimaryKey.Contains(".sprite"))
                {
                    LoadAsync<Sprite>(result.PrimaryKey, (obj) =>
                    {
                        loadCount++;
                        //마지막에 어떤 에셋을 로드했는지를 Primarykey로 받아준다
                        callback?.Invoke(result.PrimaryKey, loadCount, totalCount);
                    });
                }
                else
                {
                    LoadAsync<T>(result.PrimaryKey, (obj) =>
                    {
                        loadCount++;
                        callback?.Invoke(result.PrimaryKey, loadCount, totalCount);
                    });
                }
            }
        };
    }

    public void Clear()
    {
        _resources.Clear();

        foreach (var handle in _handles)
            Addressables.Release(handle);

        _handles.Clear();
    }
    #endregion
}
