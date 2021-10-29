using System;
using System.Collections.Generic;
using BossFight.BossFightEnums;

namespace BossFight.Models.PlayerClass
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

        public override Dictionary<String, Ability.Ability> PrepareAvailableAbilities()
        {
            var unlockedAbilities = new Dictionary<String, Ability.Ability>();
            var overSizedBearTrap = Abilities.OverSizedBearTrap();
            unlockedAbilities[overSizedBearTrap.magicWord] = overSizedBearTrap;
            var turnWeaponToSilver = Abilities.TurnWeaponToSilver();
            unlockedAbilities[turnWeaponToSilver.magicWord] = turnWeaponToSilver;
            if (Level >= 5)
            {
                var bigGameTrophy = Abilities.BigGameTrophy();
                unlockedAbilities[bigGameTrophy.magicWord] = bigGameTrophy;
            }
            return unlockedAbilities;
        }
    }
}
