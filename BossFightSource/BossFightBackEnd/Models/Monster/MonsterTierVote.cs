using System;
using System.Collections.Generic;
using System.Linq;
using BossFight.BossFightEnums;
using BossFight.Extentions;
using MySqlConnector;
using System.Text.Json.Serialization;

namespace BossFight.Models
{
    public class MonsterTierVote : PersistableBase<MonsterTierVote>, IPersist<MonsterTierVote>
    {
        [JsonIgnore]
        public override string TableName { get; set; } = nameof(MonsterTierVote);
        [JsonIgnore]
        public override string IdColumn { get; set; } = nameof(MonsterTierVoteId);

        // Persisted on MonsterTierVote table
        [PersistProperty(true)]
        public int? MonsterTierVoteId { get; set; }

        [PersistProperty]
        public MonsterTierVoteChoice? Vote { get; set; }

        [PersistProperty]
        public int? PlayerId
        {
            get { return Player?.PlayerId; }
            set
            {
                Player ??= new Player();
                Player.PlayerId = value;
            }
        }

        [PersistProperty]
        public int? MonsterInstanceId
        {
            get { return MonsterInstance?.MonsterInstanceId; }
            set
            {
                MonsterInstance ??= new MonsterInstance();
                MonsterInstance.MonsterInstanceId = value;
            }
        }

        // From other tables
        [JsonIgnore]
        public Player Player { get; set; }

        [JsonIgnore]
        public MonsterInstance MonsterInstance { get; set; }

        public MonsterTierVote () { }

        #region PersistableBase implementation

        public override IEnumerable<MonsterTierVote> BuildObjectFromReader(MySqlDataReader reader, MySqlConnection pConnection)
        {
            var result = new List<MonsterTierVote>();

            while (reader.Read())
            {
                var MonsterTierVote = new MonsterTierVote
                {
                    MonsterTierVoteId = reader.GetInt(nameof(MonsterTierVoteId)),
                    PlayerId = reader.GetInt(nameof(PlayerId)),
                    MonsterInstanceId = reader.GetInt(nameof(MonsterInstanceId)),
                    Vote = (MonsterTierVoteChoice)reader.GetInt(nameof(Vote))
                };
                result.Add(MonsterTierVote);
            }
            reader.Close();

            foreach (MonsterTierVote monsterTierVote in result)
            {
                monsterTierVote.Player = new Player().FindOneForParent(monsterTierVote.PlayerId.Value, pConnection);
                monsterTierVote.MonsterInstance = new MonsterInstance().FindOneForParent(MonsterInstance.MonsterInstanceId.Value, pConnection);
            }

            return result;
        }

        public override string AdditionalSearchCriteria(PersistableBase<MonsterTierVote> pSearchObject, bool pStartWithAnd = true)
        {
            var mtv = pSearchObject as MonsterTierVote;
            var additionalSearchCriteriaText = String.Empty;
            if (mtv.PlayerId.HasValue)
                additionalSearchCriteriaText += $" AND { nameof(PlayerId) } = { mtv.PlayerId.ToDbString() }\n";

            if (mtv.MonsterInstanceId.HasValue)
                additionalSearchCriteriaText += $" AND { nameof(MonsterInstanceId) } = { mtv.MonsterInstanceId.ToDbString() }\n";

            if (mtv.Vote.HasValue)
                additionalSearchCriteriaText += $" AND { nameof(Vote) } = { mtv.Vote.ToDbString() }\n";

            return TrimAdditionalSearchCriteriaText(additionalSearchCriteriaText, pStartWithAnd);
        }

        #endregion PersistableBase implementation
    }
}
