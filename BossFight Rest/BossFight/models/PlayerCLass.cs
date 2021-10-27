using System;
using System.Collections.Generic;
using System.Linq;
using BossFight.BossFightEnums;
using BossFight.CustemExceptions;

namespace BossFight.Models
{
    public class PlayerClassRequirement
    {
        public int ClassId {get; set;}
        public int LevelRequirement {get; set;}
        public string ClassName {get; set;}

        public PlayerClassRequirement(int pClassId, int pLevelRequirement, string pClassName)
        {
            ClassId = pClassId;
            LevelRequirement = pLevelRequirement;
            ClassName = pClassName;
        }

        public override string ToString()
        {
            return $"{ClassName}:{ClassId} - lvl {LevelRequirement}";
        }
    }

    public abstract class PlayerClass
    {
        public static int DefaultHpScale = 2;
        public static int DefaultManaScale = 2;
        public static int PlayerClassId = -1;
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
        
        protected int _classId = PlayerClassId;
        public int ClassId 
        {
            get { return _classId; } 
            set { _classId = value; }
        }

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
            var abilitiesStr = $"Spell list:\n" + String.Join("\n", (from s in allSpells.Values select s.ToString()));
            return String.Join("\n", new List<string> {
                    classNameStr,
                    critChanceStr,
                    baseHpManaStr,
                    scalesStr,
                    regenStr,
                    abilitiesStr,
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
                    pPlayerClassList.First(x => x.ClassId == classReq.ClassId);
                }
                catch (InvalidOperationException)
                {
                    throw new MyException($"You need to unlock '{classReq.ClassName}' first");
                }
                foreach (var pc in pPlayerClassList)
                {
                    if (pc.ClassId == classReq.ClassId)
                        if (pc.Level < classReq.LevelRequirement)
                            throw new MyException($"Did not meet level requirements for {classReq.ClassName}");
                }
            }
            return requirementsMet;
        }
    }

    public class Executioner : PlayerClass
    {
        public object playerClassId = 2;

        public Executioner(object player, object xp = 0, object level = 1)
            : base(xp, level, player, new List<object> {
                    WeaponType.SWORD,
                    WeaponType.MACE,
                    WeaponType.DAGGER,
                    WeaponType.POLEARM
            }, hpScale: 2.5, manaScale: 1.5)
        {
            PlayerClassCritChance = 2;
        }

        [classmethod]
        public static object FromDB(object cls, object playerClassDict = dict, object player)
        {
            var xp = playerClassDict["xp"];
            var level = Convert.ToInt32(playerClassDict["level"]);
            var playerClass = cls(player, xp, level);
            playerClass.recalculate();
            return playerClass;
        }

        public virtual object prepareAvailableAbilities()
        {
            var unlockedAbilities = new Dictionary<object, object>
            {
            };
            var sackOnHead = abilities.SackOnHead();
            unlockedAbilities[sackOnHead.magicWord] = sackOnHead;
            if (Level >= 5)
            {
                var execute = abilities.Execute();
                unlockedAbilities[execute.magicWord] = execute;
            }
            return unlockedAbilities;
        }
    }

    public class Ranger
        : PlayerClass
    {

        public object playerClassId = 3;

        public Ranger(object player, object xp = 0, object level = 1)
            : base(xp, level, player, new List<object> {
                    WeaponType.DAGGER,
                    WeaponType.THROWN,
                    WeaponType.BOW,
                    WeaponType.IMPROVISED
            })
        {
            PlayerClassCritChance = 4;
        }

        [classmethod]
        public static object FromDB(object cls, object playerClassDict = dict, object player)
        {
            var xp = playerClassDict["xp"];
            var level = Convert.ToInt32(playerClassDict["level"]);
            var playerClass = cls(player, xp, level);
            playerClass.recalculate();
            return playerClass;
        }

        public virtual object prepareAvailableAbilities()
        {
            var unlockedAbilities = new Dictionary<object, object>
            {
            };
            var doubleStrike = abilities.DoubleStrike();
            unlockedAbilities[doubleStrike.magicWord] = doubleStrike;
            if (Level >= 5)
            {
                var poisonedBait = abilities.PoisonedBait();
                unlockedAbilities[poisonedBait.magicWord] = poisonedBait;
            }
            return unlockedAbilities;
        }
    }

    public class Hexer
        : PlayerClass
    {

        public object playerClassId = 4;

        public Hexer(object player, object xp = 0, object level = 1)
            : base(xp, level, player, new List<object> {
                    WeaponType.DAGGER,
                    WeaponType.STAFF
            }, hpScale: 3.5, manaScale: 4)
        {
            PlayerClassCritChance = 3;
            PurchasePrice = 1000;
            BaseHealth = 12;
            BaseMana = 13;
            ManaRegenRate = 2;
            AttackPowerBonus = 1;
        }

        [classmethod]
        public static object FromDB(object cls, object playerClassDict = dict, object player)
        {
            var xp = playerClassDict["xp"];
            var level = Convert.ToInt32(playerClassDict["level"]);
            var playerClass = cls(player, xp, level);
            playerClass.recalculate();
            return playerClass;
        }

        public virtual object prepareAvailableAbilities()
        {
            var unlockedAbilities = new Dictionary<object, object>
            {
            };
            var hex = abilities.Hex();
            unlockedAbilities[hex.magicWord] = hex;
            if (Level >= 5)
            {
                var fractureSkin = abilities.FractureSkin();
                unlockedAbilities[fractureSkin.magicWord] = fractureSkin;
            }
            return unlockedAbilities;
        }
    }

    public class Mage
        : PlayerClass
    {

        public object playerClassId = 5;

        public Mage(object player, object xp = 0, object level = 1)
            : base(xp, level, player, new List<object> {
                    WeaponType.DAGGER,
                    WeaponType.STAFF
            }, hpScale: 3, manaScale: 4.5)
        {
            PlayerClassCritChance = 4;
            PurchasePrice = 1000;
            BaseHealth = 11;
            BaseMana = 14;
            ManaRegenRate = 2;
            SpellPowerBonus = 1;
        }

        [classmethod]
        public static object FromDB(object cls, object playerClassDict = dict, object player)
        {
            var xp = playerClassDict["xp"];
            var level = Convert.ToInt32(playerClassDict["level"]);
            var playerClass = cls(player, xp, level);
            playerClass.recalculate();
            return playerClass;
        }

        public virtual object prepareAvailableAbilities()
        {
            var unlockedAbilities = new Dictionary<object, object>
            {
            };
            var ignite = abilities.Ignite();
            unlockedAbilities[ignite.magicWord] = ignite;
            if (Level >= 5)
            {
                var enchantWeapon = abilities.EnchantWeapon();
                unlockedAbilities[enchantWeapon.magicWord] = enchantWeapon;
            }
            return unlockedAbilities;
        }
    }

    public class Barbarian
        : PlayerClass
    {

        public object playerClassId = 6;

        public Barbarian(object player, object xp = 0, object level = 1)
            : base(xp, level, player, new List<object> {
                    WeaponType.SWORD,
                    WeaponType.MACE,
                    WeaponType.DAGGER,
                    WeaponType.POLEARM,
                    WeaponType.IMPROVISED,
                    WeaponType.THROWN,
                    WeaponType.AXE,
                    WeaponType.BOW
            }, hpScale: 6, manaScale: 3)
        {
            PlayerClassCritChance = 7;
            PurchasePrice = 1500;
            BaseHealth = 20;
            BaseMana = 14;
            HpRegenRate = 2;
            AttackPowerBonus = 2;
            SpellPowerBonus = 1;
        }

        [classmethod]
        public static object FromDB(object cls, object playerClassDict = dict, object player)
        {
            var xp = playerClassDict["xp"];
            var level = Convert.ToInt32(playerClassDict["level"]);
            var playerClass = cls(player, xp, level);
            playerClass.recalculate();
            return playerClass;
        }

        [staticmethod]
        public static object getClassUnlockRequirements()
        {
            return new List<object> {
                    PlayerClassRequirement(Executioner.playerClassId, 10, "Executioner")
                };
        }

        public virtual object prepareAvailableAbilities()
        {
            var unlockedAbilities = new Dictionary<object, object>
            {
            };
            var shout = abilities.Shout();
            unlockedAbilities[shout.magicWord] = shout;
            if (Level >= 5)
            {
                var frenzy = abilities.Frenzy();
                unlockedAbilities[frenzy.magicWord] = frenzy;
            }
            return unlockedAbilities;
        }
    }

    public class Paladin
        : PlayerClass
    {

        public object playerClassId = 7;

        public Paladin(object player, object xp = 0, object level = 1)
            : base(xp, level, player, new List<object> {
                    WeaponType.SWORD,
                    WeaponType.MACE,
                    WeaponType.DAGGER,
                    WeaponType.POLEARM,
                    WeaponType.AXE
            }, hpScale: 5.5, manaScale: 3.5)
        {
            PlayerClassCritChance = 6;
            PurchasePrice = 1500;
            BaseHealth = 16;
            BaseMana = 16;
            HpRegenRate = 2;
            AttackPowerBonus = 1;
            SpellPowerBonus = 1;
        }

        [classmethod]
        public static object FromDB(object cls, object playerClassDict = dict, object player)
        {
            var xp = playerClassDict["xp"];
            var level = Convert.ToInt32(playerClassDict["level"]);
            var playerClass = cls(player, xp, level);
            playerClass.recalculate();
            return playerClass;
        }

        [staticmethod]
        public static object getClassUnlockRequirements()
        {
            return new List<object> {
                    PlayerClassRequirement(Cleric.playerClassId, 10, "Cleric")
                };
        }

        public virtual object prepareAvailableAbilities()
        {
            var unlockedAbilities = new Dictionary<object, object>
            {
            };
            var sacrifice = abilities.Sacrifice();
            unlockedAbilities[sacrifice.magicWord] = sacrifice;
            var heal = abilities.Heal();
            unlockedAbilities[heal.magicWord] = heal;
            if (Level >= 5)
            {
                var fullRestore = abilities.GreaterHeal();
                unlockedAbilities[fullRestore.magicWord] = fullRestore;
            }
            return unlockedAbilities;
        }
    }

    public class MonsterHunter
        : PlayerClass
    {

        public object playerClassId = 8;

        public MonsterHunter(object player, object xp = 0, object level = 1)
            : base(xp, level, player, new List<object> {
                    WeaponType.SWORD,
                    WeaponType.MACE,
                    WeaponType.DAGGER,
                    WeaponType.POLEARM,
                    WeaponType.IMPROVISED,
                    WeaponType.THROWN,
                    WeaponType.AXE,
                    WeaponType.BOW
            }, hpScale: 5, manaScale: 4)
        {
            PlayerClassCritChance = 7;
            PurchasePrice = 1500;
            BaseHealth = 18;
            BaseMana = 16;
            ManaRegenRate = 2;
            AttackPowerBonus = 2;
            SpellPowerBonus = 1;
        }

        [classmethod]
        public static object FromDB(object cls, object playerClassDict = dict, object player)
        {
            var xp = playerClassDict["xp"];
            var level = Convert.ToInt32(playerClassDict["level"]);
            var playerClass = cls(player, xp, level);
            playerClass.recalculate();
            return playerClass;
        }

        [staticmethod]
        public static object getClassUnlockRequirements()
        {
            return new List<object> {
                    PlayerClassRequirement(Ranger.playerClassId, 10, "Ranger")
                };
        }

        public virtual object prepareAvailableAbilities()
        {
            var unlockedAbilities = new Dictionary<object, object>
            {
            };
            var overSizedBearTrap = abilities.OverSizedBearTrap();
            unlockedAbilities[overSizedBearTrap.magicWord] = overSizedBearTrap;
            var turnWeaponToSilver = abilities.TurnWeaponToSilver();
            unlockedAbilities[turnWeaponToSilver.magicWord] = turnWeaponToSilver;
            if (Level >= 5)
            {
                var bigGameTrophy = abilities.BigGameTrophy();
                unlockedAbilities[bigGameTrophy.magicWord] = bigGameTrophy;
            }
            return unlockedAbilities;
        }
    }

    public static object ALLCLASSES = new List<object> {
            Cleric(null, level: 99),
            Ranger(null, level: 99),
            Executioner(null, level: 99),
            Hexer(null, level: 99),
            Mage(null, level: 99),
            Barbarian(null, level: 99),
            Paladin(null, level: 99),
            MonsterHunter(null, level: 99)
        };

    public static object displayInfo(object className = str)
    {
        object text;
        try
        {
            var classToPrint = next(from pc in ALLCLASSES
                                      where pc.name.lower() == className.lower()
                                      select pc);
            text = classToPrint.infoStr();
        }
        catch (StopIteration)
        {
            text = "Could not find class '{className}'";
        }
        return text;
    }
}