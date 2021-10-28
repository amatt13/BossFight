using System;
using System.Collections.Generic;
using BossFight.BossFightEnums;

namespace BossFight.Models
{
    public class Hexer : PlayerClass
    {
        public Hexer(Player pPlayer, int pXp = 0, int pLevel = 1)
            : base("Hexer", pXp, pLevel, pPlayer, new List<WeaponType> {
                    WeaponType.DAGGER,
                    WeaponType.STAFF
            }, pHpScale: 3.5, pManaScale: 4)
        {
            PlayerClassCritChance = 3;
            PurchasePrice = 1000;
            BaseHealth = 12;
            BaseMana = 13;
            ManaRegenRate = 2;
            AttackPowerBonus = 1;
        }

        public override PlayerClass FromDB(object playerClassDict, Player player)
        {
            var xp = 1;//playerClassDict["xp"];
            var level = 1;//Convert.ToInt32(playerClassDict["level"]);
            Hexer playerClass = new Hexer(player, xp, level);
            playerClass.Recalculate();
            return playerClass;
        }

        public override Dictionary<String, Ability> PrepareAvailableAbilities()
        {
            var unlockedAbilities = new Dictionary<String, Ability>();
            var hex = Abilities.Hex();
            unlockedAbilities[hex.magicWord] = hex;
            if (Level >= 5)
            {
                var fractureSkin = Abilities.FractureSkin();
                unlockedAbilities[fractureSkin.magicWord] = fractureSkin;
            }
            return unlockedAbilities;
        }
    }
}
