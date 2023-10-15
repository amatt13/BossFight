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
    }
}
