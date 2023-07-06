using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using BossFight.CustemExceptions;
using Microsoft.Identity.Client;

namespace BossFight.Models
{

    public class Heal: Ability
    {
        private const int HealLimit = 16;
        private const int FloorHeal = 3;

        public Heal()
            : base("Heal", "Restores some of the target's HP", 6, "heal")
        { }

        public override string UseAbility(Player pCaster, ITarget pTarget, bool pDontUseCasterEffect = false)
        {
            var dontUseCasterEffect = pTarget != null;
            return base.UseAbility(pCaster, pTarget, dontUseCasterEffect);
        }

        public override void CasterEffect()
        {
            CastHeal(this.Caster);
        }

        public override void TargetEffect(ITarget pTarget)
        {
            CastHeal(pTarget);
        }

        private void CastHeal(ITarget target)
        {
            if (!target.IsAtFullHealth())
            {
                int amountRestored = FloorHeal + (int)Math.Floor(this.Caster.Level * 1.5);
                int overHeal = 0;
                if (amountRestored > HealLimit)
                {
                    amountRestored = HealLimit;
                }

                if (target.Hp + amountRestored > target.GetMaxHp())
                {
                    overHeal = amountRestored;
                    amountRestored = target.GetMaxHp() - target.Hp;
                    overHeal -= overHeal;
                }

                target.Hp += amountRestored;
                UseAbilityText = $"You restored {amountRestored} of {target.PossessiveName()} HP ({target.Hp}/{target.GetMaxHp()} HP).";
                if (overHeal > 0)
                    UseAbilityText += $"\nYou over healed the target for {overHeal} HP";

                if (this.Caster is Player playerCaster)
                {
                    int xpGained = (int)Math.Ceiling(amountRestored / 2.0);
                    playerCaster.GainXp(xpGained);
                    UseAbilityText += "\nYou gained {xpGained} XP.";
                }
            }
            else
            {
                UseAbilityText = $"{target.Name} is already at full health.";
            }
        }
    }
}