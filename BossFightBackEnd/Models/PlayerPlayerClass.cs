using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using MySqlConnector;
using Newtonsoft.Json;

namespace BossFight.Models
{
    public class PlayerPlayerClass : PersistableBase, IPersist<PlayerPlayerClass>
    {
        [JsonIgnore]
        public override string TableName { get; set; } = "PlayerPlayerClass";
        [JsonIgnore]
        public override string IdColumn { get; set; } = nameof(PlayerId);

        // Persisted on PlayerPlayerClass table
        [JsonIgnore]
        public int PlayerId { get; set; }
        [JsonIgnore]
        public int PlayerClassId { get; set; }
        public int XP { get; set; }
        public int Level { get; set; }
        public bool? Active { get; set; }  // Indicates if this is the current Player-PlayerClass relation

        // From other tables
        [JsonIgnore]
        public Player Player { get; set; }
        [JsonIgnore]
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

        public PlayerPlayerClass() { }

        public PlayerPlayerClass FindOne(int id)
        {
            return (PlayerPlayerClass)_findOne(id);
        }

        public IEnumerable<PlayerPlayerClass> FindAll(int? id = null)
        {
            return _findAll(id).Cast<PlayerPlayerClass>();
        }

        public override IEnumerable<PersistableBase> BuildObjectFromReader(MySqlDataReader reader)
        {
            var result = new List<PersistableBase>();

            while (reader.Read())
            {
                var playerPlayerClass = new PlayerPlayerClass();
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

        public override string AdditionalSearchCriteria(PersistableBase pSearchObject, bool pStartWithAnd = true)
        {
            var ppc = pSearchObject as PlayerPlayerClass;
            var additionalSearchCriteriaText = String.Empty;
            if (ppc.Active.HasValue)
                additionalSearchCriteriaText += $" AND Active = { (ppc.Active.Value ? "TRUE" : "FALSE") }\n";

            return pStartWithAnd ? additionalSearchCriteriaText : additionalSearchCriteriaText.Substring(4, additionalSearchCriteriaText.Length- 4);
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
