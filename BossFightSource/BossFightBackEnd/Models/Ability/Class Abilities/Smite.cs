using System.Text;
using BossFight.BossFightEnums;
using BossFight.Controllers;

namespace BossFight.Models
{
    public class Smite: Ability
    {
        private bool _attackMonsterWithSmite = false;
        private bool _attacPlayerWithSmite = false;
        private int _smite_damage = 13;

        public Smite()
            : base("Smite", "Deals bonus damage to undead monsters", pManaCost: 4)
        {
            OnlyTargetFoe = true;
        }

        public override void TargetEffect(ITarget pTarget, AbilityResult pAbilityResult)
        {
            if (_attackMonsterWithSmite)
            {
                AttackMonsterWithSmite((Player)Caster, (MonsterInstance)Target, pAbilityResult);
            }
            else if (_attacPlayerWithSmite)
            {
                AttackPlayerWithSmite((MonsterInstance)Caster, (Player)Target, pAbilityResult);
            }
        }

        public override bool CanCastAbility(ref string pError)
        {
            var canCast = base.CanCastAbility(ref pError);
            _attackMonsterWithSmite = false;
            _attacPlayerWithSmite = false;

            if (canCast)
            {
                if (Target.MonsterTypeList.Contains(MonsterType.UNDEAD))
                {
                    if (Caster is Player playerCaster && Target is MonsterInstance)
                    {
                        if (RequestValidator.PlayerCanAttackMonsterWithEquippedWeapon(playerCaster.PlayerId.Value, out string error))
                        {
                            _attackMonsterWithSmite = true;
                        }
                        else
                        {
                            canCast = false;
                            pError += error;
                        }
                    }
                    else if (Caster is MonsterInstance && Target is Player)
                    {
                        _attacPlayerWithSmite = true;
                    }
                    else
                    {
                        canCast = false;
                        pError += "No valid targets\n";
                    }
                }
                else
                {
                    canCast = false;
                    pError += $"Smite only works on undead targets. {Target.Name} is {EnumTextFormatter.EnumPrinter(Target.MonsterTypeList)}\n";
                }
            }

            return canCast;
        }

        private void AttackMonsterWithSmite(Player pPLayer, MonsterInstance pMonster, AbilityResult pAbilityResult)
        {
            pPLayer.BonusMagicDmg += _smite_damage;
            var summary = DamageDealer.PlayerAttackMonster(pPLayer, pMonster, AttackType.WEAPON_SWING, true);
            pAbilityResult.PlayerAttackSummary = summary;
            pAbilityResult.ReloadMonster = true;
            pAbilityResult.AbilityResultText = $"You used Smite to deal { _smite_damage } bonus damage";
            pPLayer.BonusMagicDmg -= _smite_damage;
        }

        private void AttackPlayerWithSmite(MonsterInstance pMonster, Player pPLayer, AbilityResult pAbilityResult)
        {
            var monsterAttackSummary = new PlayerAttackSummary(pPLayer, pMonster);
            DamageDealer.MonsterAttackPlayer(pMonster, pPLayer, monsterAttackSummary);
            pAbilityResult.PlayerAttackSummary = monsterAttackSummary;
        }
    }
}
