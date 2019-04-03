using UnityEngine;
    public abstract class SkillEffect : MonoBehaviour
    {
        public abstract void OnCast(int range, int power, SkillType type, int AoE, CharacterScript target);

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
