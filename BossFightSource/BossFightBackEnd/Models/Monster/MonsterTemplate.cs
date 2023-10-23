using System.Collections.Generic;
using System;
using System.Linq;
using BossFight.BossFightEnums;
using BossFight.Extentions;
using MySqlConnector;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using AngleSharp.Text;

namespace BossFight.Models
{
    public class MonsterTemplate : PersistableBase<MonsterTemplate>, IPersist<MonsterTemplate>
    {
        [JsonIgnore]
        public override string TableName { get; set; } = nameof(MonsterTemplate);
        [JsonIgnore]
        public override string IdColumn { get; set; } = nameof(MonsterTemplateId);

        [PersistProperty(true)]
        public int? MonsterTemplateId { get; set; }

        [PersistProperty]
        public int? Tier { get; set; }

        [PersistProperty]
        public string Name { get; set; }

        [PersistProperty]
        public bool? BossMonster { get; set; }

        // from other tables
        [JsonIgnore]
        public Dictionary<int, int> DamageTracker { get; set; }  // key is PlayerId, value is totaled damge from player

        [JsonIgnore]
        private List<MonsterType> _monsterTypeList {get; set;}
        public List<MonsterType> MonsterTypeList
        {
            get { return _monsterTypeList; }
            set
            {
                _monsterTypeList = value;
                MonsterTypeStringList = value.Select(evalue => EnumTextFormatter.EnumPrinter(evalue)).ToList<String>();
            }
        }

        public List<String> MonsterTypeStringList { get; private set; }

        // Calculated fields/properties
        [JsonIgnore]
        public bool SearchRandomTopOne { get; set; }


        public MonsterTemplate () { }

        public override IEnumerable<MonsterTemplate> BuildObjectFromReader(MySqlDataReader reader, MySqlConnection pConnection)
        {
            var result = new List<MonsterTemplate>();

            while (reader.Read())
            {
                var monsterTemplate = new MonsterTemplate
                {
                    MonsterTemplateId = reader.GetInt(nameof(MonsterTemplateId)),
                    Tier = reader.GetInt(nameof(Tier)),
                    Name = reader.GetString(nameof(Name)),
                    BossMonster = reader.GetBoolean(nameof(BossMonster))
                };
                var monsterTypes = reader.GetString("MonsterTypes");
                if (monsterTypes != null && monsterTypes != String.Empty)
                {
                    var types = monsterTypes.Split(",");
                    var enumTypes = types.Select(type => (MonsterType)type.ToInteger(-1)).ToList();
                    monsterTemplate.MonsterTypeList = enumTypes;
                }
                result.Add(monsterTemplate);
            }

            return result;
        }

        protected override string BuildSelectStatement()
        {
            var MonsterTemplateMonsterType = TableName + nameof(MonsterType);
            var monsterTypeId = nameof(MonsterType) + "Id";
            var select = $@"SELECT { TableName }.*, (
    SELECT GROUP_CONCAT(mt.{ monsterTypeId } SEPARATOR ',')
	FROM { MonsterTemplateMonsterType } mtmt
	JOIN { nameof(MonsterType) } mt
		ON mt.{ monsterTypeId } = mtmt.{ monsterTypeId }
	WHERE mtmt.{ IdColumn } = MonsterTemplate.{ IdColumn }
) as `MonsterTypes`
FROM `{ TableName }`
";
            return select;
        }



        public override string AdditionalSearchCriteria(PersistableBase<MonsterTemplate> pSearchObject, bool pStartWithAnd = true)
        {
            var mt = pSearchObject as MonsterTemplate;
            var additionalSearchCriteriaText = String.Empty;

            if (mt.BossMonster.HasValue)
                additionalSearchCriteriaText += $" AND { TableName }.{ nameof(BossMonster) } = { (mt.BossMonster.Value ? "TRUE" : "FALSE") }\n";

            if (mt.Tier.HasValue)
                additionalSearchCriteriaText += $" AND { TableName }.{ nameof(Tier) } = { mt.Tier.Value }\n";

            // must be last
            if (mt.SearchRandomTopOne)
                additionalSearchCriteriaText += "ORDER BY RAND()\nLIMIT 1";

            // this is so fucking janky. Move "ORDY BY RAND" to PersistableBase?
            return pStartWithAnd && !additionalSearchCriteriaText.StartsWith(" AND") ? additionalSearchCriteriaText : additionalSearchCriteriaText[4..];
        }

        public override string ToString()
        {
            return Name + "_template";
        }
    }
}
