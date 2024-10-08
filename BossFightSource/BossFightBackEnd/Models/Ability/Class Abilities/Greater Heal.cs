using System;

namespace BossFight.Models
{
    public class GreaterHeal: Ability
    {
        private const int HealLimit = 64;
        private const int FloorHeal = 12;

        public GreaterHeal()
            : base("Greater heal", "A potent healing spell", pManaCost: 15)
        { }

        public override void TargetEffect(ITarget pTarget, AbilityResult pAbilityResult)
        {
            CastGreaterHeal(pTarget, pAbilityResult);
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

        private void CastGreaterHeal(ITarget target, AbilityResult pAbilityResult)
        {
            int amountRestored = FloorHeal + (int)Math.Floor(Caster.Level * 3.0);
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
