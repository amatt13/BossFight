using System.Collections.Generic;
using BossFight.BossFightEnums;

namespace BossFight.Models
{
    public class Hexer: PlayerClass
    {
        public Hexer()
        {
            PlayerClassId = PlayerClassEnum.HEXER;
            Name = "Hexer";
            HpScale = 3.5;
            ManaScale = 4;
            PurchasePrice = 1_000;
            CritChance = 3;
            HpRegenRate = 3;
            ManaRegenRate = 2;
            AttackPowerBonus = 0;
            SpellPowerBonus = 0;
            BaseHealth = 12;
            BaseMana = 13;
            Description = "Debuffs their foes witht their wicked magic.";
        }

        public Hexer(int pPlayerLevel)
        :base()
        {
            RecalculateUnlockedAbilities(pPlayerLevel);
        }

        public override List<Ability> RecalculateUnlockedAbilities(int pPlayerLevel)
        {
            var unlockedAbilities = new List<Ability>{ new Heal() };
            if (pPlayerLevel >= 3)
            {
                unlockedAbilities.Add(new Smite());
            }

            _unlockedAbilities = unlockedAbilities;
            return unlockedAbilities;
        }
    }
}
