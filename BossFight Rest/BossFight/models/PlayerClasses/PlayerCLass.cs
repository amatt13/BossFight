using System;
using System.Collections.Generic;
using System.Linq;
using BossFight.BossFightEnums;
using BossFight.CustemExceptions;

namespace BossFight.Models
{
    public class PlayerClassRequirement
    {
        public Type PlayerClassType  {get; set;}
        public int LevelRequirement {get; set;}
        public string ClassName {get; set;}

        public PlayerClassRequirement(Type pClassType, int pLevelRequirement, string pClassName)
        {
            PlayerClassType = pClassType;
            LevelRequirement = pLevelRequirement;
            ClassName = pClassName;
        }

        public override string ToString()
        {
            return $"{ClassName}:{PlayerClassType} - lvl {LevelRequirement}";
        }
    }

    public abstract class PlayerClass
    {
        public static int DefaultHpScale = 2;
        public static int DefaultManaScale = 2;
        public static int BaseHealth = 10;
        public static int BaseMana = 10;

        public string Name {get; set;}
        public int Xp {get; set;}
        public int Level {get; set;}
        Player Player {get; set;}
        public List<WeaponType> ProficientWeaponTypesList {get; set;}
        public List<Ability> Abilities {get; set;}
        public double HpScale {get; set;} = DefaultHpScale;
        public double ManaScale {get; set;} = DefaultManaScale;
        public int MaxHp {get; set;}
        public int MaxMana {get; set;}

        // also properties ?
        public int PurchasePrice {get; set;} = 0;
        public double PlayerClassCritChance {get; set;} = 0;
        public int HpRegenRate {get; set;} = 1;
        public int ManaRegenRate {get; set;} = 1;
        public int AttackPowerBonus {get; set;} = 0;
        public int SpellPowerBonus {get; set;} = 0;

        public PlayerClass(string pName, int pXp, int pLevel, PlayerClass pPlayer, List<WeaponType> pProficientWeaponTypesList, double? pHpScale = null, double? pManaScale = null)
        {
            Name = pName;
            Xp = pXp;
            Level = pLevel;
            Player = pPlayer;
            ProficientWeaponTypesList = pProficientWeaponTypesList;
            Abilities = PrepareAvailableAbilities();
            MaxHp = 0;
            MaxMana = 0;
            if (pHpScale != null)
                HpScale = pHpScale.Value;
            if (pManaScale != null)
                ManaScale = pManaScale.Value;
            Recalculate();
        }

        public abstract Dictionary<String, Ability> PrepareAvailableAbilities();

        // fromDict
        public abstract PlayerClass FromDB(object playerClassDict, Player player);

        public static List<PlayerClassRequirement> GetClassUnlockRequirements()
        {
            return new List<PlayerClassRequirement>();
        }

        public virtual Dictionary<object, object> toDict()
        {
            var playerClassDict = new Dictionary<object, object>
            {
            };
            playerClassDict["name"] = Name;
            playerClassDict["xp"] = Xp;
            playerClassDict["level"] = Level;
            return playerClassDict;
        }

        public void Recalculate()
        {
            MaxHp = (int)Math.Floor(HpScale * Level) + BaseHealth;
            MaxMana = (int)Math.Floor(ManaScale * Level) + BaseMana;
            Abilities = PrepareAvailableAbilities();
        }

        public override string ToString()
        {
            return Name;
        }

        public virtual string ShopString(int pLengthOfLongestPlayerClassName, int pLengthOfLongestPlayerClassCost)
        {
            var purchasePriceString = String.Format("{0:n0}", PurchasePrice);
            purchasePriceString = purchasePriceString.Replace(",", ".");  //TODO is this needed?
            var playerClassRequirements = GetClassUnlockRequirements();
            var playerClassRequirementsString = String.Join(", ", (from req in playerClassRequirements select $"Level {req.LevelRequirement} {req.ClassName}"));
            return $"{Name.PadLeft(pLengthOfLongestPlayerClassName, '.')} {purchasePriceString.PadLeft(pLengthOfLongestPlayerClassCost)} gold {playerClassRequirementsString}";
        }

        public virtual string InfoString()
        {
            var classNameStr = $"Class: {Name}";
            var critChanceStr = $"Base critical chance: {PlayerClassCritChance}%";
            var baseHpManaStr = $"Start hp: {BaseHealth}; start mana: {BaseMana}";
            var scalesStr = $"{HpScale} hp per level; {ManaScale} mana per level";
            var regenStr = $"{HpRegenRate} hp per regen tick; {ManaRegenRate} mana per regen tick";
            var proficientWeaponsStr = $"Is proficient with: {String.Join(", ", ProficientWeaponTypesList.Select(p => p.ToString()))}";
            var prevLvl = Level;
            Level = 99;
            var allSpells = PrepareAvailableAbilities();
            Level = prevLvl;
            var AbilitiesStr = $"Spell list:\n" + String.Join("\n", (from s in allSpells.Values select s.ToString()));
            return String.Join("\n", new List<string> {
                    classNameStr,
                    critChanceStr,
                    baseHpManaStr,
                    scalesStr,
                    regenStr,
                    AbilitiesStr,
                    proficientWeaponsStr
                });
        }

        public int GetHealthRegenRate()
        {
            return HpRegenRate;
        }

        public int GetManaRegenRate()
        {
            return ManaRegenRate;
        }

        public object GetAttackPowerBonus()
        {
            return AttackPowerBonus;
        }

        public object GetSpellPowerBonus()
        {
            return SpellPowerBonus;
        }

        public void LevelUp()
        {
            Level += 1;
            Xp = 0;
            Recalculate();
            Player.RestoreAllHealthAndMana();
        }

        public virtual bool HaveMetRequirements(IEnumerable<PlayerClass> pPlayerClassList)
        {
            var requirementsMet = true;
            var classRequirements = GetClassUnlockRequirements();
            foreach (var classReq in classRequirements)
            {
                try
                {
                    pPlayerClassList.First(x => x.GetType() == classReq.PlayerClassType);
                }
                catch (InvalidOperationException)
                {
                    throw new MyException($"You need to unlock '{classReq.ClassName}' first");
                }
                foreach (var pc in pPlayerClassList)
                {
                    if (pc.GetType() == classReq.PlayerClassType)
                        if (pc.Level < classReq.LevelRequirement)
                            throw new MyException($"Did not meet level requirements for {classReq.ClassName}");
                }
            }
            return requirementsMet;
        }

        public static List<PlayerClass> ALLCLASSES = new List<PlayerClass> {
            new Cleric(null, level: 99),
            new Ranger(null, pLevel: 99),
            new Executioner(null, pLevel: 99),
            new Hexer(null, pLevel: 99),
            new Mage(null, pLevel: 99),
            new Barbarian(null, pLevel: 99),
            new Paladin(null, pLevel: 99),
            new MonsterHunter(null, pLevel: 99)
        };

    public static string displayInfo(string pClassName)
    {
        string text;
        try
        {
            var classToPrint = ALLCLASSES.First(pc => pc.Name.ToLower() == pClassName.ToLower());
            text = classToPrint.InfoString();
        }
        catch (InvalidOperationException)
        {
            text = $"Could not find class '{pClassName}'";
        }
        return text;
    }
    }
}