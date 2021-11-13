using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        public SocketMessageHandler() { }
        
        public async Task HandleMessage(Dictionary<string, object> pJsonObject, WebSocketReceiveResult pWebSocketReceiveResult, WebSocket pWebSocket)
        {
            var handler = pJsonObject[REQUEST_KEY].ToString();
            var data = pJsonObject[REQUEST_DATA];
            var dataJsonDictionary = JsonConvert.DeserializeObject<Dictionary<String, Object>>(data.ToString());
            
            await (Task)this.GetType().GetMethod(handler).Invoke(this, new object[] { dataJsonDictionary, pWebSocketReceiveResult, pWebSocket });
        }

        // no data
        public async Task FetchActiveMonster(Dictionary<string, object> _, WebSocketReceiveResult pWebSocketReceiveResult, WebSocket pWebSocket)
        {
            var monster = new MonsterInstance{ Active = true }.FindAll().First();
            var response = new Dictionary<string, MonsterInstance>
                { 
                    { "fetch_active_monster", monster }
                };
            string output = JsonConvert.SerializeObject(response);
            var byteArray = new ArraySegment<Byte>(Encoding.UTF8.GetBytes(output));
            await pWebSocket.SendAsync(byteArray, pWebSocketReceiveResult.MessageType, pWebSocketReceiveResult.EndOfMessage, CancellationToken.None);
        }

        // player_id: "int"
        public async Task FetchPlayer(Dictionary<string, object> pJsonParameters, WebSocketReceiveResult pWebSocketReceiveResult, WebSocket pWebSocket)
        {
            var player = new Player().FindOne(Convert.ToInt32(pJsonParameters["player_id"]));
            var response = new Dictionary<string, Player>
                { 
                    { "update_player", player }
                };
            string output = JsonConvert.SerializeObject(response);
            var byteArray = new ArraySegment<Byte>(Encoding.UTF8.GetBytes(output));
            await pWebSocket.SendAsync(byteArray, pWebSocketReceiveResult.MessageType, pWebSocketReceiveResult.EndOfMessage, CancellationToken.None);
        }

        // player_id: "int", weapon_id: "int"
        public async Task SellWeapon(Dictionary<string, object> pJsonParameters, WebSocketReceiveResult pWebSocketReceiveResult, WebSocket pWebSocket)
        {
            if (RequestValidator.PlayerCanSellWeapon(pJsonParameters["player_id"].ToString(), pJsonParameters["weapon_id"].ToString(), out string error))
            {
                var player = new Player().FindOne(Convert.ToInt32(pJsonParameters["player_id"]));
                var weaponId = Convert.ToInt32(pJsonParameters["weapon_id"]);
                var weaponToSell = player.PlayerWeaponList.First(pw => pw.WeaponId == weaponId);
                weaponToSell.Sell();
                
                var response = new Dictionary<string, object>
                { 
                    { "update_player_sold_weapon", new Dictionary<string, object>
                        {
                            { "gold", player.Gold },
                            { "weapons", player.PlayerWeaponList }
                        } }
                };
                var byteArray = new ArraySegment<Byte>(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(response)));
                await pWebSocket.SendAsync(byteArray, pWebSocketReceiveResult.MessageType, pWebSocketReceiveResult.EndOfMessage, CancellationToken.None);
            }
            else
                await ReplyWithErrorMessage(pWebSocketReceiveResult, pWebSocket, error);
        }

        // player_id: "int", weapon_id: "int
        public async Task EquipWeapon(Dictionary<string, object> pJsonParameters, WebSocketReceiveResult pWebSocketReceiveResult, WebSocket pWebSocket)
        {
            if (RequestValidator.PlayerCanEquipWeapon(pJsonParameters["player_id"].ToString(), pJsonParameters["weapon_id"].ToString(), out string error))
            {
                var player = new Player().FindOne(Convert.ToInt32(pJsonParameters["player_id"]));
                var weaponId = Convert.ToInt32(pJsonParameters["weapon_id"]);
                player.EquipWeapon(weaponId);
                
                var response = new Dictionary<string, object>
                { 
                    { "update_player_equipped_weapon", new Dictionary<string, Weapon>
                        {
                            { "Weapon", player.PlayerWeaponList.First(pw => pw.WeaponId == weaponId).Weapon }
                        } }
                };
                var byteArray = new ArraySegment<Byte>(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(response)));
                await pWebSocket.SendAsync(byteArray, pWebSocketReceiveResult.MessageType, pWebSocketReceiveResult.EndOfMessage, CancellationToken.None);
            }
            else
                await ReplyWithErrorMessage(pWebSocketReceiveResult, pWebSocket, error);
        }

        // player_id: "int"
        public async Task PlayerAttackMonsterWithEquippedWeapon(Dictionary<string, object> pJsonParameters, WebSocketReceiveResult pWebSocketReceiveResult, WebSocket pWebSocket)
        {
            if (RequestValidator.PlayerCanAttackMonsterWithEquippedWeapon(pJsonParameters["player_id"].ToString(), out string error))
            {
                var player = new Player().FindOne(Convert.ToInt32(pJsonParameters["player_id"]));
                var monster = new MonsterInstance { Active = true }.FindAll().First();
                var summary = DamageDealer.PlayerAttackMonster(player, monster, true);
                var response = new Dictionary<string, object>
                { 
                    { "player_attacked_monster_with_weapon", new Dictionary<string, PlayerAttackSummary>
                        {
                            { "summary", summary }
                        } }
                };

                var byteArray = new ArraySegment<Byte>(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(response)));
                await pWebSocket.SendAsync(byteArray, pWebSocketReceiveResult.MessageType, pWebSocketReceiveResult.EndOfMessage, CancellationToken.None);
            }
            else
                await ReplyWithErrorMessage(pWebSocketReceiveResult, pWebSocket, error);
        }

        async private Task ReplyWithErrorMessage(WebSocketReceiveResult pWebSocketReceiveResult, WebSocket pWebSocket, string pError)
        {
            var response = new Dictionary<string, string>
            { 
                { "error_message", pError }
            };
            var byteArray = new ArraySegment<Byte>(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(response)));
            await pWebSocket.SendAsync(byteArray, pWebSocketReceiveResult.MessageType, pWebSocketReceiveResult.EndOfMessage, CancellationToken.None);
        }
    }
}