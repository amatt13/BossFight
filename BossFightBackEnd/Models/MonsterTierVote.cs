using System;
using System.Collections.Generic;
using System.Linq;
using BossFight.Extentions;
using MySqlConnector;
using Newtonsoft.Json;

namespace BossFight.Models.DB
{
    public class MonsterTierVote : PersistableBase, IPersist<MonsterTierVote>
    {
        [JsonIgnore]
        public override string TableName { get; set; } = nameof(MonsterTierVote);
        [JsonIgnore]
        public override string IdColumn { get; set; } = nameof(MonsterTierVoteId);

        // Persisted on MonsterTierVote table
        [PersistPropertyAttribute(true)]
        public int? MonsterTierVoteId { get; set; }

        [PersistPropertyAttribute]
        public int? PlayerId 
        { 
            get { return Player?.PlayerId; }
            set 
            { 
                if (Player == null) 
                    Player = new Player();
                Player.PlayerId = value; 
            }
        }

        // From other tables
        [JsonIgnore]
        public Player Player { get; set; }

        public MonsterTierVote () { }

        #region PersistableBase implementation

        public IEnumerable<MonsterTierVote> FindAll(int? id = null)
        {
            return _findAll(id).Cast<MonsterTierVote>();
        }

        public IEnumerable<MonsterTierVote> FindTop(uint pRowsToRetrieve, string pOrderByColumn, bool pOrderByDescending = true)
        {
            return _findTop(pRowsToRetrieve, pOrderByColumn, pOrderByDescending).Cast<MonsterTierVote>();
        }

        public MonsterTierVote FindOne(int? id = null)
        {
            return (MonsterTierVote)_findOne(id);
        }

        public override IEnumerable<PersistableBase> BuildObjectFromReader(MySqlDataReader reader, MySqlConnection pConnection)
        {
            var result = new List<PersistableBase>();

            while (reader.Read())
            {   
                var MonsterTierVote = new MonsterTierVote();
                MonsterTierVote.MonsterTierVoteId = reader.GetInt(nameof(MonsterTierVoteId));
                result.Add(MonsterTierVote);
            }

            return result;
        }

        #endregion PersistableBase implementation
    }
}