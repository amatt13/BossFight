using System;
using System.Linq;
using System.Text;
using BossFight.BossFightEnums;

namespace BossFight.Models
{
    public class Smite: Ability
    {
        private const int HealLimit = 16;
        private const int FloorHeal = 3;

        public Smite()
            : base("Smite", "Deals bonus damage to undead monsters", pManaCost: 4)
        { }

        public override void TargetEffect(ITarget pTarget)
        {
            AttackWithSmite(pTarget);
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
                else if (!Target.MonsterTypeList.Contains(MonsterType.UNDEAD))
                {
                    canCast = false;
                    pError.AppendLine($"Smite only works on undead targets. {Target.Name} is {EnumTextFormatter.EnumPrinter(Target.MonsterTypeList)}");
                }
            }

            return canCast;
        }

        private void AttackWithSmite(ITarget target)
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
