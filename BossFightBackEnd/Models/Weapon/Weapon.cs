using System;
using System.Collections.Generic;
using System.Linq;
using BossFight.BossFightEnums;
using BossFight.Extentions;
using BossFight.Models.DB;
using BossFight.Models.Loot;
using MySqlConnector;
using System.Text.Json.Serialization;

namespace BossFight.Models
{
    public class Weapon : LootItem<Weapon>, IPersist<Weapon>
    {
        [JsonIgnore]
        public const float DEFAULTWEAPONDROPCHANCE = 1.0f;

        [JsonIgnore]
        public override string TableName { get; set; } = nameof(Weapon);

        [JsonIgnore]
        public override string IdColumn { get; set; } = "WeaponId";
        
        public WeaponType WeaponType { get; set; }

        [PersistPropertyAttribute]
        public string AttackMessage { get; set; }

        [PersistPropertyAttribute]
        public bool BossWeapon { get; set; }

        [PersistPropertyAttribute]
        public int WeaponLevel { get; set; }

        [PersistPropertyAttribute]
        public int AttackPower { get; set; }

        [PersistPropertyAttribute]
        public int AttackCritChance { get; set; }

        [PersistPropertyAttribute]
        public int SpellPower { get; set; }

        [PersistPropertyAttribute]
        public int SpellCritChance { get; set; }

        public Weapon() { }

        public Weapon(int pWeaponId, string pName, string pAttackMessage, WeaponType pWeaponType = null, int pAttackPower = 1, int pCost = 0, int pAttackCritChance = 3, float pDropChance = Weapon.DEFAULTWEAPONDROPCHANCE, 
                      int pSpellPower = 0, int pSpellCritChance = 0, bool pBossWeapon = false, int pWeaponLvl = 1)
            : base(pWeaponId, pName, pDropChance, pCost)
        {
            WeaponType = pWeaponType;
            AttackMessage = pAttackMessage;
            BossWeapon = pBossWeapon;
            WeaponLevel = pWeaponLvl;
            AttackPower = pAttackPower;
            AttackCritChance = pAttackCritChance;
            SpellPower = pSpellPower;
            SpellCritChance = pSpellCritChance;
        }

        public override IEnumerable<Weapon> BuildObjectFromReader(MySqlDataReader reader, MySqlConnection pConnection)
        {
            var result = new List<Weapon>();

            while (reader.Read())
            {
                var weapon = new Weapon();
                weapon.LootId = reader.GetInt32("WeaponId");
                weapon.LootName = reader.GetString("Name");
                weapon.AttackMessage = reader.GetString(nameof(AttackMessage));
                weapon.BossWeapon = reader.GetBoolean(nameof(BossWeapon));
                weapon.WeaponLevel = reader.GetInt32(nameof(WeaponLevel));
                weapon.AttackPower = reader.GetInt32(nameof(AttackPower));
                weapon.AttackCritChance = reader.GetInt32(nameof(AttackCritChance));
                weapon.SpellPower = reader.GetInt32(nameof(SpellPower));
                weapon.SpellCritChance = reader.GetInt32(nameof(SpellCritChance));
                weapon.Cost = reader.GetInt32(nameof(Cost));
                var weaponTypeId = reader.GetIntNullable("WeaponTypeId");
                weapon.WeaponType = new WeaponType().FindOne(weaponTypeId.GetValueOrDefault(1));  // 1 == "Fists"
                result.Add(weapon);
            }

            return result;
        }

        public void SetBossWeaponProperties(int pWeaponLevel)
        {
            BossWeapon = true;
            WeaponLevel = pWeaponLevel;
            // CalcWeaponStats();
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
    }
}
