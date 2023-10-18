using System.Text;
using BossFight.BossFightEnums;
using BossFight.Controllers;

namespace BossFight.Models
{
    public class Smite: Ability
    {
        private bool _attackMonsterWithSmite = false;
        private bool _attacPlayerWithSmite = false;
        public Smite()
            : base("Smite", "Deals bonus damage to undead monsters", pManaCost: 4)
        { }

        public override void TargetEffect(ITarget pTarget)
        {
            if (_attackMonsterWithSmite)
            {
                AttackMonsterWithSmite((Player)Caster, (MonsterInstance)Target);
            }
            else if (_attacPlayerWithSmite)
            {
                AttacPlayerWithSmite((MonsterInstance)Caster, (Player)Target);
            }
        }

        public override bool CanCastAbility(StringBuilder pError)
        {
            var canCast = base.CanCastAbility(pError);
            _attackMonsterWithSmite = false;
            _attacPlayerWithSmite = false;

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

                if (canCast)
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
                            pError.AppendLine(error);
                        }
                    }
                    else if (Caster is MonsterInstance && Target is Player)
                    {
                        _attacPlayerWithSmite = true;
                    }
                    else
                    {
                        canCast = false;
                        pError.AppendLine("No valid targets");
                    }
                }
            }

            return canCast;
        }

        private void AttackMonsterWithSmite(Player pPLayer, MonsterInstance pMonster)
        {
            pPLayer.BonusMagicDmg += 13;  //TODO lol this is bad
            DamageDealer.PlayerAttackMonster(pPLayer, pMonster, true);
            pPLayer.BonusMagicDmg -= 13;
        }

        private void AttacPlayerWithSmite(MonsterInstance pMonster, Player pPLayer)
        {
            var monsterAttackSummary = new PlayerAttackSummary(pPLayer, pMonster);
            DamageDealer.MonsterAttackPlayer(pMonster, pPLayer, monsterAttackSummary);
        }
    }
}
