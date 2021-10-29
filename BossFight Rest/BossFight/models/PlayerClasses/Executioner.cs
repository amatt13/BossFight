using System;
using System.Collections.Generic;
using BossFight.BossFightEnums;

namespace BossFight.Models.PlayerClass
{
    public class Executioner : PlayerClass
    {
        public Executioner(Player pPlayer, int pXp = 0, int pLevel = 1)
            : base("Executioner", pXp, pLevel, pPlayer, new List<WeaponType> {
                    WeaponType.SWORD,
                    WeaponType.MACE,
                    WeaponType.DAGGER,
                    WeaponType.POLEARM
            }, pHpScale: 2.5, pManaScale: 1.5)
        {
            PlayerClassCritChance = 2;
        }

        public override PlayerClass FromDB(object playerClassDict, Player player)
        {
            var xp = 1;//playerClassDict["xp"];
            var level = 1;//Convert.ToInt32(playerClassDict["level"]);
            Executioner playerClass = new Executioner(player, xp, level);
            playerClass.Recalculate();
            return playerClass;
        }

        public override Dictionary<String, Ability.Ability> PrepareAvailableAbilities()
        {
            var unlockedAbilities = new Dictionary<String, Ability.Ability>();
            var sackOnHead = Abilities.SackOnHead();
            unlockedAbilities[sackOnHead.magicWord] = sackOnHead;
            if (Level >= 5)
            {
                var execute = Abilities.Execute();
                unlockedAbilities[execute.magicWord] = execute;
            }
            return unlockedAbilities;
        }
    }
}
