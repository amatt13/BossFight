using System;

namespace BossFight.Models
{
    public class Heal: Ability
    {
        private const int HealLimit = 16;
        private const int FloorHeal = 3;

        public Heal()
            : base("Heal", "Restores some of the target's HP", pManaCost: 6)
        { }

        public override void TargetEffect(ITarget pTarget, AbilityResult pAbilityResult)
        {
            CastHeal(pTarget, pAbilityResult);
        }

        public override bool CanCastAbility(ref string pError)
        {
            var canCast = base.CanCastAbility(ref pError);
            if (canCast)
            {
                if (Target.IsDead())
                {
                    canCast = false;
                    pError += $"{Target.Name} must be alive.\n";
                }
                else if (Target.IsAtFullHealth())
                {
                    canCast = false;
                    pError += $"{Target.Name} is already at full health.\n";
                }
            }

            return canCast;
        }

        private void CastHeal(ITarget target, AbilityResult pAbilityResult)
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
            pAbilityResult.AbilityResultText = $"You restored {amountRestored} of {target.PossessiveName()} HP ({target.Hp}/{target.GetMaxHp()} HP).\n";
            if (overHeal > 0)
                pAbilityResult.AbilityResultText += $"You over healed the target for {overHeal} HP\n";

            if (Caster is Player playerCaster)
            {
                int xpGained = (int)Math.Ceiling(amountRestored / 2.0);
                playerCaster.GainXp(xpGained);
                pAbilityResult.AbilityResultText += $"You gained {xpGained} XP.";
            }
        }
    }
}
