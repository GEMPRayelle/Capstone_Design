using UnityEngine;

//ScriptableEffects can be attached to both tiles and abilities. 
[CreateAssetMenu(fileName = "ScriptableEffect", menuName = "ScriptableObjects/ScriptableEffect")]
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
