using System;
using System.Collections.Generic;
using System.Linq;
using BossFight.BossFightEnums;
using BossFight.CustemExceptions;
using BossFight.Extentions;
using MySqlConnector;
using Newtonsoft.Json;

namespace BossFight.Models
{
    public class PlayerClass : PersistableBase, IPersist<PlayerClass>
    {
        [JsonIgnore]
        public override string TableName { get; set; } = "PlayerClass";
        [JsonIgnore]
        public override string IdColumn { get; set; } = nameof(PlayerClassId);

        // Persisted on PlayerClass table
        [PersistPropertyAttribute(true)]
        public int? PlayerClassId { get; set; }

        [PersistPropertyAttribute]
        public string Name { get; set; }

        [PersistPropertyAttribute]
        public double HpScale { get; set; }

        [PersistPropertyAttribute]
        public double ManaScale { get; set; }

        [PersistPropertyAttribute]
        public int PurchasePrice { get; set; }

        [PersistPropertyAttribute]
        public int CritChance { get; set; }

        [PersistPropertyAttribute]
        public int HpRegenRate { get; set; }

        [PersistPropertyAttribute]
        public int ManaRegenRate { get; set; }

        [PersistPropertyAttribute]
        public int AttackPowerBonus { get; set; }
        
        [PersistPropertyAttribute]
        public int SpellPowerBonus { get; set; }

        [PersistPropertyAttribute]
        public int BaseHealth { get; set; }

        [PersistPropertyAttribute]
        public int BaseMana { get; set; }

        // From other tables
        public IEnumerable<PlayerClassWeaponProficiency> ProficientWeaponTypesList { get; set; }
        public IEnumerable<PlayerClassRequirement> PlayerClassRequirementList { get; set; }
        public Dictionary<string, Ability> Abilities { get; set; }        
        
        public PlayerClass() { }

        public override IEnumerable<PersistableBase> BuildObjectFromReader(MySqlDataReader reader, MySqlConnection pConnection)
        {
            var result = new List<PersistableBase>();

            while (reader.Read())
            {
                var playerClass = new PlayerClass();
                playerClass.PlayerClassId = reader.GetInt(nameof(PlayerClassId));
                playerClass.Name = reader.GetString(nameof(Name));
                playerClass.HpScale = reader.GetDouble(nameof(HpScale));
                playerClass.ManaScale = reader.GetDouble(nameof(ManaScale));
                playerClass.PurchasePrice = reader.GetInt(nameof(PurchasePrice));
                playerClass.CritChance = reader.GetInt(nameof(CritChance));
                playerClass.HpRegenRate = reader.GetInt(nameof(HpRegenRate));
                playerClass.ManaRegenRate = reader.GetInt(nameof(ManaRegenRate));
                playerClass.AttackPowerBonus = reader.GetInt(nameof(AttackPowerBonus));
                playerClass.SpellPowerBonus = reader.GetInt(nameof(SpellPowerBonus));
                playerClass.BaseHealth = reader.GetInt(nameof(BaseHealth));
                playerClass.BaseMana = reader.GetInt(nameof(BaseMana));
                
                playerClass.ProficientWeaponTypesList = new PlayerClassWeaponProficiency().FindAll(playerClass.PlayerClassId);
                foreach(var x in playerClass.ProficientWeaponTypesList) { x.PlayerClass = playerClass; }
                
                playerClass.PlayerClassRequirementList = new PlayerClassRequirement().FindAll(playerClass.PlayerClassId);
                foreach(var x in playerClass.PlayerClassRequirementList) { x.PlayerClass = playerClass; }
                
                result.Add(playerClass);
            }

            return result;
        }

        public PlayerClass FindOne(int? id = null)
        {
            return (PlayerClass)_findOne(id);
        }

        public IEnumerable<PlayerClass> FindAll(int? id = null)
        {
            return _findAll(id).Cast<PlayerClass>();
        }

        public override string ToString()
        {
            return Name;
        }

        public virtual string ShopString(int pLengthOfLongestPlayerClassName, int pLengthOfLongestPlayerClassCost)
        {
            var purchasePriceString = String.Format("{0:n0}", PurchasePrice);
            purchasePriceString = purchasePriceString.Replace(",", ".");  //TODO is this needed?
            var playerClassRequirementsString = String.Join(", ", (from req in PlayerClassRequirementList select $"Level {req.LevelRequirement} {req.RequiredPlayerClassName}"));
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

        public bool HaveMetRequirementsForPlayerClass(IEnumerable<PlayerPlayerClass> pPlayersUnlockedClasses)
        {
            foreach (var classReq in PlayerClassRequirementList)
            {
                if (!pPlayersUnlockedClasses.Any(puc => puc.PlayerClassId == classReq.PlayerClassId))
                    throw new MyException($"You need to unlock '{ classReq.RequiredPlayerClassName }' first");
            }

            foreach (var classReq in PlayerClassRequirementList)
                {
                    if (!pPlayersUnlockedClasses.Any(puc => puc.PlayerClassId == classReq.PlayerClassId && classReq.LevelRequirement <= puc.Level))
                        throw new MyException($"Did not meet level requirements for { classReq.RequiredPlayerClassName }. Needs to be level {classReq.LevelRequirement} or above.");
                }
            return true;
        }
    }
}
