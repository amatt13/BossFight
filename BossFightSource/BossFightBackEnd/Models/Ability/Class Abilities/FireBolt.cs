using BossFight.BossFightEnums;
using BossFight.Controllers;

namespace BossFight.Models
{
    public class FireBolt: Ability
    {
        private bool _attackMonsterWithFireBoly = false;
        private bool _attacPlayerWithFireBolt = false;
        private int _fire_bolt_damage;
        private int _damagePerTick = 5;
        public FireBolt()
            : base("Fire bolt", "Throw a small bolt of fire", pManaCost: 4)
        {
            OnlyTargetMonster = true;
        }

        public override void TargetEffect(ITarget pTarget, AbilityResult pAbilityResult)
        {
            if (_attackMonsterWithFireBoly)
            {
                AttackMonsterWithFireBolt((Player)Caster, (MonsterInstance)Target, pAbilityResult);
            }
            else if (_attacPlayerWithFireBolt)
            {
                AttackPlayerWithFireBolt((MonsterInstance)Caster, (Player)Target, pAbilityResult);
            }
        }

        public override bool CanCastAbility(ref string pError)
        {
            var canCast = base.CanCastAbility(ref pError);
            _attackMonsterWithFireBoly = false;
            _attacPlayerWithFireBolt = false;

            if (canCast)
            {
                if (Target.IsDead())
                {
                    canCast = false;
                    pError += $"{Target.Name} must be alive.\n";
                }
                else
                {
                    if (Caster is Player playerCaster && Target is MonsterInstance)
                    {
                        if (RequestValidator.PlayerCanAttackMonsterWithEquippedWeapon(playerCaster.PlayerId.Value, out string error))
                        {
                            _attackMonsterWithFireBoly = true;
                        }
                        else
                        {
                            canCast = false;
                            pError += error;
                        }
                    }
                    else if (Caster is MonsterInstance && Target is Player)
                    {
                        _attacPlayerWithFireBolt = true;
                    }
                    else
                    {
                        canCast = false;
                        pError += "No valid targets\n";
                    }
                }
            }

            return canCast;
        }

        private void AttackMonsterWithFireBolt(Player pPLayer, MonsterInstance pMonster, AbilityResult pAbilityResult)
        {
            // Apply dot
            new DamageOverTimeEffect(Target, Caster, _damagePerTick);
            pAbilityResult.AbilityResultText = $"You cast {Name} on {Target.Name}";

            // Apply magic attack damage
            pPLayer.BonusMagicDmg += _fire_bolt_damage;
            var summary = DamageDealer.PlayerAttackMonster(pPLayer, pMonster, AttackType.MAGIC, true);
            pAbilityResult.PlayerAttackSummary = summary;
            pAbilityResult.ReloadMonster = true;
            pAbilityResult.AbilityResultText = $"Your {Name} dealt { summary.PlayerTotalDamage } damage";
            pPLayer.BonusMagicDmg -= _fire_bolt_damage;
        }

        private void AttackPlayerWithFireBolt(MonsterInstance pMonster, Player pPLayer, AbilityResult pAbilityResult)
        {
            // Apply dot
            new DamageOverTimeEffect(Target, Caster, _damagePerTick);
            pAbilityResult.AbilityResultText = $"{Caster.Name} cast {Name} on {Target.Name}";

            var monsterAttackSummary = new PlayerAttackSummary(pPLayer, pMonster);
            DamageDealer.MonsterAttackPlayer(pMonster, pPLayer, monsterAttackSummary);
            pAbilityResult.PlayerAttackSummary = monsterAttackSummary;
        }
    }
}
