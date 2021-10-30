using System;
using System.Linq;

namespace BossFight.Models
{
    public class AttackMessage
    {
        public Player Player { get; set; }
        public Monster Monster { get; set; }
        public bool PlayerCrit { get; set; }
        public string WeaponAttackMessage { get; set; }
        public string MonsterRetaliateMessage { get; set; }
        public bool PlayerExtraDamageFromBuffs { get; set; }
        public int PlayerXpEarned { get; set; }
        public string MonsterAffectedByDots { get; set; }
        public string AbilityExtra { get; set; }

        public AttackMessage(Player pPlayer, Monster pMonster)
        {
            Player = pPlayer;
            Monster = pMonster;
            PlayerCrit = false;
            WeaponAttackMessage = "";
            MonsterRetaliateMessage = "";
            PlayerExtraDamageFromBuffs = false;
            PlayerXpEarned = 0;
            MonsterAffectedByDots = "";
            AbilityExtra = "";
        }

        public override string ToString()
        {
            var pCrit = "";
            var wAtcMessage = "";
            var pEx = "";
            var pBuffs = "";
            var mRetlMessage = "";
            var mAffByDots = "";
            var abilityExtra = "";
            if (PlayerCrit)
            {
                pCrit = "**CRITICAL HIT**\n";
            }
            if (!String.IsNullOrEmpty(WeaponAttackMessage))
            {
                wAtcMessage = $"**Attack:** { WeaponAttackMessage }\n";
            }
            if (PlayerXpEarned > 0)
            {
                pEx = $"**XP:** You received { PlayerXpEarned } xp\n";
            }
            if (PlayerExtraDamageFromBuffs)
            {
                pBuffs = "**Buffs:** You dealt extra bonus damage because of your buffs\n";
            }
            if (!String.IsNullOrEmpty(MonsterRetaliateMessage))
            {
                mRetlMessage = $"**Monster attack:** { MonsterRetaliateMessage }\n";
            }
            if (!String.IsNullOrEmpty(MonsterAffectedByDots))
            {
                mAffByDots = $"**Monster dots:** { MonsterAffectedByDots }\n";
            }
            if (AbilityExtra.Any())
            {
                abilityExtra = $"{ AbilityExtra }\n";
            }
            return $"{ pCrit }{ wAtcMessage }{ pEx }{ mRetlMessage }{ pBuffs }{ mAffByDots }{ abilityExtra }";
        }
    }
}
