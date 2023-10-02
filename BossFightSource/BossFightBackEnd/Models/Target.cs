using System.Collections.Generic;

namespace BossFight.Models
{
    public interface ITarget: IEffectHolder
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
    }
}
