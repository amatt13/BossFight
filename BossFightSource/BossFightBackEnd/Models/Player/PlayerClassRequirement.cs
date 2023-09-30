using System.Collections.Generic;
using System.Linq;
using BossFight.Extentions;
using MySqlConnector;
using System.Text.Json.Serialization;

namespace BossFight.Models
{
    public class PlayerClassRequirement : PersistableBase<PlayerClassRequirement>
    {
        [JsonIgnore]
        public override string TableName { get; set; } = nameof(PlayerClassRequirement);
        [JsonIgnore]
        public override string IdColumn { get; set; } = nameof(PlayerClassId);

        // Persisted on PlayerClassRequirement table
        [PersistProperty]
        public int? PlayerClassId { get; set; }  // "owner"

        [PersistProperty]
        public int RequiredPlayerClassId { get; set; }  // Nedded class

        [PersistProperty]
        public int LevelRequirement {get; set;}

        // From other tables
        [JsonIgnore]
        public PlayerClass PlayerClass { get; set; }

        [JsonIgnore]
        public PlayerClass RequiredPlayerClass { get; set; }

        public string RequiredPlayerClassName { get => RequiredPlayerClass.Name; }

        public PlayerClassRequirement () { }


        #region PersistableBase implementation

        public override IEnumerable<PlayerClassRequirement> BuildObjectFromReader(MySqlDataReader reader, MySqlConnection pConnection)
        {
            var result = new List<PlayerClassRequirement>();

            while (reader.Read())
            {
                var playerClassRequirement = new PlayerClassRequirement
                {
                    PlayerClassId = reader.GetInt(nameof(PlayerClassId)),
                    RequiredPlayerClassId = reader.GetInt(nameof(RequiredPlayerClassId)),
                    LevelRequirement = reader.GetInt(nameof(LevelRequirement))
                };
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
