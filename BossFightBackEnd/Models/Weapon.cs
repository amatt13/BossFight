using System;
using BossFight.BossFightEnums;
using BossFight.Models.DB;
using BossFight.Models.Loot;

namespace BossFight.Models
{
    public class Weapon : LootItem, IPersist<Weapon>    {
        public const float DEFAULTWEAPONDROPCHANCE = 1.0f;
        
        public WeaponType WeaponType { get; set; }
        public string AttackMessage { get; set; }
        public bool BossWeapon { get; set; }
        public int WeaponLvl { get; set; }
        public int AttackPower { get; set; }
        public int AttackCritChance { get; set; }
        public int SpellPower { get; set; }
        public int SpellCritChance { get; set; }
        public override string TableName { get; set; } = "weapon";
        public override string IdColumn { get; set; } = "id";

        public Weapon() { }

        // public Weapon(AppDb db) : base(db)
        // { }

        public Weapon(int pWeaponId, string pName, string pAttackMessage, WeaponType pWeaponType = WeaponType.IMPROVISED, int pAttackPower = 1, int pCost = 0, int pAttackCritChance = 3, float pDropChance = Weapon.DEFAULTWEAPONDROPCHANCE, 
                      int pSpellPower = 0, int pSpellCritChance = 0, bool pBossWeapon = false, int pWeaponLvl = 1)
            : base(pWeaponId, pName, pDropChance, pCost)
        {
            WeaponType = pWeaponType;
            AttackMessage = pAttackMessage;
            BossWeapon = pBossWeapon;
            WeaponLvl = pWeaponLvl;
            AttackPower = pAttackPower;
            if (pBossWeapon)
                CalcWeaponStats();
            AttackCritChance = pAttackCritChance;
            SpellPower = pSpellPower;
            SpellCritChance = pSpellCritChance;
        }

        public Weapon FindOne(int id)
        {
            return (Weapon)_findOne(id);
        }

        public override PersistableBase BuildObjectFromReader(MySqlConnector.MySqlDataReader reader)
        {
            var weapon = new Weapon();

            while (reader.Read())
            {
                weapon.LootId = reader.GetInt32("id");
                weapon.LootName = reader.GetString("name");
                weapon.AttackMessage = reader.GetString("attack_message");
                weapon.BossWeapon = reader.GetBoolean("boss_weapon");
                weapon.WeaponLvl = reader.GetInt32("weapon_level");
                weapon.AttackPower = reader.GetInt32("attack_power");
                weapon.AttackCritChance = reader.GetInt32("attack_crit_chance");
                weapon.SpellPower = reader.GetInt32("spell_power");
                weapon.SpellCritChance = reader.GetInt32("spell_crit_chance");
                weapon.Cost = reader.GetInt32("gold_cost");
            }

            if (weapon.BossWeapon)
                weapon.CalcWeaponStats();

            return weapon;
        }

        public void SetBossWeaponProperties(int pWeaponLevel)
        {
            BossWeapon = true;
            WeaponLvl = pWeaponLevel;
            CalcWeaponStats();
        }

        public string InventoryStr()
        {
            var spellStr = "";
            if (SpellPower != 0 || SpellCritChance != 0)
                spellStr = $" - spell power { SpellPower } - spell crit chance { SpellCritChance }";
            return $"{ LootName } ({WeaponType}) - atk. power { AttackPower } - atk. crit chance - { AttackCritChance }{spellStr}";
        }

        public string GetWeaponTypeStr()
        {
            return WeaponType.ToString();
        }

        public string ShopStr(int pLengthOfLongestName, int pLengthOfLongestTypeName, int pLongestAttackDigit, int pLongestGoldPriceDigit, int pLongestCritChanceDigit, int pLongestSpellPowerDigit, int pLongestSpellCritChanceDigit)
        {
            var wName = $"{ LootName } ({GetWeaponTypeStr() })".PadLeft(pLengthOfLongestTypeName + pLengthOfLongestName, '.');
            var wCost = String.Format("{0:n0}", Cost).PadLeft(pLongestGoldPriceDigit, ' ');
            var wAttack = AttackPower.ToString().PadLeft(pLongestAttackDigit, ' ');
            var wCrit = $"{ AttackCritChance }%".PadLeft(pLongestCritChanceDigit + 1, ' ');
            var wSpellPower = "";
            if (SpellPower != 0)
                wSpellPower = $"{ SpellPower.ToString().PadLeft(pLongestSpellPowerDigit, ' ') } spell power";

            var wSpellCrit = "";
            if (SpellCritChance != 0)
                wSpellCrit = $" { SpellCritChance }%".PadLeft(pLongestSpellCritChanceDigit + 1, ' ') + " spell crit chance";

            return $"{ wName } { wCost} gold { wAttack } atk. power { wCrit } atk. crit chance { wSpellPower }{ wSpellCrit }";
        }

        public void CalcWeaponStats()
        {
            int attackPower, attackCritChance;
            if (WeaponType == WeaponType.STAFF)
            {
                attackPower = 1;
                attackCritChance = 3;
            }
            else
            {
                attackPower = (int)Math.Floor((double)WeaponLvl / 2);
                attackCritChance = (int)Math.Floor((double)WeaponLvl / 3);
                if (attackCritChance > 20)
                {
                    attackCritChance = 20;
                }
            }
            AttackPower = attackPower;
            AttackCritChance = attackCritChance;
        }
    }
}