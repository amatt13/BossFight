using System;
using System.Data;

namespace BossFight.Models
{
    public class PlayerPlayerClass : PersistableBase, IPersist<PlayerPlayerClass>
    {
        public override string TableName { get; set; } = "PlayerPlayerClass";

        // Persisted on PlayerPlayerClass table
        public int PlayerId { get; set; }
        public int PlayerClassId { get; set; }
        public int XP { get; set; }
        public int Level { get; set; }

        // From other tables
        public Player Player { get; set; }
        public PlayerClass PlayerClass { get; set; }

        // Not persisted
        public int MaxHp { get; set; }
        public int MaxMana { get; set; }
        public string PlayerClassName { get => PlayerClass.Name; }

        public PlayerPlayerClass() { }

        protected override PersistableBase _findOne(int id)
        {
            //using var cmd = Db.Connection.CreateCommand();
            using var connection = GlobalConnection.GetNewOpenConnection();
            using var cmd = connection.CreateCommand();
            
            cmd.CommandText = $@"SELECT * FROM `{ TableName }` WHERE `{ nameof(PlayerId) }` = @PlayerId AND `{ nameof(PlayerClassId) }` = @PlayerClassId";
            cmd.Parameters.Add(new MySqlParameter
            {
                ParameterName = "@id",
                DbType = DbType.String,
                Value = id.ToString(),
            });
            var result = BuildObjectFromReader(cmd.ExecuteReader());
            connection.Close();
            return result;
        }

        public void LevelUp()
        {
            Level += 1;
            XP = 0;
            MaxHp = PlayerClass.CalculateMaxHp(Level);
            MaxMana = PlayerClass.CalculateMaxMana(Level);
            Player.RestoreAllHealthAndMana();
        }
    }
}
