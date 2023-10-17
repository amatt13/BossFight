using System.Collections.Generic;
using BossFight.BossFightEnums;

namespace BossFight.Models
{
    public interface ITarget: IEffectHolder
    {
        int Hp { get; set; }
        int Mana { get; set; }
        string Name { get; set; }
        int Level { get; }
        IEnumerable<Effect> ActiveEffects {get; set;}
        List<MonsterType> MonsterTypeList { get; }

        abstract int GetMaxHp();
        bool IsDead();
        bool IsAlive();
        bool IsAtFullHealth();
        string PossessiveName();
    }
}
