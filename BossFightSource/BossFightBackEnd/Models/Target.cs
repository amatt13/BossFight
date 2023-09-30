using System.Collections.Generic;

namespace BossFight.Models
{
    public interface ITarget
    {
        int Hp { get; set; }
        int Mana { get; set; }
        string Name { get; set; }
        int Level { get; }
        IEnumerable<Effect> ActiveEffects {get; set;}

        abstract int GetMaxHp();
        bool IsDead();
        bool IsAlive();
        bool IsAtFullHealth();
        string PossessiveName();
        /// <summary>
        /// Returns true if the effect was added to the ITarget
        /// </summary>
        bool AddEffect(Effect pEffect);
        void RemoveEffect(EffectType pEffectType);
    }
}
