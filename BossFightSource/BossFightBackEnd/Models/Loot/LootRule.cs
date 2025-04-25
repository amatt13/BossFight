using System.Collections.Generic;
using System.Text.Json.Serialization;
using MySqlConnector;
using BossFight.Extentions;
using BossFight.BossFightEnums;

namespace BossFight.Models
{
    public class LootRule : PersistableBase<LootRule>, IPersist<LootRule>
    {
        [JsonIgnore]
        public override string TableName { get; set; } = nameof(LootRule);
        [JsonIgnore]
        public override string IdColumn { get; set; } = nameof(LootRuleId);

        // Persisted on LootRule table
        [PersistProperty(true)]
        public int? LootRuleId { get; set; }

        [PersistProperty]
        public int? MonsterTemplateId { get; set; }

        [PersistProperty]
        public int? MonsterTypeId { get; set; }

        [PersistProperty]
        public LootType LootType { get; set; }

        [PersistProperty]
        public int LootId { get; set; }

        [PersistProperty]
        public int? MinLevel { get; set; }

        [PersistProperty]
        public int? MaxLevel { get; set; }

        [PersistProperty]
        public bool? BossOnly { get; set; }

        [PersistProperty]
        public float DropChance { get; set; }

        [PersistProperty]
        public int? MinQuantity { get; set; }

        [PersistProperty]
        public int? MaxQuantity { get; set; }

        public LootRule () { }

        #region PersistableBase implementation

        public override IEnumerable<LootRule> BuildObjectFromReader(MySqlDataReader reader, MySqlConnection pConnection)
        {
            var result = new List<LootRule>();

            while (reader.Read())
            {
                var LootRule = new LootRule
                {
                    LootRuleId = reader.GetInt(nameof(LootRuleId)),
                    MonsterTemplateId = reader.GetIntNullable(nameof(MonsterTemplateId)),
                    MonsterTypeId = reader.GetIntNullable(nameof(MonsterTypeId)),
                    LootType = reader.GetEnum<LootType>(nameof(LootType)),
                    LootId = reader.GetInt(nameof(LootId)),
                    MinLevel = reader.GetIntNullable(nameof(MinLevel)),
                    MaxLevel = reader.GetIntNullable(nameof(MaxLevel)),
                    BossOnly = reader.GetBooleanNullable(nameof(BossOnly)),
                    DropChance = reader.GetFloat(nameof(DropChance)),
                    MinQuantity = reader.GetInt(nameof(MinQuantity)),
                    MaxQuantity = reader.GetInt(nameof(MaxQuantity)),
                };
                result.Add(LootRule);
            }
            reader.Close();

            return result;
        }

        #endregion PersistableBase implementation
    }
}
