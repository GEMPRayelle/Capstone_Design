using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;

public interface ILoader<Key, Value>
{
    //Dictionary를 만들어 리턴시키는 함수
    Dictionary<Key, Value> MakeDict();
}

// DataSheet를 관리할 클래스
public class DataManager
{
    #region 각 데이터들의 Dictionary

    #endregion

    public void Init()
    {

    }

    /// <summary>
    /// Addressable에 등록한 리소스가 다 올라가 있는 상태에서 데이터들을 변환시켜서 메모리에 올라감
    /// </summary>
    public Loader LoadJson<Loader, Key, Value>(string path) where Loader : ILoader<Key, Value>
    {
        //경로에 있는 TextAsset을 가져와서
        TextAsset textAsset = Managers.Resource.Load<TextAsset>(path);
        //그 textAsset을 Json으로 파싱
        return JsonConvert.DeserializeObject<Loader>(textAsset.text);
    }
}
