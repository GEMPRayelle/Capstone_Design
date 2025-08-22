using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

namespace Data
{
    #region CreatureData    
    [Serializable]
    public class CreatureData
    {
        public int DataId;
        public string DescriptionTextID;
        public string PrefabLabel;
        public float ColliderOffsetX;
        public float ColliderOffsetY;
        public float ColliderRadius;
        public float MaxHp;
        public float UpMaxHpBonus;
        public float Atk;
        public float AtkRange;
        public float AtkBonus;
        public float MoveSpeed;
        public float CriRate;
        public float CriDamage;
        public string IconImage;
        public string SkeletonDataID;
        public int DefaultSkillId;
        public int EnvSkillId;
        public int SkillAId;
        public int SkillBId;
    }
    #endregion

    #region MonsterData
    public class MonsterData : CreatureData
    {
        public int DropItemId;
    }

    [Serializable]
    public class MonsterDataLoader : ILoader<int, MonsterData>
    {
        public List<MonsterData> monsters = new List<MonsterData>();
        public Dictionary<int, MonsterData> MakeDict()
        {
            Dictionary<int, MonsterData> dict = new Dictionary<int, MonsterData>();
            foreach (MonsterData monster in monsters)
                dict.Add(monster.DataId, monster);
            return dict;
        }
    }
    #endregion

    #region HeroData
    [Serializable]
    public class HeroData : CreatureData
    {

    }

    [Serializable]
    public class HeroDataLoader : ILoader<int, HeroData>
    {
        public List<HeroData> heroes = new List<HeroData>();
        public Dictionary<int, HeroData> MakeDict()
        {
            Dictionary<int, HeroData> dict = new Dictionary<int, HeroData>();
            foreach (HeroData hero in heroes)
                dict.Add(hero.DataId, hero);
            return dict;
        }
    }
    #endregion

    #region SkillData
    [Serializable]
    public class SkillData
    {
        public int DataId;
        public string Name;
        public string ClassName;
        public string Description;
        public int ProjectileId;
        public string PrefabLabel;
        public string IconLabel;
        public string AnimName;
        public float CoolTime;
        public float DamageMultiplier;
        public float Duration;
        public float AnimImpactDuration;
        public string CastingSound;
        public float SkillRange;
        public float ScaleMultiplier;
        public int TargetCount;
        public List<int> EffectIds = new List<int>();
        public int NextLevelId;
        public int AoEId;
        public EEffectSize EffectSize;
    }

    [Serializable]
    public class SkillDataLoader : ILoader<int, SkillData>
    {
        public List<SkillData> skills = new List<SkillData>();

        public Dictionary<int, SkillData> MakeDict()
        {
            Dictionary<int, SkillData> dict = new Dictionary<int, SkillData>();
            foreach (SkillData skill in skills)
                dict.Add(skill.DataId, skill);
            return dict;
        }
    }
    #endregion

    #region ProjectileData
    [Serializable]
    public class ProjectileData
    {
        public int DataId;
        public string Name;
        public string ClassName;
        public string ComponentName;
        public string ProjectileSpriteName;
        public string PrefabLabel;
        public float Duration;
        public float HitSound;
        public float ProjRange;
        public float ProjSpeed;
    }

    [Serializable]
    public class ProjectileDataLoader : ILoader<int, ProjectileData>
    {
        public List<ProjectileData> projectiles = new List<ProjectileData>();

        public Dictionary<int, ProjectileData> MakeDict()
        {
            Dictionary<int, ProjectileData> dict = new Dictionary<int, ProjectileData>();
            foreach (ProjectileData projectile in projectiles)
                dict.Add(projectile.DataId, projectile);
            return dict;
        }
    }
    #endregion

    #region DropTable
    public class RewardData
    {
        public int Probability;
        public int ItemTemplateId;
    }

