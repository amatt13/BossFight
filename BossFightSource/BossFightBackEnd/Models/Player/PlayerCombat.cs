using System;

namespace BossFight.Models
{
    public class PlayerCombat
    {
        private static readonly Random _random = new();
        private readonly Player _player;

        public PlayerCombat(Player player)
        {
            _player = player;
        }

        public int CalckulateWeaponAttackDamage(MonsterInstance pTargetMonster, PlayerAttackSummary pPlayerAttackSummary)
        {
            var isCrit = pTargetMonster.AttackOnMonsterIsCrit(GetAttackCritChance());
            var dmg = _player.Weapon.AttackPower + GetAttackBonus();

            if (isCrit)
            {
                dmg = (int)Math.Ceiling(1.25 * dmg);
                pPlayerAttackSummary.PlayerCrit = true;
            }

            pPlayerAttackSummary.PlayerTotalDamage = dmg;
            return dmg;
        }

        public int CalckulateWeaponMagicDamage(MonsterInstance pTargetMonster, PlayerAttackSummary pPlayerAttackSummary)
        {
            var isCrit = pTargetMonster.AttackOnMonsterIsCrit(GetSpellCritChance());
            var dmg = _player.Weapon.SpellPower + GetSpellBonus();

            if (isCrit)
            {
                dmg = (int)Math.Ceiling(1.25 * dmg);
                pPlayerAttackSummary.PlayerCrit = true;
            }

            pPlayerAttackSummary.PlayerTotalDamage = dmg;
            return dmg;
        }

        public void SubtractHealth(int pDamage, ITarget pAttacker)
        {
            if (pDamage > 0)
            {
                _player.Hp -= pDamage;

                if (_player.Hp < -3)
                    _player.Hp = -3;
            }
        }

        public bool IsKnockedOut()
        {
            return IsDead();
        }

        public bool IsDead()
        {
            return _player.Hp <= 0;
        }

        public bool IsAtFullHealth()
        {
            return _player.Hp >= _player.Stats.GetMaxHp();
        }

        public bool HasEnoughManaForAbility(Ability pAbility)
        {
            return _player.Mana >= pAbility.ManaCost;
        }

        public int GetAttackBonus()
        {
            return (int)Math.Floor((double)_player.Stats.GetLevel() / 2) + _player.BonusMagicDmg + _player.PlayerPlayerClass.AttackPowerBonus;
        }

        public int GetSpellBonus()
        {
            return (int)Math.Floor((double)_player.Stats.GetLevel() / 2) + _player.BonusMagicDmg + _player.PlayerPlayerClass.SpellPowerBonus;
        }

        public int GetAttackCritChance()
        {
            var critChance = _player.Weapon.AttackCritChance;
            critChance += _player.PlayerPlayerClass.CritChance;
            return critChance;
        }

        public int GetSpellCritChance()
        {
            var critChance = _player.Weapon.SpellCritChance;
            critChance += _player.PlayerPlayerClass.CritChance;
            return critChance;
        }

        public bool PlayerSpellIsCrit(int pBonusCritChance = 0)
        {
            var critChance = GetSpellCritChance();
            critChance += pBonusCritChance;
            var roll = _random.Next(0, 101);
            return roll <= critChance;
        }
    }
}
