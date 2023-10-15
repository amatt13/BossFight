using System.Collections.Generic;
using BossFight.BossFightEnums;

namespace BossFight.Models
{
    public class Paladin: PlayerClass
    {
        public Paladin()
        {
            PlayerClassId = PlayerClassEnum.PALADIN;
            Name = "Paladin";
            HpScale = 5.5;
            ManaScale = 3.5;
            PurchasePrice = 2_500;
            CritChance = 6;
            HpRegenRate = 2;
            ManaRegenRate = 2;
            AttackPowerBonus = 1;
            SpellPowerBonus = 1;
            BaseHealth = 16;
            BaseMana = 16;
            Description = @"The feverous paladin will follow their sacred texts no matter the cost.
            A powerful ally to have by your side.";

            PlayerClassRequirementList = BuildPlayerClassRequirementList();
        }

        protected static new List<PlayerClassRequirement> BuildPlayerClassRequirementList()
        {
            return new List<PlayerClassRequirement>{new PlayerClassRequirement(new Cleric(), 10)};
        }
    }
}
