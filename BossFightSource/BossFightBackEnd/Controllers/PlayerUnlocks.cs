using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using BossFight.BossFightEnums;
using BossFight.Extentions;
using BossFight.Models;
using Microsoft.VisualBasic;

namespace BossFight.Controllers
{
    public static class PlayerUnlocks
    {
        private static List<PlayerClass> _allPlayerClasses => _buildAllPlayerClassesList();
        private static List<PlayerClass> _buildAllPlayerClassesList()
        {
            var allClasses = new List<PlayerClass>();
            Array.ForEach((PlayerClassEnum[])Enum.GetValues(typeof(PlayerClassEnum)), playerClassEnumValue => allClasses.Add(PlayerClassFactory.CreatePlayerClass(playerClassEnumValue)));
            return allClasses;
        }

        // Relation that details if the PlayerClass is owned/Aquired (not to be confused with its "locked" status)
        public record PlayerClassUnlockStatus(PlayerClass PlayerClass, bool Aquired);

        public static List<PlayerClassUnlockStatus> UnlockedClasses(Player pPlayer, bool pOnlyClassesThatAreOwnedBy)
        {
            var unlockedClasses = new List<PlayerClassUnlockStatus>();

            // Find list of classes that are already aquired
            var aquiredClasses = new PlayerPlayerClass{ PlayerId = pPlayer.PlayerId }.FindAll().Select(pc => pc.PlayerClass);
            foreach (var aquiredClass in aquiredClasses)
            {
                unlockedClasses.Add(new PlayerClassUnlockStatus(aquiredClass, true));
            }

            if (!pOnlyClassesThatAreOwnedBy)
            {
                var unlockableClasses = GetPlayerClassesThatPlayerCanUnlock(pPlayer, aquiredClasses.Select(ac => ac.PlayerClassId));
                foreach (var unlockedClass in unlockableClasses)
                {
                    unlockedClasses.Add(new PlayerClassUnlockStatus(unlockedClass, false));
                }
            }

            return unlockedClasses;
        }

        public static List<PlayerClass> GetPlayerClassesThatPlayerCanUnlock(Player pPlayer, IEnumerable<PlayerClassEnum> pPlayerClassIdsToIgnore)
        {
            var playerClassesThatCanBeUnlockedByPlayer = new List<PlayerClass>();

            foreach (PlayerClass playerClass in _allPlayerClasses)
            {
                // Is it true that playerClass is not already unlocked?
                if (pPlayer.UnlockedPlayerPlayerClassList.All(unlocked => unlocked.PlayerClass.PlayerClassId != playerClass.PlayerClassId))
                {
                    var allRequirementsForClassIsMet = true;

                    // Do the player have the required class, and the required level?
                    foreach(var requirement in playerClass.PlayerClassRequirementList)
                    {
                        var prerequisitePlayerClass = pPlayer.UnlockedPlayerPlayerClassList.FirstOrDefault(unlocked => unlocked.PlayerClass.PlayerClassId == requirement.RequiredPlayerClass.PlayerClassId);
                        if (prerequisitePlayerClass == null || prerequisitePlayerClass.Level < requirement.LevelRequirement)
                        {
                            allRequirementsForClassIsMet = false;
                        }
                    }

                    if (allRequirementsForClassIsMet)
                    {
                        playerClassesThatCanBeUnlockedByPlayer.Add(playerClass);
                    }
                }


            }

            return playerClassesThatCanBeUnlockedByPlayer;
        }
    }
}
