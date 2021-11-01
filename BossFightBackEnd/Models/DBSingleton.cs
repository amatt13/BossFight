using System;
using MySqlConnector;
using BossFight.Models.DB;

namespace BossFight.Models
{
    public class DBSingleton
    {
        private static DBSingleton _instance { get; set; } = null;
        private static DBConnector _connector { get; set; }

        private DBSingleton(string pConnectionString)
        { }

        public static void Init(string pConnectionString)
        {
            _instance = new DBSingleton(pConnectionString);
            _connector = new DBConnector(pConnectionString);
        }

        public static DBSingleton GetInstance()
        {
            return _instance;
        }

        public MySqlDataReader ExecuteQuery(string pQuery)
        {
            var command = new MySqlCommand("pQuery", _connector.Connection);
            return command.ExecuteReader();
        }

        public void CloseConnection()
        {
            _connector.Connection.Close();
        }

        public string TEST(string pQuery = "select 'I am a test';")
        {
            var command = new MySqlCommand(pQuery, _connector.Connection);
            var reader = command.ExecuteReader();
            string text = "";
            while (reader.Read())
            {
                text = reader.GetString(0);
                Console.WriteLine("DBSingleton.TEST: " + text);
            }
            return text;
        }

        // private DBSingleton()
        // {
        //     using (var )
        //     {
        //         connection.Open();

        //         using ()
        //         using (var reader = )
        //             while (reader.Read())
        //                 Console.WriteLine(reader.GetString(0));
        //     }
        // }
    }
}
