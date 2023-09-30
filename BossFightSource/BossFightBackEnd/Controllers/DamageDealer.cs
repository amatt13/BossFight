using System;
using System.Collections.Generic;
using System.Linq;
using BossFight.Extentions;
using BossFight.Models;

namespace BossFight.Controllers
{
    public static class DamageDealer
    {
        public static void MonsterAttackPlayer(MonsterInstance pMonster, Player pPlayerToAttack, PlayerAttackSummary pPlayerAttackSummary)
        {
            var damageTexts = new List<string>();
            var dealDamage = pMonster.IsAlive();

            if (dealDamage)
            {
                var monsterDamage = pMonster.CalculateMonsterDamage(out bool isCrit);
                pPlayerAttackSummary.MonsterCrit = isCrit;

                var onDamageReceivedEffects = pPlayerToAttack.ActiveEffects.Where(effect => effect is IProcsOnDamageTaken);
                foreach (var effect in onDamageReceivedEffects)
                {
                    effect.OnDamageReceived(pPlayerToAttack, pMonster, ref monsterDamage);
                }

                pPlayerToAttack.RemoveExpiredEffects();
                pMonster.RemoveExpiredEffects();

                pPlayerAttackSummary.MonsterTotalDamage = monsterDamage;
                pPlayerToAttack.SubtractHealth(monsterDamage);
                damageTexts.Add($"{ pMonster.Name } hits you for { monsterDamage } damage");
            }

            pPlayerAttackSummary.MonsterRetaliateMessage = String.Join("\n", damageTexts);
        }

        public static PlayerAttackSummary PlayerAttackMonster(Player pPlayer, MonsterInstance pTargetMonster, bool pRetaliate = true)
        {
            var playerAttackSummary = new PlayerAttackSummary(pPlayer, pTargetMonster);

            var playerAttack = pPlayer.CalckWeaponAttackDamage(pTargetMonster, playerAttackSummary);
            MonsterReceiveDamge(pTargetMonster, playerAttack, pPlayer);

            var xpEarned = ExperienceCalculator.CalculateExperienceFromDamageDealtToMonster(playerAttack, pTargetMonster);
            playerAttackSummary.PlayerXpEarned = xpEarned;
            pPlayer.GainXp(xpEarned, pTargetMonster.Level);

            if (pTargetMonster.IsAlive() && pRetaliate)
            {
                MonsterAttackPlayer(pTargetMonster, pPlayer, playerAttackSummary);
            }
            else if (pTargetMonster.IsDead())
                playerAttackSummary.PlayerKilledMonster = true;

            pTargetMonster.Persist();
            pPlayer.Persist();
            pPlayer.PlayerPlayerClass.Persist();
            return playerAttackSummary;
        }

        public static void MonsterReceiveDamge(MonsterInstance pMonsterInstance, int pDamageToReceive, Player pAttackingPlayer)
        {
            var onDamageReceivedEffects = pMonsterInstance.ActiveEffects.Where(effect => effect is IProcsOnDamageTaken);
            foreach (var effect in onDamageReceivedEffects)
            {
                effect.OnDamageReceived(pMonsterInstance, pAttackingPlayer, ref pDamageToReceive);
            }
            foreach (var effect in onDamageReceivedEffects)
                {
                    if (effect.Duration <= 0 || effect.Charges <= 0)
                    {
                        effect.Delete(effect.EffectId.Value);
                        pMonsterInstance.ActiveEffects = pMonsterInstance.ActiveEffects.Where(effectToRemove => effectToRemove.EffectType != effect.EffectType);
                    }
                }

            pMonsterInstance.SubtractHealth(pDamageToReceive, pAttackingPlayer);
        }
    }
}
