using System;
using MySqlConnector;
using BossFight.Models.DB;

namespace BossFight
{
    public static class GlobalConnection
    {
        public static AppDb AppDb { get; set; }


        public static string ConnString;
        public static MySqlConnection GetNewOpenConnection()
        {
            var newCon = new MySqlConnection(ConnString);
            newCon.Open();

            return newCon;
        }

        public static T SingleValue<T>(string pSql)
        {
            using var connection = GetNewOpenConnection();
            using var cmd = connection.CreateCommand();

            cmd.CommandText = pSql;            
            var reader = cmd.ExecuteReader();
            if (reader.FieldCount > 1) 
            {
                var errorMessage = $"Too many columns in SingleValue (found reader.FieldCount).";
                Console.WriteLine($"{errorMessage }{ Environment.NewLine }{ cmd.CommandText }");
                throw new Exception(errorMessage);
            }
            var result = default(T);
            if (reader.Read())
            {
                //var columnName = reader.GetSchemaTable().Columns[0].ColumnName;
                var readValue = reader.GetValue(0);
                if (!(readValue is DBNull))
                {                    
                    result = (T)readValue;
                }
            }
            reader.Close();
            connection.Close();

            return result;
        }

        public static T SingleValue<T>(MySqlCommand pSqlCommand)
        {
            using var connection = GetNewOpenConnection();           
            pSqlCommand.Connection = connection;
            var reader = pSqlCommand.ExecuteReader();
            if (reader.FieldCount > 1) 
            {
                var errorMessage = $"Too many columns in SingleValue (found reader.FieldCount).";
                Console.WriteLine($"{errorMessage }{ Environment.NewLine }{ pSqlCommand.CommandText }");
                throw new Exception(errorMessage);
            }
            var result = default(T);
            if (reader.Read())
            {   
                if (typeof(T) == typeof(bool))
                {
                    result = (T)(object)reader.GetBoolean(0);
                }
                else
                {
                    result = (T)reader.GetValue(0);
                }
            }
            
            reader.Close();
            connection.Close();

            return result;
        }
    }
}
