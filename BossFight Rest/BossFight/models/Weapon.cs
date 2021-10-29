namespace BossFight.Models
{
    public class Weapon : LootItem
    {
        public static int DEFAULTWEAPONDROPCHANCE = 1.0;
        
        public Weapon(int pWeaponId, string pName, string pAttackMessage, WeaponType.IMPROVISED pWeaponType, int pAttackPower = 1, int pCost = 0, int pAttackCritChance = 3, double pDropChance = DEFAULTWEAPONDROPCHANCE, 
                      int pSpellPower = 0, int pSpellCritChance = 0, bool pBossWeapon = false, int pWeaponLvl = 1)
            : base(name, dropChance, cost)
        {
            weaponType = weaponType;
            attackMessage = attackMessage;
            bossWeapon = bossWeapon;
            weaponLvl = weaponLvl;
            attackPower = attackPower;
            if (bossWeapon)
            {
                calcWeaponStats();
            }
            attackCritChance = attackCritChance;
            spellPower = spellPower;
            spellCritChance = spellCritChance;
        }

        public virtual object setBossWeaponProperties(object weaponLevel)
        {
            bossWeapon = true;
            weaponLvl = weaponLevel;
            calcWeaponStats();
        }

        public virtual object inventoryStr()
        {
            var spellStr = "";
            if (spellPower != 0 || spellCritChance != 0)
            {
                spellStr = " - spell power { self.spellPower } - spell crit chance { self.spellCritChance }";
            }
            return "{ self.lootName } ({self.weaponType}) - atk. power { self.attackPower } - atk. crit chance - { self.attackCritChance }{spellStr}";
        }

        public virtual object getWeaponTypeStr()
        {
            return weaponType.ToString();
        }

        public virtual object shopStr(
            object lengthOfLongestName,
            object lengthOfLongestTypeName,
            object longestAttackDigit,
            object longestGoldPriceDigit,
            object longestCritChanceDigit,
            object longestSpellPowerDigit,
            object longestSpellCritChanceDigit)
        {
            var wName = "{self.lootName} ({self.getWeaponTypeStr()})".ljust(lengthOfLongestTypeName + lengthOfLongestName, ".");
            var wCost = "{self.cost:,}".ljust(longestGoldPriceDigit, " ");
            var wAttack = attackPower.ToString().ljust(longestAttackDigit, " ");
            var wCrit = "{self.attackCritChance}%".ljust(longestCritChanceDigit + 1, " ");
            var wSpellPower = "";
            if (spellPower != 0)
            {
                wSpellPower = "{str(self.spellPower).ljust(longestSpellPowerDigit, ' ')} spell power";
            }
            var wSpellCrit = "";
            if (spellCritChance != 0)
            {
                wSpellCrit = " {self.spellCritChance}%".ljust(longestSpellCritChanceDigit + 1, " ") + " spell crit chance";
            }
            return "{ wName } { wCost} gold { wAttack } atk. power { wCrit } atk. crit chance {wSpellPower}{wSpellCrit}";
        }

        public virtual object calcWeaponStats()
        {
            object attackPower;
            if (object.ReferenceEquals(weaponType, WeaponType.STAFF))
            {
                attackPower = 1;
                var attackCritChance = 3;
            }
            else
            {
                attackPower = math.floor(weaponLvl / 2);
                attackCritChance = math.floor(weaponLvl / 3);
                if (attackCritChance > 20)
                {
                    attackCritChance = 20;
                }
            }
            attackPower = attackPower;
            attackCritChance = attackCritChance;
        }
    }
}