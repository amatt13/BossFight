using System;
using System.Collections.Generic;
using System.Linq;
using BossFight.Extentions;
using MySqlConnector;
using System.Text.Json.Serialization;

namespace BossFight.Models
{
    public class MonsterDamageTracker : PersistableBase<MonsterDamageTracker>, IPersist<MonsterDamageTracker>
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

        public override IEnumerable<MonsterDamageTracker> BuildObjectFromReader(MySqlDataReader reader, MySqlConnection pConnection)
        {
            var result = new List<MonsterDamageTracker>();

            while (reader.Read())
            {
                var monsterDamageTracker = new MonsterDamageTracker
                {
                    MonsterDamageTrackerId = reader.GetInt(nameof(MonsterDamageTrackerId)),
                    PlayerId = reader.GetInt(nameof(PlayerId)),
                    MonsterInstanceId = reader.GetInt(nameof(MonsterInstanceId)),
                    DamageReceivedFromPlayer = reader.GetInt(nameof(DamageReceivedFromPlayer))
                };

                monsterDamageTracker.Player = new Player().FindOne(monsterDamageTracker.PlayerId);
                result.Add(monsterDamageTracker);
            }

            return result;
        }

        public override string AdditionalSearchCriteria(PersistableBase<MonsterDamageTracker> pSearchObject, bool pStartWithAnd = true)
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
