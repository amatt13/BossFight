using System;
using System.Collections.Generic;
using BossFight.BossFightEnums;

namespace BossFight.Models
{
    public class Barbarian : PlayerClass
    {
        public Barbarian(Player pPlayer, int pXp = 0, int pLevel = 1)
            : base("Barbarian", pXp, pLevel, pPlayer, new List<WeaponType> {
                WeaponType.SWORD,
                WeaponType.MACE,
                WeaponType.DAGGER,
                WeaponType.POLEARM,
                WeaponType.IMPROVISED,
                WeaponType.THROWN,
                WeaponType.AXE,
                WeaponType.BOW
            }, pHpScale: 6, pManaScale: 3)
        {
            PlayerClassCritChance = 7;
            PurchasePrice = 1500;
            BaseHealth = 20;
            BaseMana = 14;
            HpRegenRate = 2;
            AttackPowerBonus = 2;
            SpellPowerBonus = 1;
        }

        public override PlayerClass FromDB(object playerClassDict, Player player)
        {
            var xp = 1;//playerClassDict["xp"];
            var level = 1;//Convert.ToInt32(playerClassDict["level"]);
            Barbarian playerClass = new Barbarian(player, xp, level);
            playerClass.Recalculate();
            return playerClass;
        }

        public static object getClassUnlockRequirements()
        {
            return new List<PlayerClassRequirement> {
                    new PlayerClassRequirement(typeof(Executioner), 10, "Executioner")
                };
        }

        public override Dictionary<String, Ability> PrepareAvailableAbilities()
        {
            var unlockedAbilities = new Dictionary<String, Ability>();
            var shout = new Shout();
            unlockedAbilities[shout.MagicWord] = shout;
            if (Level >= 5)
            {
                var frenzy = new Frenzy();
                unlockedAbilities[frenzy.MagicWord] = frenzy;
            }
            return unlockedAbilities;
        }
    }
}
