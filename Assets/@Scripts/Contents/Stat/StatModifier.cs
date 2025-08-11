using UnityEngine;
using static Define;

//스탯의 영향을 주는 동작을 정의하는 클래스
public class StatModifier 
{
    public readonly float Value;
    public readonly EStatModType Type;
    public readonly int Order;
    public readonly object Source;

    /// <summary>
    /// 생성자
    /// </summary>
    /// <param name="value">변화시킬 양</param>
    /// <param name="type">무슨 연산인지</param>
    /// <param name="order">연산의 가중치</param>
    /// <param name="source"></param>
    public StatModifier(float value, EStatModType type, int order, object source)
    {
        Value = value;
        Type = type;
        Order = order; //중요도 (덧셈을 먼저하고 곱셈을 할지에 대한 가중치 정보)
        Source = source;
    }

    public StatModifier(float value, EStatModType type) : this(value, type, (int)type, null) { }
    public StatModifier(float value, EStatModType type, int order) : this(value, type, order, null) { }
    public StatModifier(float value, EStatModType type, object source) : this(value, type, (int)type, source) { }
}
