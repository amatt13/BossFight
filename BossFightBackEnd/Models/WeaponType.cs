using System.Collections.Generic;
using System.Linq;
using BossFight.Extentions;
using MySqlConnector;
using Newtonsoft.Json;

namespace BossFight.Models
{
    public class WeaponType : PersistableBase<WeaponType>, IPersist<WeaponType>
    {
        [JsonIgnore]
        public override string TableName { get; set; } =  nameof(WeaponType);

        [JsonIgnore]
        public override string IdColumn { get; set; } = nameof(WeaponTypeId);

        [PersistProperty(true)]
        public int? WeaponTypeId { get; private set; }
        
        [PersistProperty]
        public string WeaponTypename { get; private set; }

        public WeaponType() { }

        public WeaponType(int pWeaponTypeId, string pWeaponTypename)
        {
            WeaponTypeId = pWeaponTypeId;
            WeaponTypename = pWeaponTypename;
        }

        public override IEnumerable<WeaponType> BuildObjectFromReader(MySqlConnector.MySqlDataReader reader, MySqlConnection pConnection)
        {
            var result = new List<WeaponType>();

            while (reader.Read())
            {   
                var weaponType = new WeaponType();
                weaponType.WeaponTypeId = reader.GetInt(nameof(WeaponTypeId));
                weaponType.WeaponTypename = reader.GetString(nameof(WeaponTypename));
                result.Add(weaponType);
            }

            return result;
        }
    }
}