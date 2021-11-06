using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Data.Common;
using BossFight.Models.DB;
using MySqlConnector;
using System;

namespace BossFight.Models
{
    public class PersistableBase
    {
        public static string BackUpConnString;

        public virtual string TableName { get; set; }
        public virtual string IdColumn { get; set; }

        public PersistableBase()
        { }

        protected virtual PersistableBase _findOne(int id)
        {
            //using var cmd = Db.Connection.CreateCommand();
            using var connection = GlobalConnection.GetNewOpenConnection();
            using var cmd = connection.CreateCommand();
            
            cmd.CommandText = $@"SELECT * FROM `{ TableName }` WHERE `{ IdColumn }` = @id";
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

        public virtual PersistableBase BuildObjectFromReader(MySqlConnector.MySqlDataReader reader)
        {
            throw new NotImplementedException();
        }
    }
}