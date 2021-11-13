using System;
using System.Collections.Generic;
using System.Linq;
using BossFight.Models;

namespace BossFight.Controllers
{
    public static class DamageDealer
    {
        public static void MonsterAttackPlayer(MonsterInstance pMonster, Player pPlayerToAttack, PlayerAttackSummary pPlayerAttackSummary)
        {
            var damageTexts = new List<string>();
            var dealDamage = pMonster.IsAlive();
            
            if (pMonster.HasActiveBlindDebuff())
            {
                if (pMonster.IsBlinded())
                {
                    damageTexts.Add("Monster is currently blind and cannot hit you.");
                    dealDamage = false;    
                }                
                else
                    damageTexts.Add("Monster resisted blind!");
            }

            if (pMonster.HasActiveStunDebuff())
            {
                if (pMonster.IsStunned())
                {
                    dealDamage = false;
                    damageTexts.Add("Monster is currently stunned and cannot hit you.");
                }
            }

            if (dealDamage)
            {
                var monsterDamage = pMonster.CalculateMonsterDamage(out bool isCrit);
                pPlayerAttackSummary.MonsterCrit = isCrit;
                
                // subtract "lowered attack" debuff
                if (pMonster.HasLowerAttackDefuff())
                    monsterDamage = pMonster.LowerAttackBecauseOfLowerAttackDebuff(monsterDamage);
                    
                pPlayerToAttack.SubtractHealth((int)monsterDamage);
            }

            pPlayerAttackSummary.MonsterRetaliateMessage = String.Join("\n", damageTexts);
        }

        public static PlayerAttackSummary PlayerAttackMonster(Player pPlayer, MonsterInstance pTargetMonster, bool pRetaliate = true)
        {
            var playerAttackSummary = new PlayerAttackSummary(pPlayer, pTargetMonster);

            var playerAttack = pPlayer.CalckWeaponAttackDamage(pTargetMonster, playerAttackSummary);            
            MonsterReceiveDamge(pTargetMonster, playerAttack, pPlayer);
            MonsterReceiveDOTDamage(pTargetMonster);
            
            var xpEarned = GenralHelperFunctions.CalculateExperienceFromDamageDealtToMonster(playerAttack, pTargetMonster);
            playerAttackSummary.PlayerXpEarned = xpEarned;
            pPlayer.GainXp(xpEarned, pTargetMonster.Level);

            if (pTargetMonster.IsAlive() && pRetaliate)
                MonsterAttackPlayer(pTargetMonster, pPlayer, playerAttackSummary);

            pTargetMonster.Persist();
            pPlayer.Persist();
            pPlayer.PlayerPlayerClass.Persist();
            return playerAttackSummary;
        }

        public static void MonsterReceiveDamge(MonsterInstance pMonsterInstance, int pDamageToReceive, Player pAttackingPlayer)
        {
            if (pDamageToReceive <= 0.0)
                pDamageToReceive = 1;
                
            pMonsterInstance.SubtractHealth(pDamageToReceive, pAttackingPlayer);
        }

        public static Tuple<int, List<string>> MonsterReceiveDOTDamage(MonsterInstance pTargetMonster)
        {
            var extraDamage = 0;
            var playerNames = new List<string>();
            // // count up damage and record the players
            // foreach (var entry in pTargetMonster.DamageOverTimeTracker.Values)
            // {
            //     if (pTargetMonster.IsAlive())
            //     {
            //         MonsterReceiveDamge(pTargetMonster, entry.GetDamage(), entry.GetPlayerId());
            //         extraDamage += entry.GetDamage();
            //         playerNames.Add(entry.GetPlayerName());
            //         entry.SubtractTurn();
            //     }
            // }
            // // remove debuffs where the duration is 0
            // var keysToRemove = (from key in pTargetMonster.DamageOverTimeTracker.Keys
            //                     where pTargetMonster.DamageOverTimeTracker[key].GetDuration() <= 0
            //                     select key).ToList();
            // foreach (var key in keysToRemove)
            // {
            //     pTargetMonster.DamageOverTimeTracker.Remove(key);
            // }
            return Tuple.Create(extraDamage, playerNames);
        }
    }
}