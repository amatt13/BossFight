using System;
using System.Collections.Generic;
using System.Linq;
using BossFight.Extentions;
using MySqlConnector;
using Newtonsoft.Json;

namespace BossFight.Models
{
    public class MonsterDamageTracker : PersistableBase, IPersist<MonsterDamageTracker>
    {
        public override string TableName { get; set; } = nameof(MonsterDamageTracker);
        public override string IdColumn { get; set; } = nameof(MonsterDamageTrackerId);

        // Persisted on MonsterDamageTracker table
        [PersistProperty(true)]
        public int? MonsterDamageTrackerId { get; set; }
        
        [PersistProperty]
        public int PlayerId { get; set; }
                
        [PersistProperty]
        public int? MonsterInstanceId { get; set; }

        [PersistProperty]
        public int DamageReceivedFromPlayer { get; set; }

        // From other tables
        [JsonIgnore]
        public Player Player { get; set; }

        [JsonIgnore]
        public MonsterInstance MonsterInstance { get; set; }

        public MonsterDamageTracker () { }

        public MonsterDamageTracker(Player pPlayer, MonsterInstance pMonsterInstance, int pDamageReceivedFromPlayer)
        {
            Player = pPlayer;
            PlayerId = pPlayer.PlayerId.Value;
            MonsterInstance = pMonsterInstance;
            MonsterInstanceId = pMonsterInstance.MonsterInstanceId;
            DamageReceivedFromPlayer = pDamageReceivedFromPlayer;
        }

        #region PersistableBase implementation

        public IEnumerable<MonsterDamageTracker> FindAll(int? id = null)
        {
            return _findAll(id).Cast<MonsterDamageTracker>();
        }

        public MonsterDamageTracker FindOne(int id)
        {
            return (MonsterDamageTracker)_findOne(id);
        }

        public override IEnumerable<PersistableBase> BuildObjectFromReader(MySqlDataReader reader)
        {
            var result = new List<PersistableBase>();

            while (reader.Read())
            {   
                var monsterDamageTracker = new MonsterDamageTracker();
                monsterDamageTracker.MonsterDamageTrackerId = reader.GetInt(nameof(MonsterDamageTrackerId));
                monsterDamageTracker.PlayerId = reader.GetInt(nameof(PlayerId));
                monsterDamageTracker.MonsterInstanceId = reader.GetInt(nameof(MonsterInstanceId));

                monsterDamageTracker.Player = new Player().FindOne(monsterDamageTracker.PlayerId);
                result.Add(monsterDamageTracker);
            }

            return result;
        }

        public override string AdditionalSearchCriteria(PersistableBase pSearchObject, bool pStartWithAnd = true)
        {
            var mdt = pSearchObject as MonsterDamageTracker;
            var additionalSearchCriteriaText = String.Empty;
            if (mdt.MonsterInstanceId.HasValue)
                additionalSearchCriteriaText += $" AND { nameof(MonsterDamageTracker.MonsterInstanceId) } = { mdt.MonsterInstanceId.Value }";

            return pStartWithAnd ? additionalSearchCriteriaText : additionalSearchCriteriaText.Substring(4, additionalSearchCriteriaText.Length- 4);
        }


        #endregion PersistableBase implementation
    }
}