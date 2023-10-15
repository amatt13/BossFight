using System.Collections.Generic;
using System.Linq;
using BossFight.BossFightEnums;

namespace BossFight.Models
{
    public class Barbarian: PlayerClass
    {
        public Barbarian()
        {
            PlayerClassId = PlayerClassEnum.BARBARIAN;
            Name = "Barbarian";
            HpScale = 6;
            ManaScale = 3;
            PurchasePrice = 2_500;
            CritChance = 7;
            HpRegenRate = 2;
            ManaRegenRate = 1;
            AttackPowerBonus = 2;
            SpellPowerBonus = 1;
            BaseHealth = 20;
            BaseMana = 14;
            Description = @"REEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE.
            This bad ass barbarian will wreck your skull!";

            PlayerClassRequirementList = BuildPlayerClassRequirementList();
        }

        protected static new List<PlayerClassRequirement> BuildPlayerClassRequirementList()
        {
            return new List<PlayerClassRequirement>{new PlayerClassRequirement(new Highwayman(), 10)};
        }
    }
}
