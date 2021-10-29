using System;
using System.Collections.Generic;
using BossFight.Models;
using BossFight.Models.PlayerClass;

namespace BossFight
{
    public static class GenralHelperFunctions
    {
        // return a negative value if above required amount
        public static int XpNeededToNextLevel(PlayerClass pPlayerClass)
        {
            // xpNeeded = (lvl*(1+lvl))^1.5
            var xpNeeded = (int)Math.Floor(Math.Pow(pPlayerClass.Level * (1 + pPlayerClass.Level), 1.5));
            xpNeeded -= pPlayerClass.XP;
            return xpNeeded;
        }

        public static int CalculateExperienceFromDamageDealtToMonster(int pDamageDealt, Monster pMonster)
        {
            var xp = 1;
            xp += (int)Math.Floor((double)pDamageDealt * 1.1);
            xp += (int)Math.Floor((double)pMonster.Level / 3);
            if (pMonster.BossMonster)
                xp += (int)Math.Ceiling((double)xp * 0.2);

            return xp;
        }

        // public static Weapon FindWeaponByWeaponId(int weaponId)
        // {
        //     var weapon = WeaponList.fists;
        //     try
        //     {
        //         if (weaponId is int)
        //         {
        //             weapon = next(from w in WeaponList.ALLWEAPONS
        //                           where w.lootId == weaponId
        //                           select w);
        //         }
        //     }
        //     catch (StopIteration)
        //     {
        //         Console.WriteLine("Could not find weapon with id: {weaponId}");
        //     }
        //     return weapon;
        // }

        // public static object findLootByName(object lootName = str)
        // {
        //     lootName = lootName.lower();
        //     var weapon = WeaponList.fists;
        //     try
        //     {
        //         if (lootName is str)
        //         {
        //             weapon = next(from w in WeaponList.ALLWEAPONS
        //                           where w.lootName.lower() == lootName
        //                           select w);
        //         }
        //     }
        //     catch (StopIteration)
        //     {
        //         Console.WriteLine("Could not find weapon with name: {lootName}");
        //     }
        //     return weapon;
        // }

        public static int CalcXpPenalty(int pXP, int pPlayerLevel, int? pMonsterLevel)
        {
            double result = pXP;
            if (pMonsterLevel == null)
            {
                pMonsterLevel = pPlayerLevel;
            }
            if (pMonsterLevel > pPlayerLevel + 2)
            {
                result = pXP * (pPlayerLevel / pMonsterLevel.Value);
            }
            else if (pMonsterLevel < pPlayerLevel)
            {
                var levelsBelow = pPlayerLevel - pMonsterLevel;
                if (levelsBelow == 6)
                {
                    result = result * 0.81;
                }
                else if (levelsBelow == 7)
                {
                    result = result * 0.62;
                }
                else if (levelsBelow == 8)
                {
                    result = result * 0.43;
                }
                else if (levelsBelow == 9)
                {
                    result = result * 0.24;
                }
                else if (levelsBelow >= 10)
                {
                    result = result * 0.05;
                }
            }
            return (int)Math.Ceiling(result);
        }

        public static string UpdatePlayersHealthAndManaHelper(List<Player> pPlayers, string pTimestamp, int pMinutesBetweenTicks, Action<Player, int> pRegenFunc)
        {
            var previousTimestamp = DateTime.Parse(pTimestamp);
            var now = DateTime.Now;
            var minutesDiff = Math.Floor((now - previousTimestamp).TotalSeconds / 60);
            var ticks = (int)Math.Floor(minutesDiff / pMinutesBetweenTicks);
            foreach (var p in pPlayers)
                pRegenFunc(p, ticks);

            var minutesToRemove = minutesDiff % pMinutesBetweenTicks;
            now = now.AddMinutes(minutesToRemove);
            return now.ToString("%Y-%m-%d:%H:%M:00");
        }

        public static void RegenPlayerHealth(Player p, int timesToRegen = 1)
        {
            p.RegenHealth(timesToRegen);
        }

        public static void RegenPlayerMana(Player p, int timesToRegen = 1)
        {
            p.RegenMana(timesToRegen);
        }
    }
}