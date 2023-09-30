namespace BossFight.Models
{
    public class DivineShield: Ability
    {
        public DivineShield()
            : base("Divine Shield", "Absorbs all incomming damage", pManaCost: 6)
        { }

        public override string UseAbility(ITarget pCaster, ITarget pTarget, bool pDontUseCasterEffect = false)
        {
            var dontUseCasterEffect = pTarget != null;
            return base.UseAbility(pCaster, pTarget, dontUseCasterEffect);
        }

        public override void CasterEffect()
        {
            CastDivineShield(Caster);
        }

        public override void TargetEffect(ITarget pTarget)
        {
            CastDivineShield(pTarget);
        }

        private void CastDivineShield(ITarget pTarget)
        {
            new DivineShieldEffect(pTarget);
        }
    }
}
