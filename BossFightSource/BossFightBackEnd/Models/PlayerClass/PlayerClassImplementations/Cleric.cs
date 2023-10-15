using BossFight.BossFightEnums;

namespace BossFight.Models
{
    public class Cleric: PlayerClass
    {
        public Cleric()
        {
            PlayerClassId = PlayerClassEnum.CLERIC;
            Name = "Cleric";
            HpScale = 1.5;
            ManaScale = 2.5;
            PurchasePrice = 0;
            CritChance = 0;
            HpRegenRate = 1;
            ManaRegenRate = 1;
            AttackPowerBonus = 0;
            SpellPowerBonus = 0;
            BaseHealth = 10;
            BaseMana = 10;
            Description = "Uses their healing magic to aid their allies.";
        }
    }
}
