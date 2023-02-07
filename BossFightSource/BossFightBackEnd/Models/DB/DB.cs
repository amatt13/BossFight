using System;
using MySqlConnector;

namespace BossFight.Models.DB
{
    public class AppDb : IDisposable
    {
        public ConnectionWrapper Connection { get; private set; }

        public AppDb(string pConnectionString)
        {
            Connection = new ConnectionWrapper(pConnectionString);
        }

        public void Dispose() => Connection.Dispose();


        public class ConnectionWrapper : IDisposable
        {
            public MySqlConnection Connection { get; }            

            public ConnectionWrapper(string pConnectionString)
            {
                Connection = new MySqlConnection(pConnectionString);
            }

            public void Dispose() => Connection.Dispose();
        }
    }
}