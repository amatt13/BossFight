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
    }
}
