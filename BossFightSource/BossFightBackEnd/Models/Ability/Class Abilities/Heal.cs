using System;
using System.Text;

namespace BossFight.Models
{
    public class Heal: Ability
    {
        private const int HealLimit = 16;
        private const int FloorHeal = 3;

        public Heal()
            : base("Heal", "Restores some of the target's HP", pManaCost: 6)
        { }

        public override void TargetEffect(ITarget pTarget)
        {
            CastHeal(pTarget);
        }

        public override bool CanCastAbility(StringBuilder pError)
        {
            var canCast = base.CanCastAbility(pError);
            if (canCast)
            {
                if (Target.IsDead())
                {
                    canCast = false;
                    pError.AppendLine($"{Target.Name} must be alive.");
                }
                else if (Target.IsAtFullHealth())
                {
                    canCast = false;
                    pError.AppendLine($"{Target.Name} is already at full health.");
                }
            }

            return canCast;
        }

        private void CastHeal(ITarget target)
        {
            int amountRestored = FloorHeal + (int)Math.Floor(Caster.Level * 1.5);
            int overHeal = 0;
            if (amountRestored > HealLimit)
            {
                amountRestored = HealLimit;
            }

            if (target.Hp + amountRestored > target.GetMaxHp())
            {
                overHeal = amountRestored;
                amountRestored = target.GetMaxHp() - target.Hp;
                overHeal -= amountRestored;
            }

            target.Hp += amountRestored;
            UseAbilityText.AppendLine($"You restored {amountRestored} of {target.PossessiveName()} HP ({target.Hp}/{target.GetMaxHp()} HP).");
            if (overHeal > 0)
                UseAbilityText.AppendLine($"You over healed the target for {overHeal} HP");

            if (Caster is Player playerCaster)
            {
                int xpGained = (int)Math.Ceiling(amountRestored / 2.0);
                playerCaster.GainXp(xpGained);
                UseAbilityText.AppendLine($"You gained {xpGained} XP.");
            }
        }
    }
}
