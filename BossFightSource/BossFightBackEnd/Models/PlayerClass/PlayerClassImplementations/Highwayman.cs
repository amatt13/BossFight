using System.Collections.Generic;
using BossFight.BossFightEnums;

namespace BossFight.Models
{
    public class Highwayman: PlayerClass
    {
        public Highwayman()
        {
            PlayerClassId = PlayerClassEnum.HIGHWAYMAN;
            Name = "Highwayman";
            HpScale = 2.5;
            ManaScale = 1.5;
            PurchasePrice = 0;
            CritChance = 2;
            HpRegenRate = 1;
            ManaRegenRate = 1;
            AttackPowerBonus = 0;
            SpellPowerBonus = 0;
            BaseHealth = 10;
            BaseMana = 10;
            Description = "A scoundrel and bruiser!";
        }

        public Highwayman(int pPlayerLevel)
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