    //임시적으로 파싱할 데이터들
    [Serializable]
    public class DropTableData_Internal
    {
        public int DataId;
        public int RewardExp;
        public int Prob1;
        public int Item1;
        //public int Prob2;
        //public int Item2;
        //public int Prob3;
        //public int Item3;
        //public int Prob4;
        //public int Item4;
        //public int Prob5;
        //public int Item5;
    }

    [Serializable]
    public class DropTableData
    {
        public int DataId;
        public int RewardExp;
        //파싱한 데이터들을 리스트로 전달
        public List<RewardData> Rewards = new List<RewardData>();
    }

    [Serializable]
    public class DropTableDataLoader : ILoader<int, DropTableData>
    {
        public List<DropTableData_Internal> dropTables = new List<DropTableData_Internal>();
        public Dictionary<int,DropTableData> MakeDict()
        {
            Dictionary<int, DropTableData> dict = new Dictionary<int, DropTableData>();

            foreach(DropTableData_Internal tempData in dropTables)
            {
                DropTableData data = new DropTableData()
                {
                    DataId = tempData.DataId,
                    RewardExp = tempData.RewardExp,
                };
                if (tempData.Item1 > 0)
                {
                    data.Rewards.Add(new RewardData()
                    {
                        Probability = tempData.Prob1,
                        ItemTemplateId = tempData.Item1
                    });
                }
                //if (tempData.Item2 > 0)
                //{
                //    data.Rewards.Add(new RewardData()
                //    {
                //        Probability = tempData.Prob2,
                //        ItemTemplateId = tempData.Item2
                //    });
                //}
                //if (tempData.Item3 > 0)
                //{
                //    data.Rewards.Add(new RewardData()
                //    {
                //        Probability = tempData.Prob3,
                //        ItemTemplateId = tempData.Item3
                //    });
                //}
                //if (tempData.Item4 > 0)
                //{
                //    data.Rewards.Add(new RewardData()
                //    {
                //        Probability = tempData.Prob4,
                //        ItemTemplateId = tempData.Item4
                //    });
                //}
                //if (tempData.Item5 > 0)
                //{
                //    data.Rewards.Add(new RewardData()
                //    {
                //        Probability = tempData.Prob5,
                //        ItemTemplateId = tempData.Item5
                //    });
                //}

                dict.Add(tempData.DataId, data);
            }
            return dict;
        }
    }
    #endregion

    #region Effect Data
    [Serializable]
    public class EffectData
    {
        public int DataId;
        public string Name;
        public string ClassName;
        public string DescriptionTextID;
        public string SkeletonDataID;
        public string IconLabel;
        public string SoundLabel;
        public float Amount; //StatModifier
        public float PercentAdd;
        public float PercentMult;
        public float TickTime; //duration
        public float TickCount; //period
        public EEffectType EffectType; //effect 효과 (buff, debuff, cc)
    }

    [Serializable]
    public class EffectDataLoader : ILoader<int, EffectData>
    {
        public List<EffectData> effects = new List<EffectData>();
        public Dictionary<int, EffectData> MakeDict()
        {
            Dictionary<int, EffectData> dict = new Dictionary<int, EffectData>();
            foreach (EffectData effect in effects)
                dict.Add(effect.DataId, effect);
            return dict;
        }
    }
    #endregion

    #region Level Data
    [Serializable]
    public class LevelData
    {
        public int DataId;
        public int Level;
        public int MaxGauge;
        public int MaxMonsterNum;
        public int MonsterId; // TODO 스폰될 몬스터들 List로 작성?
    }

    [Serializable]
    public class LevelDataLoader : ILoader<int, LevelData>
    {
        public List<LevelData> levels = new List<LevelData>();
        public Dictionary<int, LevelData> MakeDict()
        {
            Dictionary<int, LevelData> dict = new Dictionary<int, LevelData>();
            foreach (LevelData level in levels)
                dict.Add(level.DataId, level);
            return dict;
        }
    }
    #endregion

}