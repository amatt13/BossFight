using System;
using System.Collections.Generic;
using System.Linq;
using BossFight.BossFightEnums;
using BossFight.CustemExceptions;

namespace BossFight.Models
{
    public class PlayerClass : PersistableBase, IPersist<PlayerClass>
    {
        public override string TableName { get; set; } = "PlayerClass";
        public override string IdColumn { get; set; } = nameof(PlayerClassId);

        // Persisted on PlayerClass table
        public int PlayerClassId { get; set; }
        public string Name { get; set; }
        public double HpScale { get; set; }
        public double ManaScale { get; set; }
        public int PurchasePrice { get; set; }
        public int CritChance { get; set; }
        public int HpRegenRate { get; set; }
        public int ManaRegenRate { get; set; }
        public int AttackPowerBonus { get; set; }
        public int SpellPowerBonus { get; set; }
        public int BaseHealth { get; set; }
        public int BaseMana { get; set; }

        // From other tables
        public List<WeaponType> ProficientWeaponTypesList { get; set; }
        public List<PlayerClassRequirement> PlayerClassRequirementList { get; set; }
        public Dictionary<string, Ability> Abilities { get; set; }        
        
        public PlayerClass() { }

        public PlayerClass FindOne(int id)
        {
            return (PlayerClass)_findOne(id);
        }

        public override PersistableBase BuildObjectFromReader(MySqlConnector.MySqlDataReader reader)
        {
            throw new NotImplementedException("BuildObjectFromReader() cannot be called from base class 'PlayerClass'");
        }

        public override string ToString()
        {
            return Name;
        }

        public virtual string ShopString(int pLengthOfLongestPlayerClassName, int pLengthOfLongestPlayerClassCost)
        {
            var purchasePriceString = String.Format("{0:n0}", PurchasePrice);
            purchasePriceString = purchasePriceString.Replace(",", ".");  //TODO is this needed?
            var playerClassRequirementsString = String.Join(", ", (from req in PlayerClassRequirementList select $"Level {req.LevelRequirement} {req.ClassName}"));
            return $"{Name.PadLeft(pLengthOfLongestPlayerClassName, '.')} {purchasePriceString.PadLeft(pLengthOfLongestPlayerClassCost)} gold {playerClassRequirementsString}";
        }

        public virtual string InfoString()
        {
            var classNameStr = $"Class: { Name }";
            var critChanceStr = $"Base critical chance: { CritChance }%";
            var baseHpManaStr = $"Start hp: {BaseHealth}; start mana: { BaseMana }";
            var scalesStr = $"{ HpScale } hp per level; { ManaScale } mana per level";
            var regenStr = $"{ HpRegenRate } hp per regen tick; { ManaRegenRate } mana per regen tick";
            var proficientWeaponsStr = $"Is proficient with: { String.Join(", ", ProficientWeaponTypesList.Select(p => p.ToString())) }";
            var AbilitiesStr = $"Spell list:\n" + String.Join("\n", (from s in Abilities.Values select s.ToString()));
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

        public int CalculateMaxHp(int pClassLevel)
        {
            return (int)Math.Floor(HpScale * pClassLevel) + BaseHealth;
        }

        public int CalculateMaxMana(int pClassLevel)
        {
            return (int)Math.Floor(ManaScale * pClassLevel) + BaseMana;
        }

        public bool HaveMetRequirements(IEnumerable<PlayerPlayerClass> pPlayersUnlockedClasses)
        {
            var requirementsMet = true;
            foreach (var classReq in PlayerClassRequirementList)
            {
                try
                {
                    pPlayersUnlockedClasses.First(x => x.GetType() == classReq.PlayerClassType);
                }
                catch (InvalidOperationException)
                {
                    throw new MyException($"You need to unlock '{classReq.ClassName}' first");
                }
                foreach (var pc in pPlayersUnlockedClasses)
                {
                    if (pc.GetType() == classReq.PlayerClassType)
                        if (pc.Level < classReq.LevelRequirement)
                            throw new MyException($"Did not meet level requirements for {classReq.ClassName}");
                }
            }
            return requirementsMet;
        }
    }
}
