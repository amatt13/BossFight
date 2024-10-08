using System;
using BossFight.Models;

namespace BossFight
{
    public static class ExperienceCalculator
    {
        // return a negative value if above required amount
        public static int XpNeededToNextLevel(PlayerPlayerClass pPlayerClass)
        {
            // xpNeeded = (lvl*(1+lvl))^1.5
            var xpNeeded = (int)Math.Floor(Math.Pow(pPlayerClass.Level * (1 + pPlayerClass.Level), 1.5));
            xpNeeded -= pPlayerClass.XP;
            return xpNeeded;
        }

        public static int CalculateExperienceFromDamageDealtToMonster(int pDamageDealt, MonsterInstance pMonster)
        {
            var xp = 1;
            xp += (int)Math.Floor((double)pDamageDealt * 1.1);
            xp += (int)Math.Floor((double)pMonster.Level / 3);
            if (pMonster.IsBossMonster)
                xp = (int)Math.Ceiling((double)xp * 1.2);

            return xp;
        }

        public static int CalcXpPenalty(int pXP, int pPlayerLevel, int? pMonsterLevel)
        {
            // XP penality system is copied straight from Diablo II (http://classic.battle.net/diablo2exp/basics/experience.shtml).
            // There is differences in the actual numbers, but the idea is the same.
            decimal result = pXP;
            decimal playerLevel_decimal = pPlayerLevel;
            decimal monsterLevel_decimal = pMonsterLevel.GetValueOrDefault(pPlayerLevel);
            
            if (monsterLevel_decimal > playerLevel_decimal + 2)
            {
                result = pXP * (playerLevel_decimal / monsterLevel_decimal);
            }
            else if (monsterLevel_decimal < playerLevel_decimal)
            {
                var levelsBelow = playerLevel_decimal - monsterLevel_decimal;
                if (levelsBelow == 6)
                {
                    result *= 0.81m;
                }
                else if (levelsBelow == 7)
                {
                    result *= 0.62m;
                }
                else if (levelsBelow == 8)
                {
                    result *= 0.43m;
                }
                else if (levelsBelow == 9)
                {
                    result *= 0.24m;
                }
                else if (levelsBelow >= 10)
                {
                    result *= 0.05m;
                }
            }
            return (int)Math.Ceiling(result);
        }
    }
}
