using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using BossFight.Extentions;
using MySqlConnector;
using Newtonsoft.Json;

namespace BossFight.Models
{
    public class PlayerPlayerClass : PersistableBase, IPersist<PlayerPlayerClass>
    {
        [JsonIgnore]
        public override string TableName { get; set; } = nameof(PlayerPlayerClass);
        [JsonIgnore]
        public override string IdColumn { get; set; } = nameof(PlayerClassId);

        // Persisted on PlayerPlayerClass table
        [PersistPropertyAttribute(true)]
        public int? PlayerId { get; set; }
        
        [JsonIgnore]
        [PersistPropertyAttribute]
        public int PlayerClassId { get; set; }

        [PersistPropertyAttribute]
        public int XP { get; set; }

        [PersistPropertyAttribute]
        public int Level { get; set; }

        [PersistPropertyAttribute]
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

        public int XpNeededToNextLevel { get => GenralHelperFunctions.XpNeededToNextLevel(this); }

        public PlayerPlayerClass() { }

        public PlayerPlayerClass FindOne(int? id = null)
        {
            return (PlayerPlayerClass)_findOne(id);
        }

        public IEnumerable<PlayerPlayerClass> FindTop(uint pRowsToRetrieve, string pOrderByColumn, bool pOrderByDescending = true)
        {
            return _findTop(pRowsToRetrieve, pOrderByColumn, pOrderByDescending).Cast<PlayerPlayerClass>();
        }

        public IEnumerable<PlayerPlayerClass> FindAll(int? id = null)
        {
            return _findAll(id).Cast<PlayerPlayerClass>();
        }

        public IEnumerable<PlayerPlayerClass> FindAllForParent(MySqlConnection pConnection, int? id = null)
        {
            return _findAllForParent(id, pConnection).Cast<PlayerPlayerClass>();
        }

        public override IEnumerable<PersistableBase> BuildObjectFromReader(MySqlDataReader reader, MySqlConnection pConnection)
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
                additionalSearchCriteriaText += $" AND { nameof(Active) } = { (ppc.Active.Value ? "TRUE" : "FALSE") }\n";

            if (ppc.PlayerId.HasValue)
                additionalSearchCriteriaText += $" AND { nameof(PlayerId) } = { ppc.PlayerId.Value }\n";

            return TrimAdditionalSearchCriteriaText(additionalSearchCriteriaText, pStartWithAnd);
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
