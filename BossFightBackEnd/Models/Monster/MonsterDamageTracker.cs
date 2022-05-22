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
        [JsonIgnore]
        public override string TableName { get; set; } = nameof(MonsterDamageTracker);
        [JsonIgnore]
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

        public IEnumerable<MonsterDamageTracker> FindAllForParent(MySqlConnection pConnection, int? id = null)
        {
            return _findAllForParent(id, pConnection).Cast<MonsterDamageTracker>();
        }

        public IEnumerable<MonsterDamageTracker> FindTop(uint pRowsToRetrieve, string pOrderByColumn, bool pOrderByDescending = true)
        {
            return _findTop(pRowsToRetrieve, pOrderByColumn, pOrderByDescending).Cast<MonsterDamageTracker>();
        }

        public MonsterDamageTracker FindOne(int? id = null)
        {
            return (MonsterDamageTracker)_findOne(id);
        }

        public override IEnumerable<PersistableBase> BuildObjectFromReader(MySqlDataReader reader, MySqlConnection pConnection)
        {
            var result = new List<PersistableBase>();

            while (!reader.IsClosed && reader.Read())
            {   
                var monsterDamageTracker = new MonsterDamageTracker();
                monsterDamageTracker.MonsterDamageTrackerId = reader.GetInt(nameof(MonsterDamageTrackerId));
                monsterDamageTracker.PlayerId = reader.GetInt(nameof(PlayerId));
                monsterDamageTracker.MonsterInstanceId = reader.GetInt(nameof(MonsterInstanceId));
                monsterDamageTracker.DamageReceivedFromPlayer = reader.GetInt(nameof(DamageReceivedFromPlayer));
                reader.Close();

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

            return TrimAdditionalSearchCriteriaText(additionalSearchCriteriaText, pStartWithAnd);
        }


        #endregion PersistableBase implementation
    }
}