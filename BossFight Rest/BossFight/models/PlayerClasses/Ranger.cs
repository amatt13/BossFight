using System;
using System.Collections.Generic;
using BossFight.BossFightEnums;

namespace BossFight.Models
{
    public class Ranger : PlayerClass
    {
        public Ranger(Player pPlayer, int pXp = 0, int pLevel = 1)
            : base("Ranger", pXp, pLevel, pPlayer, new List<WeaponType> {
                    WeaponType.DAGGER,
                    WeaponType.THROWN,
                    WeaponType.BOW,
                    WeaponType.IMPROVISED
            })
        {
            PlayerClassCritChance = 4;
        }

        public override PlayerClass FromDB(object playerClassDict, Player player)
        {
            var xp = 1;//playerClassDict["xp"];
            var level = 1;//Convert.ToInt32(playerClassDict["level"]);
            Ranger playerClass = new Ranger(player, xp, level);
            playerClass.Recalculate();
            return playerClass;
        }

        public override Dictionary<String, Ability> PrepareAvailableAbilities()
        {
            var unlockedAbilities = new Dictionary<String, Ability>();
            var doubleStrike = Abilities.DoubleStrike();
            unlockedAbilities[doubleStrike.magicWord] = doubleStrike;
            if (Level >= 5)
            {
                var poisonedBait = Abilities.PoisonedBait();
                unlockedAbilities[poisonedBait.magicWord] = poisonedBait;
            }
            return unlockedAbilities;
        }
    }
}