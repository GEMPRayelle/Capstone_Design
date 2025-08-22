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
    public Dictionary<int, Data.HeroData> HeroDic { get; private set; } = new Dictionary<int, Data.HeroData>();
    public Dictionary<int, Data.MonsterData> MonsterDic { get; private set; } = new Dictionary<int, Data.MonsterData>();
    public Dictionary<int, Data.SkillData> SkillDic { get; private set; } = new Dictionary<int, Data.SkillData>();
    public Dictionary<int, Data.ProjectileData> ProjectileDic { get; private set; } = new Dictionary<int, Data.ProjectileData>();
    public Dictionary<int, Data.DropTableData> DropTableDic { get; private set; } = new Dictionary<int, Data.DropTableData>();
    public Dictionary<int, Data.EffectData> EffectDic { get; private set; } = new Dictionary<int, Data.EffectData>();
    public Dictionary<int, Data.LevelData> LevelDic { get; private set; } = new Dictionary<int, Data.LevelData>();
    #endregion

    public void Init()
    {
        HeroDic = LoadJson<Data.HeroDataLoader, int, Data.HeroData>("HeroData").MakeDict();
        MonsterDic = LoadJson<Data.MonsterDataLoader, int, Data.MonsterData>("MonsterData").MakeDict();
        SkillDic = LoadJson<Data.SkillDataLoader, int, Data.SkillData>("SkillData").MakeDict();
        ProjectileDic = LoadJson<Data.ProjectileDataLoader, int, Data.ProjectileData>("ProjectileData").MakeDict();
        DropTableDic = LoadJson<Data.DropTableDataLoader, int, Data.DropTableData>("DropTableData").MakeDict();
        EffectDic = LoadJson<Data.EffectDataLoader, int, Data.EffectData>("EffectData").MakeDict();
        LevelDic = LoadJson<Data.LevelDataLoader, int, Data.LevelData>("LevelData").MakeDict();
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
