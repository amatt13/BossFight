using System;
using BossFight.Models;

namespace BossFight
{
    public class MonsterSpawner
    {
        private static readonly object _spawnNewMonsterLock = new();
        public static readonly int MAX_MONSTER_TIER = 5;

        public static MonsterInstance SpawnNewMonster()
        {
            MonsterInstance newMonster = null;
            lock(_spawnNewMonsterLock)
            {
                var currentMonster = new MonsterInstance{ Active = true }.FindOne(null);
                // make sure the monster is dead before we spawn a new one
                if (currentMonster != null && currentMonster.IsDead())
                {
                    var nextMonsterTier = NextMonsterTier(currentMonster.MonsterTemplate.Tier.GetValueOrDefault(1));
                    var randomMonsterTemplate = new MonsterTemplate{ SearchRandomTopOne = true, Tier = nextMonsterTier, BossMonster = false }.FindOne(null);
                    if (randomMonsterTemplate != null)
                    {
                        newMonster = new MonsterInstance(randomMonsterTemplate)
                        {
                            Level = randomMonsterTemplate.Tier.Value * 5 + new Random().Next(1, 6),
                            Active = true
                        };
                        newMonster.CalcHealth();
                        newMonster.Persist();

                        currentMonster.Active = false;
                        currentMonster.Persist();

                        newMonster = newMonster.FindOne(null);
                    }
                }
            }
            return newMonster;
        }

        ///<summary>
        /// Finds the next monster tier, based on the players's votes
        ///</summary>
        private static int NextMonsterTier(int pCurrentMonsterTier)
        {
            var nextTier = pCurrentMonsterTier;

            var sql = $@"SELECT SUM(mtv.Vote)
FROM MonsterInstance mi
JOIN MonsterTierVote mtv
	ON mtv.MonsterInstanceId = mi.MonsterInstanceId
WHERE mi.Active  = 1";

            var votes = GlobalConnection.SingleValue<decimal?>(sql).GetValueOrDefault(0);

            if (votes > 0 )
            {
                nextTier += 1;
            }
            else if (votes < 0)
                nextTier -= 1;

            if (nextTier < 0)
            {
                nextTier = 0;
            }
            else if (nextTier > MAX_MONSTER_TIER)
                nextTier = MAX_MONSTER_TIER;

            return nextTier;
        }
    }
}
