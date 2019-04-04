using UnityEngine;

[CreateAssetMenu(fileName = "Skill Config", menuName = "Skills", order = 1)]
public class Skill : ScriptableObject
{
    public string skillName { get; private set; }
    [SerializeField] private int skillId;
    [SerializeField] int Range;
    [SerializeField] int Power;
    public SkillType Type { get; private set; }
    [SerializeField] int AoE;
    //[SerializeField] private SkillEffect Effect;
    [SerializeField] private GameObject emitter;

    //public void Cast(CharacterScript target)
   // {
   //     Effect.OnCast(Range,Power, Type,AoE, target, emitter);
  //  }

    public void Cast(CharacterScript target)
    {
        target.Damage(Power*20);
        Instantiate(emitter);
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