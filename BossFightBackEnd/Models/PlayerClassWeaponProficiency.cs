using System.Collections.Generic;
using System.Linq;
using BossFight.Extentions;
using MySqlConnector;
using Newtonsoft.Json;

namespace BossFight.Models
{
    public class PlayerClassWeaponProficiency : PersistableBase, IPersist<PlayerClassWeaponProficiency>
    {
        [JsonIgnore]
        public override string TableName { get; set; } = nameof(PlayerClassWeaponProficiency);
        [JsonIgnore]
        public override string IdColumn { get; set; } = nameof(PlayerClassId);

        // Persisted on PlayerClassWeaponProficiency table
        [PersistPropertyAttribute]
        public int? PlayerClassId { get; set; }

        [PersistPropertyAttribute]
        public int WeaponTypeId { get; set; }

        // From other tables
        [JsonIgnore]
        public PlayerClass PlayerClass { get; set; }
        public WeaponType WeaponType { get; set; }

        public PlayerClassWeaponProficiency () { }


        #region PersistableBase implementation

        public IEnumerable<PlayerClassWeaponProficiency> FindAll(int? id = null)
        {
            return _findAll(id).Cast<PlayerClassWeaponProficiency>();
        }

        public PlayerClassWeaponProficiency FindOne(int id)
        {
            return (PlayerClassWeaponProficiency)_findOne(id);
        }

        public override IEnumerable<PersistableBase> BuildObjectFromReader(MySqlDataReader reader, MySqlConnection pConnection)
        {
            var result = new List<PersistableBase>();

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
