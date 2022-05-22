using System.Collections.Generic;
using System;
using System.Linq;
using BossFight.BossFightEnums;
using BossFight.Extentions;
using MySqlConnector;
using System.Text.Json.Serialization;

namespace BossFight.Models
{
    public class MonsterTemplate : PersistableBase, IPersist<MonsterTemplate>
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
        public Dictionary<int, int> DamageTracker { get; set; }  // key is PlayerId, value is totaled damge from player
        public List<MonsterType> MonsterTypeList { get; set; }

        // Calculated fields/properties
        [JsonIgnore]
        public bool SearchRandomTopOne { get; set; }


        public MonsterTemplate () { }

        public MonsterTemplate FindOne(int? id = null)
        {
            return (MonsterTemplate)_findOne(id);
        }

        public IEnumerable<MonsterTemplate> FindTop(uint pRowsToRetrieve, string pOrderByColumn, bool pOrderByDescending = true)
        {
            return _findTop(pRowsToRetrieve, pOrderByColumn, pOrderByDescending).Cast<MonsterTemplate>();
        }

        public MonsterTemplate FindOneForParent(int id, MySqlConnection pConnection)
        {
            return (MonsterTemplate)_findOneForParent(id, pConnection);
        }

        public IEnumerable<MonsterTemplate> FindAll(int? id)
        {
            return _findAll(id).Cast<MonsterTemplate>();
        }

        public override IEnumerable<PersistableBase> BuildObjectFromReader(MySqlDataReader reader, MySqlConnection pConnection)
        {
            var result = new List<PersistableBase>();

            while (reader.Read())
            {   
                var monsterTemplate = new MonsterTemplate();
                monsterTemplate.MonsterTemplateId = reader.GetInt(nameof(MonsterTemplateId));
                monsterTemplate.Tier = reader.GetInt(nameof(Tier));
                monsterTemplate.Name = reader.GetString(nameof(Name));
                monsterTemplate.BossMonster = reader.GetBoolean(nameof(BossMonster));
                result.Add(monsterTemplate);
            }

            return result;
        }

        public override string AdditionalSearchCriteria(PersistableBase pSearchObject, bool pStartWithAnd = true)
        {
            var mt = pSearchObject as MonsterTemplate;
            var additionalSearchCriteriaText = String.Empty;

            if (mt.BossMonster.HasValue)
                additionalSearchCriteriaText += $" AND { nameof(BossMonster) } = { (mt.BossMonster.Value ? "TRUE" : "FALSE") }\n";

            if (mt.Tier.HasValue)
                additionalSearchCriteriaText += $" AND { nameof(Tier) } = { mt.Tier.Value }\n";

            // must be last
            if (mt.SearchRandomTopOne)
                additionalSearchCriteriaText += "ORDER BY RAND()\nLIMIT 1";

            // this is so fucking janky. Move "ORDY BY RAND" to PersistableBase?
            return pStartWithAnd && !additionalSearchCriteriaText.StartsWith(" AND") ? additionalSearchCriteriaText : additionalSearchCriteriaText.Substring(4, additionalSearchCriteriaText.Length - 4);
        }
    }
}