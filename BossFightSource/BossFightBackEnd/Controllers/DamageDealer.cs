using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BossFight.BossFightEnums;
using BossFight.Models;

namespace BossFight.Controllers
{
    public static class DamageDealer
    {
        public static bool BeforeMonsterAttackPlayer(MonsterInstance pMonster, PlayerAttackSummary pPlayerAttackSummary)
        {
            var continueAttack = true;
            var wasAlive = pMonster.IsAlive();

            var onBeforeAttackEffects = pMonster.ActiveEffects.Where(effect => effect is IProcsBeforeAttack);
            foreach (var effect in onBeforeAttackEffects)
            {
                effect.OnBeforeAttack(pMonster, effect.GetCaster(), pPlayerAttackSummary);
            }
            var gotKilled = wasAlive && pMonster.IsDead();
            if (gotKilled) {
                pPlayerAttackSummary.PlayerKilledMonster = true;
                continueAttack = false;
            }

            return continueAttack;
        }

        public static void MonsterAttackPlayer(MonsterInstance pMonster, Player pPlayerToAttack, PlayerAttackSummary pPlayerAttackSummary)
        {
            var damageTexts = new StringBuilder();
            var dealDamage = pMonster.IsAlive() && BeforeMonsterAttackPlayer(pMonster, pPlayerAttackSummary);;

            // TODO Check if monster dies to thron damage! We need to make sure a new monster is spawned!
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
                pPlayerToAttack.SubtractHealth(monsterDamage, pMonster);
                damageTexts.Append($"{ pMonster.Name } hits you for { monsterDamage } damage");
            }

            pPlayerAttackSummary.AddMonsterRetaliateMessage(damageTexts);
        }

        public static PlayerAttackSummary PlayerAttackMonster(Player pPlayer, MonsterInstance pTargetMonster, AttackType pAttackType, bool pRetaliate = true)
        {
            var playerAttackSummary = new PlayerAttackSummary(pPlayer, pTargetMonster);

            var playerAttack = pAttackType switch
            {
                AttackType.WEAPON_SWING => pPlayer.CalckulateWeaponAttackDamage(pTargetMonster, playerAttackSummary),
                AttackType.MAGIC => pPlayer.CalckulateWeaponMagicDamage(pTargetMonster, playerAttackSummary),
                _ => throw new Exception($"Invalid AttackType for player attack. AttackType : '{EnumTextFormatter.EnumPrinter(pAttackType)}'"),
            };
            MonsterReceiveDamge(pTargetMonster, playerAttack, pPlayer);

            var xpEarned = ExperienceCalculator.CalculateExperienceFromDamageDealtToMonster(playerAttack, pTargetMonster);
            playerAttackSummary.PlayerXpEarned = xpEarned;
            pPlayer.GainXp(xpEarned, pTargetMonster.Level);

            if (pTargetMonster.IsAlive() && pRetaliate)
            {
                MonsterAttackPlayer(pTargetMonster, pPlayer, playerAttackSummary);
            }
            else if (pTargetMonster.IsDead())
            {
                playerAttackSummary.PlayerKilledMonster = true;
                foreach(var activeEffect in pTargetMonster.ActiveEffects)
                {
                    activeEffect.Delete(activeEffect.EffectId.Value);
                }
            }

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
                if (effect.Charges <= 0)
                {
                    effect.Delete(effect.EffectId.Value);
                    pMonsterInstance.ActiveEffects = pMonsterInstance.ActiveEffects.Where(effectToRemove => effectToRemove.EffectType != effect.EffectType);
                }
            }

            pMonsterInstance.SubtractHealth(pDamageToReceive, pAttackingPlayer);
        }
    }
}
