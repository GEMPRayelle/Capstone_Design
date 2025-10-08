using UnityEngine;

[CreateAssetMenu(fileName = "ScriptableEffect", menuName = "Scriptable Objects/ScriptableEffect")]
public class ScriptableEffect : ScriptableObject
{
    public CreatureStat statKey;
    public StatModifier Operator;
    public float Duration;
    public int Value;

    public CreatureStat GetStatKey()
    {
        return statKey;
    }
}
