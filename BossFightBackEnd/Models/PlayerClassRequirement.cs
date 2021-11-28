using System.Collections.Generic;
using System.Linq;
using BossFight.Extentions;
using MySqlConnector;
using Newtonsoft.Json;

namespace BossFight.Models
{
    public class PlayerClassRequirement : PersistableBase, IPersist<PlayerClassRequirement>
    {
        [JsonIgnore]
        public override string TableName { get; set; } = nameof(PlayerClassRequirement);
        [JsonIgnore]
        public override string IdColumn { get; set; } = nameof(PlayerClassId);

        // Persisted on PlayerClassRequirement table
        [PersistPropertyAttribute]
        public int? PlayerClassId { get; set; }  // "owner"

        [PersistPropertyAttribute]
        public int RequiredPlayerClassId { get; set; }  // Nedded class

        [PersistPropertyAttribute]
        public int LevelRequirement {get; set;}

        // From other tables
        [JsonIgnore]
        public PlayerClass PlayerClass { get; set; }

        [JsonIgnore]
        public PlayerClass RequiredPlayerClass { get; set; }
        
        public string RequiredPlayerClassName { get => RequiredPlayerClass.Name; }

        public PlayerClassRequirement () { }


        #region PersistableBase implementation

        public IEnumerable<PlayerClassRequirement> FindAll(int? id = null)
        {
            return _findAll(id).Cast<PlayerClassRequirement>();
        }

        public PlayerClassRequirement FindOne(int? id = null)
        {
            return (PlayerClassRequirement)_findOne(id);
        }

        public override IEnumerable<PersistableBase> BuildObjectFromReader(MySqlDataReader reader, MySqlConnection pConnection)
        {
            var result = new List<PersistableBase>();

            while (reader.Read())
            {   
                var playerClassRequirement = new PlayerClassRequirement();
                playerClassRequirement.PlayerClassId = reader.GetInt(nameof(PlayerClassId));
                playerClassRequirement.RequiredPlayerClassId = reader.GetInt(nameof(RequiredPlayerClassId));
                playerClassRequirement.LevelRequirement = reader.GetInt(nameof(LevelRequirement));
                playerClassRequirement.RequiredPlayerClass = new PlayerClass().FindOne(playerClassRequirement.RequiredPlayerClassId);
                result.Add(playerClassRequirement);
            }

            return result;
        }

        #endregion PersistableBase implementation

        public override string ToString()
        {
            return $"{RequiredPlayerClassName}:{RequiredPlayerClassName} - lvl {LevelRequirement}";
        }
    }
}