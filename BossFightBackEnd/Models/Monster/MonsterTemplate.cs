using System.Collections.Generic;
using System;
using System.Linq;
using BossFight.BossFightEnums;
using BossFight.Extentions;
using MySqlConnector;

namespace BossFight.Models
{
    public class MonsterTemplate : PersistableBase, IPersist<MonsterTemplate>
    {
        public override string TableName { get; set; } = nameof(MonsterTemplate);
        public override string IdColumn { get; set; } = nameof(MonsterTemplateId);
        
        [PersistProperty(true)]
        public int? MonsterTemplateId { get; set; }

        [PersistProperty]
        public int Tier { get; set; }

        [PersistProperty]
        public string Name { get; set; }

        [PersistProperty]
        public bool BossMonster { get; set; }

        // from other tables
        public Dictionary<int, int> DamageTracker { get; set; }  // key is PlayerId, value is totaled damge from player
        public List<MonsterType> MonsterTypeList { get; set; }

        // Calculated fields/properties



        public MonsterTemplate () { }

        public MonsterTemplate FindOne(int id)
        {
            return (MonsterTemplate)_findOne(id);
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
    }
}