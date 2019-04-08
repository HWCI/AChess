using UnityEngine;

[CreateAssetMenu(fileName = "Skill Config", menuName = "Skills", order = 1)]
public class Skill : ScriptableObject
{
    [SerializeField] public string skillName;
    [SerializeField] private int skillId;
    [SerializeField] public int Range;
    [SerializeField] public int Power;
    [SerializeField] public SkillType Type;
    [SerializeField] public int AoE;
    //[SerializeField] private SkillEffect Effect;
    [SerializeField] private GameObject emitter;

    //public void Cast(CharacterScript target)
   // {
   //     Effect.OnCast(Range,Power, Type,AoE, target, emitter);
  //  }

    public void Cast(CharacterScript target)
    {
        target.Damage(Power);
        GameObject s = Instantiate(emitter);
    }

    public void DealSingleDamage(CharacterScript target, int damage)
    {
        target.Damage(damage);
    }
    public void DealAoEDamage(CharacterScript[] target, int damage)
    {
        foreach (var piece in target)
        {
            piece.Damage(damage);    
        }

    }
    public void DealDirectionalDamage(CharacterScript[] target, int damage)
    {
        foreach (var piece in target)
        {
            piece.Damage(damage);    
        }

    }

}