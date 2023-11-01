using System;
using System.Collections.Generic;
using BossFight.Extentions;
using MySqlConnector;
using System.Text.Json.Serialization;
using BossFight.BossFightEnums;

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
        public PlayerClassEnum? PlayerClassId { get; set; }

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
                _maxHp ??= PlayerClass.CalculateMaxHp(Level);
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
                _maxMana ??= PlayerClass.CalculateMaxMana(Level);
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
                var playerPlayerClass = new PlayerPlayerClass
                {
                    PlayerPlayerClassId = reader.GetInt(nameof(PlayerPlayerClassId)),
                    PlayerId = reader.GetInt(nameof(PlayerId)),
                    PlayerClassId = (PlayerClassEnum)reader.GetInt(nameof(PlayerClassId)),
                    XP = reader.GetInt(nameof(XP)),
                    Level = reader.GetInt(nameof(Level)),
                    Active = reader.GetBooleanNullable(nameof(Active))
                };

                playerPlayerClass.PlayerClass = PlayerClassFactory.CreatePlayerClass(playerPlayerClass);
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

            if (ppc.PlayerClassId.HasValue)
                additionalSearchCriteriaText += $" AND { nameof(PlayerClassId) } = { (int)ppc.PlayerClassId.Value }\n";

            return TrimAdditionalSearchCriteriaText(additionalSearchCriteriaText, pStartWithAnd);
        }

        public override void BeforePersist()
        {
            base.BeforePersist();
            Active ??= false;
            Player.UpdateBossFightConnectionWithPlayer();
        }

        public void LevelUp()
        {
            Level += 1;
            XP = 0;
            MaxHp = PlayerClass.CalculateMaxHp(Level);
            MaxMana = PlayerClass.CalculateMaxMana(Level);
            Player.RestoreAllHealthAndMana();
            PlayerClass.RecalculateUnlockedAbilities(Level);
        }
    }
}
