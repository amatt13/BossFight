using System;
using System.Collections.Generic;
using BossFight.BossFightEnums;

namespace BossFight.Models.PlayerClass
{
    public class Mage : PlayerClass
    {
        public Mage(Player pPlayer, int pXp = 0, int pLevel = 1)
            : base("Mage", pXp, pLevel, pPlayer, new List<WeaponType> {
                WeaponType.DAGGER,
                WeaponType.STAFF
            }, pHpScale: 3, pManaScale: 4.5)
        {
            PlayerClassCritChance = 4;
            PurchasePrice = 1000;
            BaseHealth = 11;
            BaseMana = 14;
            ManaRegenRate = 2;
            SpellPowerBonus = 1;
        }

        public override PlayerClass FromDB(object playerClassDict, Player player)
        {
            var xp = 1;//playerClassDict["xp"];
            var level = 1;//Convert.ToInt32(playerClassDict["level"]);
            Mage playerClass = new Mage(player, xp, level);
            playerClass.Recalculate();
            return playerClass;
        }

        public override Dictionary<String, Ability> PrepareAvailableAbilities()
        {
            var unlockedAbilities = new Dictionary<String, Ability>();
            var ignite = Abilities.Ignite();
            unlockedAbilities[ignite.magicWord] = ignite;
            if (Level >= 5)
            {
                var enchantWeapon = Abilities.EnchantWeapon();
                unlockedAbilities[enchantWeapon.magicWord] = enchantWeapon;
            }
            return unlockedAbilities;
        }
    }
}
