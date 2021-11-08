using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace BossFight.Models
{
    public class WeaponType : PersistableBase, IPersist<WeaponType>
    {
        [JsonIgnore]
        public override string TableName { get; set; } =  "WeaponType";
        [JsonIgnore]
        public override string IdColumn { get; set; } = nameof(WeaponTypeId);

        public int WeaponTypeId { get; private set; }
        public string WeaponTypename { get; private set; }

        public WeaponType() { }

        public WeaponType(int pWeaponTypeId, string pWeaponTypename)
        {
            WeaponTypeId = pWeaponTypeId;
            WeaponTypename = pWeaponTypename;
        }

        public override IEnumerable<PersistableBase> BuildObjectFromReader(MySqlConnector.MySqlDataReader reader)
        {
            var result = new List<PersistableBase>();

            while (reader.Read())
            {   
                var weaponType = new WeaponType();
                weaponType.WeaponTypeId = reader.GetInt(nameof(WeaponTypeId));
                weaponType.WeaponTypename = reader.GetString(nameof(WeaponTypename));
                result.Add(weaponType);
            }

            return result;
        }

        public WeaponType FindOne(int id)
        {
            return (WeaponType)_findOne(id);
        }

        public IEnumerable<WeaponType> FindAll(int? id = null)
        {
            return _findAll(id).Cast<WeaponType>();
        }
    }
}