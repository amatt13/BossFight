using System.Collections.Generic;
using BossFight.BossFightEnums;

namespace BossFight.Models
{
    public class Mage: PlayerClass
    {
        public Mage()
        {
            PlayerClassId = PlayerClassEnum.MAGE;
            Name = "Mage";
            HpScale = 3;
            ManaScale = 4.5;
            PurchasePrice = 1_000;
            CritChance = 4;
            HpRegenRate = 1;
            ManaRegenRate = 2;
            AttackPowerBonus = 0;
            SpellPowerBonus = 0;
            BaseHealth = 11;
            BaseMana = 14;
            Description = "Utilises the elements to blast their foes!";
        }

        public Mage(int pPlayerLevel)
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
