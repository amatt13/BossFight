using System.Collections.Generic;
using System.Linq;
using BossFight.Extentions;
using MySqlConnector;
using System.Text.Json.Serialization;

namespace BossFight.Models
{
    public class PlayerClassWeaponProficiency : PersistableBase<PlayerClassWeaponProficiency>, IPersist<PlayerClassWeaponProficiency>
    {
        [JsonIgnore]
        public override string TableName { get; set; } = nameof(PlayerClassWeaponProficiency);
        [JsonIgnore]
        public override string IdColumn { get; set; } = nameof(PlayerClassId);

        // Persisted on PlayerClassWeaponProficiency table
        [PersistProperty]
        public int? PlayerClassId { get; set; }

        [PersistProperty]
        public int WeaponTypeId { get; set; }

        // From other tables
        [JsonIgnore]
        public PlayerClass PlayerClass { get; set; }
        public WeaponType WeaponType { get; set; }

        public PlayerClassWeaponProficiency () { }


        #region PersistableBase implementation

        public override IEnumerable<PlayerClassWeaponProficiency> BuildObjectFromReader(MySqlDataReader reader, MySqlConnection pConnection)
        {
            var result = new List<PlayerClassWeaponProficiency>();

            while (reader.Read())
            {   
                var playerClassWeaponProficiency = new PlayerClassWeaponProficiency();
                playerClassWeaponProficiency.PlayerClassId = reader.GetInt(nameof(PlayerClassId));
                playerClassWeaponProficiency.WeaponTypeId = reader.GetInt(nameof(WeaponTypeId));
                playerClassWeaponProficiency.WeaponType = new WeaponType().FindOne(playerClassWeaponProficiency.WeaponTypeId);
                result.Add(playerClassWeaponProficiency);
            }

            return result;
        }

        #endregion PersistableBase implementation
    }
}
