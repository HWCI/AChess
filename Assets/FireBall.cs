
    using UnityEngine;

    public class FireBall : SkillEffect
    {
        public GameObject emitter;
        public void OnCast(int range, int power, SkillType type, int AoE, CharacterScript target)
        {
            target.Damage(power*20);
            Instantiate(emitter);
        }
    }
