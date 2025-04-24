using BossFight.BossFightEnums;
using BossFight.Controllers;

namespace BossFight.Models
{
    public class CutDeep: Ability
    {
        private bool _attackMonsterWithCutDeep = false;
        private bool _attacPlayerWithCutDeep = false;
        private int _damagePerTick = 3;
        private int _charges = 4;
        public CutDeep()
            : base("Cut deep", "Inflicts a severe wound that causes steady blood loss after the initial strike", pManaCost: 6)
        {
            OnlyTargetFoe = true;
        }

        public override void TargetEffect(ITarget pTarget, AbilityResult pAbilityResult)
        {
            if (_attackMonsterWithCutDeep)
            {
                AttackMonsterWithCutDeep((Player)Caster, (MonsterInstance)Target, pAbilityResult);
            }
            else if (_attacPlayerWithCutDeep)
            {
                AttackPlayerWithCutDeep((MonsterInstance)Caster, (Player)Target, pAbilityResult);
            }
        }

        public override bool CanCastAbility(ref string pError)
        {
            var canCast = base.CanCastAbility(ref pError);
            _attackMonsterWithCutDeep = false;
            _attacPlayerWithCutDeep = false;

            if (canCast)
            {
                if (Caster is Player playerCaster && Target is MonsterInstance)
                {
                    if (RequestValidator.PlayerCanAttackMonsterWithEquippedWeapon(playerCaster.PlayerId.Value, out string error))
                    {
                        _attackMonsterWithCutDeep = true;
                    }
                    else
                    {
                        canCast = false;
                        pError += error;
                    }
                }
                else if (Caster is MonsterInstance && Target is Player)
                {
                    _attacPlayerWithCutDeep = true;
                }
                else
                {
                    canCast = false;
                    pError += "No valid targets\n";
                }
            }

            return canCast;
        }

        private void AttackMonsterWithCutDeep(Player pPLayer, MonsterInstance pMonster, AbilityResult pAbilityResult)
        {
            // Apply dot
            new DamageOverTimeEffect(Target, Caster, _damagePerTick, _charges);
            pAbilityResult.AbilityResultText = $"You use {Name} on {Target.Name}";

            var summary = DamageDealer.PlayerAttackMonster(pPLayer, pMonster, AttackType.MAGIC, true);
            pAbilityResult.PlayerAttackSummary = summary;
            pAbilityResult.ReloadMonster = true;
            pAbilityResult.AbilityResultText = $"{Name} has been inflicted on { Target.Name } damage";
        }

        private void AttackPlayerWithCutDeep(MonsterInstance pMonster, Player pPLayer, AbilityResult pAbilityResult)
        {
            // Apply dot
            new DamageOverTimeEffect(Target, Caster, _damagePerTick, _charges);
            pAbilityResult.AbilityResultText = $"{Caster.Name} use {Name} on {Target.Name}";

            var monsterAttackSummary = new PlayerAttackSummary(pPLayer, pMonster);
            DamageDealer.MonsterAttackPlayer(pMonster, pPLayer, monsterAttackSummary);
            pAbilityResult.PlayerAttackSummary = monsterAttackSummary;
        }
    }
}
