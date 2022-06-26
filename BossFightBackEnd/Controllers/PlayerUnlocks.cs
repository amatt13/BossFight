using System;
using System.Collections.Generic;
using System.Linq;
using BossFight.Extentions;
using BossFight.Models;

namespace BossFight.Controllers
{
    public static class PlayerUnlocks
    {
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
                var unlockableClasses = GetPlayerClassesThatPlayerCanUnlock(pPlayer, aquiredClasses.Select(ac => ac.PlayerClassId.Value));
                foreach (var unlockedClass in unlockableClasses)
                {
                    unlockedClasses.Add(new PlayerClassUnlockStatus(unlockedClass, false));
                }
            }

            return unlockedClasses;
        }

        public static List<PlayerClass> GetPlayerClassesThatPlayerCanUnlock(Player pPlayer, IEnumerable<int> pPlayerClassIdsToIgnore)
        {
            using var connection = GlobalConnection.GetNewOpenConnection();
            using var cmd = connection.CreateCommand();
            var notInClause = pPlayerClassIdsToIgnore.Any() ? $"pc.PlayerClassId  NOT IN ({ String.Join(", ", pPlayerClassIdsToIgnore) }) AND" : String.Empty;
            cmd.CommandText = $@"SELECT pc.*
FROM PlayerClass pc
LEFT Join PlayerClassRequirement pcr 
	ON pcr.PlayerClassId = pc.PlayerClassId 
LEFT Join PlayerClass requiredClass
	ON requiredClass.PlayerClassId = pcr.RequiredPlayerClassId 
WHERE { notInClause } EXISTS (SELECT 1
	FROM PlayerPlayerClass unlockedRelation 
	WHERE (
		requiredClass.PlayerClassId  IS NULL 
		OR unlockedRelation.PlayerClassId = requiredClass.PlayerClassId 
	)
	AND unlockedRelation.PlayerId = @playerID
	AND (
		pcr.LevelRequirement is NULL 
		OR pcr.LevelRequirement <= unlockedRelation.Level
	)
)
";
            cmd.Parameters.AddParameter(pPlayer.PlayerId.Value, "@playerID");
            var reader = cmd.ExecuteReader();
            var playerClasses = new PlayerClass().BuildObjectFromReader(reader, connection).ToList();
            connection.Close();

            return playerClasses;
        }
    }
}