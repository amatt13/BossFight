using System;
using System.Collections.Generic;
using BossFight.BossFightEnums;

namespace BossFight.Models.PlayerClass
{
    public class Paladin : PlayerClass
    {
        public Paladin(Player pPlayer, int pXp = 0, int pLevel = 1)
            : base("Paladin", pXp, pLevel, pPlayer, new List<WeaponType> {
                WeaponType.SWORD,
                WeaponType.MACE,
                WeaponType.DAGGER,
                WeaponType.POLEARM,
                WeaponType.AXE
            }, pHpScale: 5.5, pManaScale: 3.5)
        {
            PlayerClassCritChance = 6;
            PurchasePrice = 1500;
            BaseHealth = 16;
            BaseMana = 16;
            HpRegenRate = 2;
            AttackPowerBonus = 1;
            SpellPowerBonus = 1;
        }

        public override PlayerClass FromDB(object playerClassDict, Player player)
        {
            var xp = 1;//playerClassDict["xp"];
            var level = 1;//Convert.ToInt32(playerClassDict["level"]);
            Paladin playerClass = new Paladin(player, xp, level);
            playerClass.Recalculate();
            return playerClass;
        }

        public static object getClassUnlockRequirements()
        {
            return new List<PlayerClassRequirement> {
                    new PlayerClassRequirement(typeof(Cleric), 10, "Cleric")
                };
        }

        public override Dictionary<String, Ability.Ability> PrepareAvailableAbilities()
        {
            var unlockedAbilities = new Dictionary<String, Ability.Ability>();
            var sacrifice = Abilities.Sacrifice();
            unlockedAbilities[sacrifice.magicWord] = sacrifice;
            var heal = Abilities.Heal();
            unlockedAbilities[heal.magicWord] = heal;
            if (Level >= 5)
            {
                var fullRestore = Abilities.GreaterHeal();
                unlockedAbilities[fullRestore.magicWord] = fullRestore;
            }
            return unlockedAbilities;
        }
    }
}
