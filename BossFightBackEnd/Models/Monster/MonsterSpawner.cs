using System;
using BossFight.Models;

namespace BossFight
{
    public class MonsterSpawner
    {
        private static object _spawnNewMonsterLock = new object();

        public static MonsterInstance SpawnNewMonster()
        {
            MonsterInstance newMonster = null;
            lock(_spawnNewMonsterLock)
            {
                var currentMonster = new MonsterInstance{ Active = true }.FindOne(null);
                // make sure the monster is dead before we spawn a new one
                if (currentMonster != null && currentMonster.IsDead())
                {
                    var randomMonsterTemplate = new MonsterTemplate{ SearchRandomTopOne = true, Tier = 1, BossMonster = false }.FindOne(null);
                    if (randomMonsterTemplate != null)
                    {
                        newMonster = new MonsterInstance(randomMonsterTemplate);
                        newMonster.Level = randomMonsterTemplate.Tier.Value * new Random().Next(1, 6);
                        newMonster.Active = true;
                        newMonster.CalcHealth();
                        newMonster.Persist();

                        currentMonster.Active = false;
                        currentMonster.Persist();
                    }
                }
            }
            return newMonster;
        }
    }
}
