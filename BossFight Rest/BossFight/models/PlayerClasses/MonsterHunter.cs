using System;
using System.Collections.Generic;
using BossFight.BossFightEnums;

namespace BossFight.Models
{
    public class MonsterHunter : PlayerClass
    {
        public MonsterHunter(Player pPlayer, int pXp = 0, int pLevel = 1)
            : base("MonsterHunter", pXp, pLevel, pPlayer, new List<WeaponType> {
                WeaponType.SWORD,
                WeaponType.MACE,
                WeaponType.DAGGER,
                WeaponType.POLEARM,
                WeaponType.IMPROVISED,
                WeaponType.THROWN,
                WeaponType.AXE,
                WeaponType.BOW
            }, pHpScale: 5, pManaScale: 4)
        {
            PlayerClassCritChance = 7;
            PurchasePrice = 1500;
            BaseHealth = 18;
            BaseMana = 16;
            ManaRegenRate = 2;
            AttackPowerBonus = 2;
            SpellPowerBonus = 1;
        }

        public override PlayerClass FromDB(object playerClassDict, Player player)
        {
            var xp = 1;//playerClassDict["xp"];
            var level = 1;//Convert.ToInt32(playerClassDict["level"]);
            MonsterHunter playerClass = new MonsterHunter(player, xp, level);
            playerClass.Recalculate();
            return playerClass;
        }

        public static object getClassUnlockRequirements()
        {
            return new List<object> {
                    new PlayerClassRequirement(typeof(Ranger), 10, "Ranger")
                };
        }

        public override Dictionary<String, Ability> PrepareAvailableAbilities()
        {
            var unlockedAbilities = new Dictionary<String, Ability>();
            var overSizedBearTrap = new OverSizedBearTrap();
            unlockedAbilities[overSizedBearTrap.MagicWord] = overSizedBearTrap;
            var turnWeaponToSilver = new TurnWeaponToSilver();
            unlockedAbilities[turnWeaponToSilver.MagicWord] = turnWeaponToSilver;
            if (Level >= 5)
            {
                var bigGameTrophy = new BigGameTrophy();
                unlockedAbilities[bigGameTrophy.MagicWord] = bigGameTrophy;
            }
            return unlockedAbilities;
        }
    }
}
