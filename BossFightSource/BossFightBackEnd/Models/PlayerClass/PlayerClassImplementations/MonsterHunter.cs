using System.Collections.Generic;
using BossFight.BossFightEnums;

namespace BossFight.Models
{
    public class MonsterHunter: PlayerClass
    {
        public MonsterHunter()
        {
            PlayerClassId = PlayerClassEnum.MONSTER_HUNTER;
            Name = "Monster Hunter";
            HpScale = 5;
            ManaScale = 4;
            PurchasePrice = 2_500;
            CritChance = 7;
            HpRegenRate = 1;
            ManaRegenRate = 2;
            AttackPowerBonus = 2;
            SpellPowerBonus = 1;
            BaseHealth = 18;
            BaseMana = 16;
            Description = "No monster nor demon may escape them.";

            PlayerClassRequirementList = BuildPlayerClassRequirementList();
        }

        public MonsterHunter(int pPlayerLevel)
        :base()
        {
            RecalculateUnlockedAbilities(pPlayerLevel);
        }

        protected static new List<PlayerClassRequirement> BuildPlayerClassRequirementList()
        {
            return new List<PlayerClassRequirement>{new PlayerClassRequirement(new Ranger(), 10)};
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
