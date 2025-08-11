using System;
using System.Collections.Generic;
using UnityEngine;
using static Define;

//Creature 객체 하나의 Stat을 관리하는 클래스
[Serializable]
public class CreatureStat 
{
    public float BaseValue { get; private set; }//스탯 변화가 일어나기 전 초기값

    //Dirty Flag Pattern
    private bool _isDirty = true;

    [SerializeField]
    private float _value;
    public virtual float Value
    {
        get
        {
            //Dirty Flag Check
            if (_isDirty)
            {
                //최종 계산을 다시 돌림
                _value = CalculateFinalValue();
                _isDirty = false;
            }
            return _value;
        }
        private set { _value = value; }
    }

    //스탯마다 어떠한 변화를 받았는지 기록 (스탯 변화 history)
    public List<StatModifier> StatModifiers = new List<StatModifier>();

    public CreatureStat()
    {

    }

    public CreatureStat(float baseValue) : this()
    {
        BaseValue = baseValue;
    }

    public virtual void AddModifier(StatModifier modifier)
    {
        _isDirty = true;
        StatModifiers.Add(modifier);
    }

    public virtual bool RemoveModifier(StatModifier modifier)
    {
        if (StatModifiers.Remove(modifier))
        {
            _isDirty = true;
            return true;
        }
        return false;
    }

    //버프와 관련된건 전부 없애야할때 통으로 없애주는 함수
    public virtual bool ClearModifiersFromSource(object source)
    {
        int numRemovals = StatModifiers.RemoveAll(mod => mod.Source == source);

        if (numRemovals > 0)
        {
            _isDirty = true;
            return true;
        }
        return false;
    }

    private int CompareOrder(StatModifier a, StatModifier b)
    {
        if (a.Order == b.Order)
            return 0;

        return (a.Order < b.Order) ? -1 : 1;
    }

    private float CalculateFinalValue()
    {
        //초기값을 처음부터 다시 가져와서 연산 시작
        float finalValue = BaseValue;
        float sumPercentAdd = 0;

        //처리할 연산들 정렬
        StatModifiers.Sort(CompareOrder);

        for (int i = 0; i < StatModifiers.Count; i++)
        {
            StatModifier modifier = StatModifiers[i];

            //타입에 따라 수치 계산
            switch (modifier.Type)
            {
                case EStatModType.Add:
                    finalValue += modifier.Value; break;
                case EStatModType.PercentAdd:
                    sumPercentAdd += modifier.Value;
                    if (i == StatModifiers.Count - 1 || StatModifiers[i + 1].Type != EStatModType.PercentAdd)
                    {
                        finalValue *= 1 + sumPercentAdd;
                        sumPercentAdd = 0;
                    }
                    break;
                case EStatModType.PercentMult:
                    finalValue *= 1 + modifier.Value;
                    break;
            }
        }

        return (float)Math.Round(finalValue, 4);
    }
}
