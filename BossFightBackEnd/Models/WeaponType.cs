namespace BossFight.Models
{
    public class WeaponType : PersistableBase, IPersist<WeaponType>
    {
        public int WeaponTypeId { get; private set; }
        public string WeaponTypename { get; private set; }
        public override string TableName { get; set; } =  "WeaponType";
        public override string IdColumn { get; set; } = nameof(WeaponTypeId);

        public WeaponType() { }

        public WeaponType(int pWeaponTypeId, string pWeaponTypename)
        {
            WeaponTypeId = pWeaponTypeId;
            WeaponTypename = pWeaponTypename;
        }

        public override PersistableBase BuildObjectFromReader(MySqlConnector.MySqlDataReader reader)
        {
            var weaponType = new WeaponType();

            while (reader.Read())
            {   
                weaponType.WeaponTypeId = reader.GetInt(nameof(WeaponTypeId));
                weaponType.WeaponTypename = reader.GetString(nameof(WeaponTypename));
            }

            return weaponType;
        }

        public WeaponType FindOne(int id)
        {
            return (WeaponType)_findOne(id);
        }
    }
}