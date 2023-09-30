using System;
using MySqlConnector;
using BossFight.Models.DB;
using BossFight.Extentions;

namespace BossFight
{
    public static class GlobalConnection
    {
        public static AppDb AppDb { get; set; }


        private static string _connectionString;
        public static MySqlConnection GetNewOpenConnection()
        {
            var newCon = new MySqlConnection(_connectionString);
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
                if (readValue is not DBNull)
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

        public static void SetConnectionString(string pConnectionString)
        {
            if (_connectionString.HasText())
            {
                throw new Exception($"Tried to overwrite connection string! Existing value is '{_connectionString}' new value is '{pConnectionString}'");
            }
            else
            {
                _connectionString = pConnectionString;
            }
        }
    }
}
