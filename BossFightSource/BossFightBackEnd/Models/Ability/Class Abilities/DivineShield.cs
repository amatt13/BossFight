using System.Text;

namespace BossFight.Models
{
    public class DivineShield: Ability
    {
        public DivineShield()
            : base("Divine Shield", "Absorbs all incomming damage", pManaCost: 6)
        { }

        public override string UseAbility(ITarget pCaster, ITarget pTarget)
        {
            return base.UseAbility(pCaster, pTarget);
        }

        public override void TargetEffect(ITarget pTarget)
        {
            CastDivineShield(pTarget);
        }

        public override bool CanCastAbility(StringBuilder pError)
        {
            var canCast = base.CanCastAbility(pError);
            if (canCast)
            {
                if (Target.HasEffect(EffectType.DivineShield))
                {
                    pError.AppendLine($"{Target.Name} is already affected by {Name}");
                    canCast = false;
                }
            }

            return canCast;
        }

        private void CastDivineShield(ITarget pTarget)
        {
            new DivineShieldEffect(pTarget);
            UseAbilityText.AppendLine($"You cast {Name} on {Target.Name}");
        }
    }
}
