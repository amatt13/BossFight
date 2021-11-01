using System;
using MySqlConnector;

namespace BossFight.Models.DB
{
    public class DBConnector : IDisposable
    {
        public MySqlConnection Connection { get; }

        public DBConnector(string connectionString)
        {
            Connection = new MySqlConnection(connectionString);
            Connection.Open();
        }

        public void Dispose() => Connection.Dispose();
    }
}