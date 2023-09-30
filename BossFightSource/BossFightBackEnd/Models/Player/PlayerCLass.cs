using System;
using System.Collections.Generic;
using System.Linq;
using BossFight.BossFightEnums;
using BossFight.CustemExceptions;
using BossFight.Extentions;
using MySqlConnector;
using System.Text.Json.Serialization;

namespace BossFight.Models
{
    public class PlayerClass : PersistableBase<PlayerClass>, IPersist<PlayerClass>
    {
        [JsonIgnore]
        public override string TableName { get; set; } = nameof(PlayerClass);
        [JsonIgnore]
        public override string IdColumn { get; set; } = nameof(PlayerClassId);

        // Persisted on PlayerClass table
        [PersistProperty(true)]
        public int? PlayerClassId { get; set; }

        [PersistProperty]
        public string Name { get; set; }

        [PersistProperty]
        public double HpScale { get; set; }

        [PersistProperty]
        public double ManaScale { get; set; }

        [PersistProperty]
        public int PurchasePrice { get; set; }

        [PersistProperty]
        public int CritChance { get; set; }

        [PersistProperty]
        public int HpRegenRate { get; set; }

        [PersistProperty]
        public int ManaRegenRate { get; set; }

        [PersistProperty]
        public int AttackPowerBonus { get; set; }

        [PersistProperty]
        public int SpellPowerBonus { get; set; }

        [PersistProperty]
        public int BaseHealth { get; set; }

        [PersistProperty]
        public int BaseMana { get; set; }

        [PersistProperty]
        public string Description { get; set; }

        // From other tables
        public IEnumerable<PlayerClassWeaponProficiency> ProficientWeaponTypesList { get; set; }
        public IEnumerable<PlayerClassRequirement> PlayerClassRequirementList { get; set; }
        public Dictionary<string, Ability> Abilities { get; set; }

        public PlayerClass() { }

        public override IEnumerable<PlayerClass> BuildObjectFromReader(MySqlDataReader reader, MySqlConnection pConnection)
        {
            var result = new List<PlayerClass>();

            while (reader.Read())
            {
                var playerClass = new PlayerClass
                {
                    PlayerClassId = reader.GetInt(nameof(PlayerClassId)),
                    Name = reader.GetString(nameof(Name)),
                    HpScale = reader.GetDouble(nameof(HpScale)),
                    ManaScale = reader.GetDouble(nameof(ManaScale)),
                    PurchasePrice = reader.GetInt(nameof(PurchasePrice)),
                    CritChance = reader.GetInt(nameof(CritChance)),
                    HpRegenRate = reader.GetInt(nameof(HpRegenRate)),
                    ManaRegenRate = reader.GetInt(nameof(ManaRegenRate)),
                    AttackPowerBonus = reader.GetInt(nameof(AttackPowerBonus)),
                    SpellPowerBonus = reader.GetInt(nameof(SpellPowerBonus)),
                    BaseHealth = reader.GetInt(nameof(BaseHealth)),
                    BaseMana = reader.GetInt(nameof(BaseMana)),
                    Description = reader.GetString(nameof(Description))
                };

                playerClass.ProficientWeaponTypesList = new PlayerClassWeaponProficiency().FindAll(playerClass.PlayerClassId);
                foreach(var x in playerClass.ProficientWeaponTypesList) { x.PlayerClass = playerClass; }

                playerClass.PlayerClassRequirementList = new PlayerClassRequirement().FindAll(playerClass.PlayerClassId);
                foreach(var x in playerClass.PlayerClassRequirementList) { x.PlayerClass = playerClass; }

                result.Add(playerClass);
            }

            return result;
        }

        public override string ToString()
        {
            return Name;
        }


        // public virtual string InfoString()
        // {
        //     var classNameStr = $"Class: { Name }";
        //     var critChanceStr = $"Base critical chance: { CritChance }%";
        //     var baseHpManaStr = $"Start hp: {BaseHealth}; start mana: { BaseMana }";
        //     var scalesStr = $"{ HpScale } hp per level; { ManaScale } mana per level";
        //     var regenStr = $"{ HpRegenRate } hp per regen tick; { ManaRegenRate } mana per regen tick";
        //     var proficientWeaponsStr = $"Is proficient with: { String.Join(", ", ProficientWeaponTypesList.Select(p => p.ToString())) }";
        //     var AbilitiesStr = $"Spell list:\n" + String.Join("\n", from s in Abilities.Values select s.ToString());
        //     // var DescriptionStr
        //     return String.Join("\n", new List<string> {
        //             classNameStr,
        //             critChanceStr,
        //             baseHpManaStr,
        //             scalesStr,
        //             regenStr,
        //             AbilitiesStr,
        //             proficientWeaponsStr
        //         });
        // }

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
