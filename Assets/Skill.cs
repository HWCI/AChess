using UnityEngine;

[System.Serializable]
public class Skill : MonoBehaviour
{
    [SerializeField] private string name;
    [SerializeField] private int skillId;
    [SerializeField] int Range;
    [SerializeField] int Power;
    [SerializeField] SkillType Type;
    [SerializeField] int AoE;
    [SerializeField] private SkillEffect Effect;

    public void Cast(CharacterScript target)
    {
        Effect.OnCast(Range,Power, Type,AoE, target);
    }

}