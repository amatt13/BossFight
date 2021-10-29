using System;
using System.Collections.Generic;
using BossFight.BossFightEnums;
using BossFight.Models.Ability;

namespace BossFight.Models.PlayerClass
{
    public class Cleric : PlayerClass
    {
        public Cleric(Player player, int xp = 0, int level = 1)
            : base("Cleric", xp, level, player, new List<WeaponType> {
                WeaponType.SWORD,
                WeaponType.MACE,
                WeaponType.DAGGER,
                WeaponType.STAFF
            }, pHpScale: 1.5, pManaScale: 2.5)
        {
        }

        public override PlayerClass FromDB(object playerClassDict, Player player)
        {
            var xp = 1;//playerClassDict["xp"];
            var level = 1;//Convert.ToInt32(playerClassDict["level"]);
            Cleric playerClass = new Cleric(player, xp, level);
            playerClass.Recalculate();
            return playerClass;
        }

        public override Dictionary<String, Ability.Ability> PrepareAvailableAbilities()
        {
            var unlockedAbilities = new Dictionary<String, Ability.Ability>();
            var heal = Abilities.Heal();
            unlockedAbilities[heal.magicWord] = heal;
            if (Level >= 5)
            {
                var smite = Abilities.Smite();
                unlockedAbilities[smite.magicWord] = smite;
            }
            return unlockedAbilities;
        }
    }
}
