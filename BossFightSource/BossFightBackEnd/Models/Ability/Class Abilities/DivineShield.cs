using System.Text;

namespace BossFight.Models
{
    public class DivineShield: Ability
    {
        public DivineShield()
            : base("Divine Shield", "Absorbs all incomming damage", pManaCost: 6)
        { }

        public override void TargetEffect(ITarget pTarget, AbilityResult pAbilityResult)
        {
            CastDivineShield(pTarget, pAbilityResult);
        }

        public override bool CanCastAbility(ref string pError)
        {
            var canCast = base.CanCastAbility(ref pError);
            if (canCast)
            {
                if (Target.HasEffect(EffectType.DivineShield))
                {
                    pError += $"{Target.Name} is already affected by {Name}\n";
                    canCast = false;
                }
            }

            return canCast;
        }

        private void CastDivineShield(ITarget pTarget, AbilityResult pAbilityResult)
        {
            new DivineShieldEffect(pTarget);
            pAbilityResult.AbilityResultText = $"You cast {Name} on {Target.Name}";
        }
    }
}
