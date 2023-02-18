using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using BossFight.Extentions;
using MySqlConnector;
using System.Text.Json.Serialization;

namespace BossFight.Models
{
    public class PlayerPlayerClass : PersistableBase<PlayerPlayerClass>, IPersist<PlayerPlayerClass>
    {
        [JsonIgnore]
        public override string TableName { get; set; } = nameof(PlayerPlayerClass);
        [JsonIgnore]
        public override string IdColumn { get; set; } = nameof(PlayerPlayerClassId);

        // Persisted on PlayerPlayerClass table
        [PersistProperty(true)]
        public int? PlayerPlayerClassId {get; set;}

        [PersistProperty]
        public int? PlayerId { get; set; }
        
        [JsonIgnore]
        [PersistProperty]
        public int PlayerClassId { get; set; }

        [PersistProperty]
        public int XP { get; set; }

        [PersistProperty]
        public int Level { get; set; }

        [PersistProperty]
        public bool? Active { get; set; }  // Indicates if this is the current Player-PlayerClass relation

        // From other tables
        [JsonIgnore]
        public Player Player { get; set; }

        public PlayerClass PlayerClass { get; set; }

        [JsonIgnore]
        public int AttackPowerBonus { get => PlayerClass.AttackPowerBonus; }

        [JsonIgnore]
        public int SpellPowerBonus { get => PlayerClass.SpellPowerBonus; }
        
        [JsonIgnore]
        public int CritChance { get => PlayerClass.CritChance; }

        // Not persisted
        [JsonIgnore]
        private int? _maxHp;
        public int MaxHp
        {
            get 
            {
                if (_maxHp == null)
                    _maxHp = PlayerClass.CalculateMaxHp(Level);
                return _maxHp.Value;
            }
            private set { _maxHp = value; }
        }

        [JsonIgnore]
        private int? _maxMana;
        public int MaxMana
        {
            get 
            {
                if (_maxMana == null)
                    _maxMana = PlayerClass.CalculateMaxMana(Level);
                return _maxMana.Value;
            }
            private set { _maxMana = value; }
        }

        public string PlayerClassName { get => PlayerClass.Name; }

        public int XpNeededToNextLevel { get => ExperienceCalculator.XpNeededToNextLevel(this); }

        public PlayerPlayerClass() { }

        public override IEnumerable<PlayerPlayerClass> BuildObjectFromReader(MySqlDataReader reader, MySqlConnection pConnection)
        {
            var result = new List<PlayerPlayerClass>();

            while (reader.Read())
            {
                var playerPlayerClass = new PlayerPlayerClass();
                playerPlayerClass.PlayerPlayerClassId = reader.GetInt(nameof(PlayerPlayerClassId));
                playerPlayerClass.PlayerId = reader.GetInt(nameof(PlayerId));
                playerPlayerClass.PlayerClassId = reader.GetInt(nameof(PlayerClassId));
                playerPlayerClass.XP = reader.GetInt(nameof(XP));
                playerPlayerClass.Level = reader.GetInt(nameof(Level));
                playerPlayerClass.Active = reader.GetBooleanNullable(nameof(Active));
                
                playerPlayerClass.PlayerClass = new PlayerClass().FindOne(playerPlayerClass.PlayerClassId);
                result.Add(playerPlayerClass);
            }

            return result;
        }

        public override string AdditionalSearchCriteria(PersistableBase<PlayerPlayerClass> pSearchObject, bool pStartWithAnd = true)
        {
            var ppc = pSearchObject as PlayerPlayerClass;
            var additionalSearchCriteriaText = String.Empty;
            if (ppc.Active.HasValue)
                additionalSearchCriteriaText += $" AND { nameof(Active) } = { (ppc.Active.Value ? "TRUE" : "FALSE") }\n";

            if (ppc.PlayerId.HasValue)
                additionalSearchCriteriaText += $" AND { nameof(PlayerId) } = { ppc.PlayerId.Value }\n";

            return TrimAdditionalSearchCriteriaText(additionalSearchCriteriaText, pStartWithAnd);
        }

        public override void BeforePersist()
        {
            base.BeforePersist();
            Active ??= false;
        }

        public void LevelUp()
        {
            Level += 1;
            XP = 0;
            MaxHp = PlayerClass.CalculateMaxHp(Level);
            MaxMana = PlayerClass.CalculateMaxMana(Level);
            Player.RestoreAllHealthAndMana();
        }
    }
}
