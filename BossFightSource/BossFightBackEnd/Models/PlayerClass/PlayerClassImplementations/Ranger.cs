using System.Collections.Generic;
using BossFight.BossFightEnums;

namespace BossFight.Models
{
    public class Ranger: PlayerClass
    {
        public Ranger()
        {
            PlayerClassId = PlayerClassEnum.RANGER;
            Name = "Ranger";
            HpScale = 2;
            ManaScale = 2;
            PurchasePrice = 0;
            CritChance = 4;
            HpRegenRate = 1;
            ManaRegenRate = 1;
            AttackPowerBonus = 0;
            SpellPowerBonus = 0;
            BaseHealth = 10;
            BaseMana = 10;
            Description = "Deals bonus damage to beasts.";
        }

        public Ranger(int pPlayerLevel)
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
