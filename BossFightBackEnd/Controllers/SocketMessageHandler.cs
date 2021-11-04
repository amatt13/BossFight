using System;
using System.Collections.Generic;
using System.IO;
using System.Net.WebSockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BossFight.Models;
using BossFight.Models.DB;
using Newtonsoft.Json;

namespace BossFight.Controllers
{
    public class SocketMessageHandler
    {
        public static string REQUEST_KEY = "request_key";
        public static string REQUEST_DATA = "request_data";

        // public AppDb Db { get; }

        // public SocketMessageHandler(AppDb db)
        public SocketMessageHandler()
        {
            // Db = db;
            // Db.Connection.Open();
        }
        
        public async Task HandleMessage(Dictionary<string, object> pJsonObject, WebSocketReceiveResult pWebSocketReceiveResult, WebSocket pWebSocket)
        {
            var handler = pJsonObject[REQUEST_KEY].ToString();
            var data = pJsonObject[REQUEST_DATA];
            var dataJsonDictionary = JsonConvert.DeserializeObject<Dictionary<String, Object>>(data.ToString());
            
            await (Task)this.GetType().GetMethod(handler).Invoke(this, new object[] { dataJsonDictionary, pWebSocketReceiveResult, pWebSocket });
        }

        // player_id: "int"
        public async Task FetchPlayer(Dictionary<string, object> pJsonParameters, WebSocketReceiveResult pWebSocketReceiveResult, WebSocket pWebSocket)
        {
            var player = new Player().FindOne(Convert.ToInt32(pJsonParameters["player_id"]));
            string output = JsonConvert.SerializeObject(player);
            var byteArray = new ArraySegment<Byte>(Encoding.UTF8.GetBytes(output));
            //Db.Connection.Close();
            await pWebSocket.SendAsync(byteArray, pWebSocketReceiveResult.MessageType, pWebSocketReceiveResult.EndOfMessage, CancellationToken.None);
        }
    }
}