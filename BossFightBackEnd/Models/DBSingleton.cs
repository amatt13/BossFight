using System;
using MySqlConnector;
using BossFight.Models.DB;
using System.Threading.Tasks;

namespace BossFight.Models
{
    // public class DBSingleton
    // {
    //     private static DBSingleton _instance { get; set; } = null;
    //     private static string _dbConnectionString {get; set; }

    //     private DBSingleton(string pConnectionString)
    //     { 
    //         _dbConnectionString = pConnectionString;
    //     }

    //     public static void Init(string pConnectionString)
    //     {
    //         _instance = new DBSingleton(pConnectionString);
    //     }

    //     public static DBSingleton GetInstance()
    //     {
    //         return _instance;
    //     }

    //     public async Task<MySqlDataReader> ExecuteQuery(string pQuery)
    //     {
    //         using (MySqlConnection connection = new MySqlConnection(_dbConnectionString))
    //         {
    //             connection.Open();
    //             var command = new MySqlCommand(pQuery, connection);
    //             return await command.ExecuteReaderAsync();
    //         }  
    //     }
    // }
}
