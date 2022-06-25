using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BossFight.Models;
using BossFight.Models.DB;
using Ganss.XSS;
using System.Text.Json;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;


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
            Console.WriteLine($"HandleMessage: '{handler}'");
            var data = pJsonObject[REQUEST_DATA];
            var dataJsonDictionary = JsonSerializer.Deserialize<Dictionary<String, JsonElement>>(data.ToString());

            var methodToCall = this.GetType().GetMethod(handler);
            if (methodToCall != null)
            {
                await (Task)methodToCall.Invoke(this, new object[] { dataJsonDictionary, pWebSocketReceiveResult, pWebSocket });
            }
            else
            {
                throw new ArgumentException($"Method '{handler}' does not exist!\n");
            }
        }

        // takes: no data
        // returns: monster
        public async Task FetchActiveMonster(Dictionary<string, JsonElement> _, WebSocketReceiveResult pWebSocketReceiveResult, WebSocket pWebSocket)
        {
            var monster = new MonsterInstance { Active = true }.FindAll().First();
            var response = new Dictionary<string, MonsterInstance>
                {
                    { "fetch_active_monster", monster }
                };
            string output = JsonSerializer.Serialize(response);
            var byteArray = new ArraySegment<Byte>(Encoding.UTF8.GetBytes(output));
            await pWebSocket.SendAsync(byteArray, pWebSocketReceiveResult.MessageType, pWebSocketReceiveResult.EndOfMessage, CancellationToken.None);
        }

        // // takes: player_id: "int"
        // // returns: player
        // public async Task FetchPlayer(Dictionary<string, JsonElement> pJsonParameters, WebSocketReceiveResult pWebSocketReceiveResult, WebSocket pWebSocket)
        // {
        //     var requiredValues = CreateValueList(pJsonParameters, new List<string>{"player_id", "cake"});

        //     if (RequestValidator.AllValuesAreFilled(requiredValues, out string error))
        //     {
        //         var player = new Player().FindOne(Convert.ToInt32(pJsonParameters["player_id"].ToString()));
        //         var response = new Dictionary<string, Player>
        //             {
        //                 { "update_player", player }
        //             };
        //         string output = JsonSerializer.Serialize(response);
        //         var byteArray = new ArraySegment<Byte>(Encoding.UTF8.GetBytes(output));
        //         await pWebSocket.SendAsync(byteArray, pWebSocketReceiveResult.MessageType, pWebSocketReceiveResult.EndOfMessage, CancellationToken.None);
        //     }
        //     else
        //         await ReplyWithErrorMessage(pWebSocketReceiveResult, pWebSocket, error);
        // }

        // takes: player_id: "int", weapon_id: "int"
        // returns: dict => gold, weapons
        public async Task SellWeapon(Dictionary<string, JsonElement> pJsonParameters, WebSocketReceiveResult pWebSocketReceiveResult, WebSocket pWebSocket)
        {
            var requiredValues = CreateValueList(pJsonParameters, new List<string> { "player_id", "weapon_id" });

            if (RequestValidator.AllValuesAreFilled(requiredValues, out string error) && RequestValidator.PlayerCanSellWeapon(pJsonParameters["player_id"].GetInt32(), pJsonParameters["weapon_id"].GetInt32(), out error))
            {
                var player = new Player().FindOne(pJsonParameters["player_id"].GetInt32());
                var weaponId = pJsonParameters["weapon_id"].GetInt32();
                var weaponToSell = player.PlayerWeaponList.First(pw => pw.WeaponId == weaponId);
                weaponToSell.Sell();

                var response = new Dictionary<string, object>
                {
                    { "update_player_sold_weapon", new Dictionary<string, object>
                        {
                            { "gold", player.Gold },
                            { "weapons", player.PlayerWeaponList }
                        }
                    }
                };
                var byteArray = new ArraySegment<Byte>(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(response)));
                await pWebSocket.SendAsync(byteArray, pWebSocketReceiveResult.MessageType, pWebSocketReceiveResult.EndOfMessage, CancellationToken.None);
            }
            else
                await ReplyWithErrorMessage(pWebSocketReceiveResult, pWebSocket, error);
        }

        // takes: player_id: "int", weapon_id: "int
        // returns: weapon
        public async Task EquipWeapon(Dictionary<string, JsonElement> pJsonParameters, WebSocketReceiveResult pWebSocketReceiveResult, WebSocket pWebSocket)
        {
            var requiredValues = CreateValueList(pJsonParameters, new List<string> { "player_id", "weapon_id" });
            if (RequestValidator.AllValuesAreFilled(requiredValues, out string error) && RequestValidator.PlayerCanEquipWeapon(pJsonParameters["player_id"].GetInt32(), pJsonParameters["weapon_id"].GetInt32(), out error))
            {
                var player = new Player().FindOne(pJsonParameters["player_id"].GetInt32());
                var weaponId = pJsonParameters["weapon_id"].GetInt32();
                player.EquipWeapon(weaponId);

                var response = new Dictionary<string, object>
                {
                    { "update_player_equipped_weapon", player.PlayerWeaponList.First(pw => pw.WeaponId == weaponId).Weapon }
                };
                var byteArray = new ArraySegment<Byte>(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(response)));
                await pWebSocket.SendAsync(byteArray, pWebSocketReceiveResult.MessageType, pWebSocketReceiveResult.EndOfMessage, CancellationToken.None);
            }
            else
                await ReplyWithErrorMessage(pWebSocketReceiveResult, pWebSocket, error);
        }

        // takes: player_id: "int"
        // returns: PlayerAttackSummary
        public async Task PlayerAttackMonsterWithEquippedWeapon(Dictionary<string, JsonElement> pJsonParameters, WebSocketReceiveResult pWebSocketReceiveResult, WebSocket pWebSocket)
        {
            var requiredValues = CreateValueList(pJsonParameters, new List<string> { "player_id" });
            if (RequestValidator.AllValuesAreFilled(requiredValues, out string error) && RequestValidator.PlayerCanAttackMonsterWithEquippedWeapon(pJsonParameters["player_id"].GetInt32(), out error))
            {
                var player = new Player().FindOne(pJsonParameters["player_id"].GetInt32());
                var monster = new MonsterInstance { Active = true }.FindAll().First();
                var summary = DamageDealer.PlayerAttackMonster(player, monster, true);
                var response = new Dictionary<string, PlayerAttackSummary>
                {
                    { "player_attacked_monster_with_weapon", summary }
                };

                var byteArray = new ArraySegment<Byte>(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(response)));
                await pWebSocket.SendAsync(byteArray, pWebSocketReceiveResult.MessageType, pWebSocketReceiveResult.EndOfMessage, CancellationToken.None);

                if (summary.PlayerKilledMonster)
                {
                    await NewMonster(monster, player);
                }
                else
                {
                    // monster is still alive. Update everyone with the new monster
                    var monsterUpdate = new Dictionary<string, MonsterInstance>
                    {
                        { "fetch_active_monster", monster }
                    };
                    string output = JsonSerializer.Serialize(monsterUpdate);
                    var monsterUpdateByteArray = new ArraySegment<Byte>(Encoding.UTF8.GetBytes(output));
                    // send to everyone but the current connection. They just got an update with player_attacked_monster_with_weapon
                    var otherConnections = WebSocketConnections.GetInstance().GetAllOpenConnections().Where(con => con != pWebSocket);
                    foreach (var ws in otherConnections)
                        await ws.SendAsync(monsterUpdateByteArray, WebSocketMessageType.Text, true, CancellationToken.None);
                }
            }
            else
                await ReplyWithErrorMessage(pWebSocketReceiveResult, pWebSocket, error);
        }

        private async Task NewMonster(MonsterInstance pMonster, Player pPLayer)
        {
            var newMonsterInstance = MonsterSpawner.SpawnNewMonster();
            if (newMonsterInstance != null)
            {
                var monsterWasKilledMessage = $"{(pMonster.IsBossMonster ? "BOSS KILL\n" : String.Empty)}{pPLayer.Name} killed {pMonster.Name}!";
                var monsterDamageInfo = "Player - Damage dealt\n" + String.Join("\n", pMonster.MonsterDamageTrackerList.OrderBy(x => x.DamageReceivedFromPlayer).Select(x => $"{x.Player.Name} {x.DamageReceivedFromPlayer}"));
                var newMonsterMessage = new Dictionary<string, object>
                {
                    { "new_monster", new Dictionary<string, object>
                        {
                            { "newMonsterInstance", newMonsterInstance },
                            { "monsterWasKilledMessage", monsterWasKilledMessage },
                            { "monsterDamageInfo", monsterDamageInfo }
                        }
                    }
                };
                var byteArray = new ArraySegment<Byte>(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(newMonsterMessage)));
                foreach (var ws in WebSocketConnections.GetInstance().GetAllOpenConnections())
                    await ws.SendAsync(byteArray, WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }

        // takes: userName: "string", password: "string"
        // returns: player & MonsterTierVote
        public async Task SignIn(Dictionary<string, JsonElement> pJsonParameters, WebSocketReceiveResult pWebSocketReceiveResult, WebSocket pWebSocket)
        {
            var requiredValues = CreateValueList(pJsonParameters, new List<string> { "userName", "password" });
            if (RequestValidator.AllValuesAreFilled(requiredValues, out string error))
            {
                var userName = pJsonParameters["userName"].GetString();
                var password = pJsonParameters["password"].GetString();

                if (RequestValidator.ValidateUserLoginCredentials(userName, password, out error))
                {
                    userName = userName.Trim();
                    password = password.Trim();
                    var player = new Player { UserName = userName, Password = password }.FindAll().First();
                    var currentVote = MonsterTierVoteUpdater.PlayersCurrentMonsterTierVote(player.PlayerId.Value);
                    var response = new Dictionary<string, Dictionary<string, object>>
                    {
                        { "player_signed_in", new Dictionary<string, object> {
                            {"player", player},
                            {"current_vote", currentVote}
                        } }
                    };
                    string output = JsonSerializer.Serialize(response);
                    var byteArray = new ArraySegment<Byte>(Encoding.UTF8.GetBytes(output));
                    await pWebSocket.SendAsync(byteArray, pWebSocketReceiveResult.MessageType, pWebSocketReceiveResult.EndOfMessage, CancellationToken.None);
                }
            }
            if (!String.IsNullOrEmpty(error))
                await ReplyWithErrorMessage(pWebSocketReceiveResult, pWebSocket, error);
        }

        // takes: message: "string", player_id: "int"
        // receive an message from the client. This message must then be sent to all connection
        public async Task SendChatMessage(Dictionary<string, JsonElement> pJsonParameters, WebSocketReceiveResult pWebSocketReceiveResult, WebSocket pWebSocket)
        {
            var requiredValues = CreateValueList(pJsonParameters, new List<string> { "message", "player_id" });
            if (RequestValidator.AllValuesAreFilled(requiredValues, out string error))
            {
                var message = pJsonParameters["message"].GetString();
                var playerId = pJsonParameters["player_id"].GetInt32(); //TODO validate valid player
                if (RequestValidator.ValidateChatMessage(message, playerId, out error))
                {
                    var sanitized = new HtmlSanitizer().Sanitize(message);
                    var doubleEscaped = sanitized.Replace(@"\", @"\\");
                    var player = new Player().FindOne(playerId);
                    var chatMessage = new ChatMessage { MessageContent = doubleEscaped, Timestamp = DateTime.Now, Player = player };
                    chatMessage.Persist();
                    chatMessage.MessageContent = sanitized;

                    var response = new Dictionary<string, ChatMessage>
                    {
                        { "receive_chat_message", chatMessage }
                    };
                    string output = JsonSerializer.Serialize(response);
                    var byteArray = new ArraySegment<Byte>(Encoding.UTF8.GetBytes(output));
                    foreach (var ws in WebSocketConnections.GetInstance().GetAllOpenConnections())
                        await ws.SendAsync(byteArray, pWebSocketReceiveResult.MessageType, pWebSocketReceiveResult.EndOfMessage, CancellationToken.None);
                }
            }
            if (!String.IsNullOrEmpty(error))
                await ReplyWithErrorMessage(pWebSocketReceiveResult, pWebSocket, error);
        }

        // takes: messages_to_fetch: "int"
        // returns: list of ChatMessage
        public async Task FetchMostRecentMessages(Dictionary<string, JsonElement> pJsonParameters, WebSocketReceiveResult pWebSocketReceiveResult, WebSocket pWebSocket)
        {
            var requiredValues = CreateValueList(pJsonParameters, new List<string> { "messages_to_fetch" });
            if (RequestValidator.AllValuesAreFilled(requiredValues, out string error))
            {
                var messagesToFetch = pJsonParameters["messages_to_fetch"].GetInt32();

                if (RequestValidator.ValidateMaxMessageRequestNumberNotExceeded(messagesToFetch, out error))
                {
                    var chatMessages = new ChatMessage { }.FindTop((UInt32)messagesToFetch, nameof(ChatMessage.Timestamp));
                    var response = new Dictionary<string, IEnumerable<ChatMessage>>
                    {
                        { "receive_multiple_chat_message", chatMessages }
                    };
                    string output = JsonSerializer.Serialize(response);
                    var byteArray = new ArraySegment<Byte>(Encoding.UTF8.GetBytes(output));
                    await pWebSocket.SendAsync(byteArray, pWebSocketReceiveResult.MessageType, pWebSocketReceiveResult.EndOfMessage, CancellationToken.None);
                }
            }
            if (!String.IsNullOrEmpty(error))
                await ReplyWithErrorMessage(pWebSocketReceiveResult, pWebSocket, error);
        }

        // takes: player_id: "int", monster_instance_id: "int", vote: "int"
        public async Task VoteForMonsterTier(Dictionary<string, JsonElement> pJsonParameters, WebSocketReceiveResult pWebSocketReceiveResult, WebSocket pWebSocket)
        {
            var requiredValues = CreateValueList(pJsonParameters, new List<string> { "player_id", "monster_instance_id", "vote" });

            if (RequestValidator.AllValuesAreFilled(requiredValues, out string error))
            {
                var playerId = pJsonParameters["player_id"].GetInt32();
                var monsterInstanceId = pJsonParameters["monster_instance_id"].GetInt32();
                var vote = pJsonParameters["vote"].GetInt32();
                if (RequestValidator.ValidateVoteForMonsterTier(playerId, monsterInstanceId, vote, out error))
                {
                    MonsterTierVoteUpdater.UpdatePlayersMonsterTierVote(playerId, monsterInstanceId, vote);
                }
            }

            if (!String.IsNullOrEmpty(error))
                await ReplyWithErrorMessage(pWebSocketReceiveResult, pWebSocket, error);
        }

        private List<Tuple<object, string>> CreateValueList(Dictionary<string, JsonElement> pDict, List<string> pRequiredValues)
        {
            var valuesList = new List<Tuple<object, string>>();
            foreach (var key in pRequiredValues)
            {
                if (pDict.ContainsKey(key))
                {
                    valuesList.Add(new Tuple<object, string>(pDict[key], key));
                }
                else
                {
                    valuesList.Add(new Tuple<object, string>(null, key));
                }
            }
            return valuesList;
        }

        async private Task ReplyWithErrorMessage(WebSocketReceiveResult pWebSocketReceiveResult, WebSocket pWebSocket, string pError)
        {
            var response = new Dictionary<string, string>
            {
                { "error_message", pError }
            };
            var byteArray = new ArraySegment<Byte>(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(response)));
            await pWebSocket.SendAsync(byteArray, pWebSocketReceiveResult.MessageType, pWebSocketReceiveResult.EndOfMessage, CancellationToken.None);
        }

        // EXAMPLE/TEMPLATE FUNCTION
        // takes: player_id: "int"
        public async Task Example(Dictionary<string, JsonElement> pJsonParameters, WebSocketReceiveResult pWebSocketReceiveResult, WebSocket pWebSocket)
        {
            var requiredValues = CreateValueList(pJsonParameters, new List<string> { "player_id" });

            if (RequestValidator.AllValuesAreFilled(requiredValues, out string error))
            {
                var playerId = pJsonParameters["player_id"].GetInt32();
                if (RequestValidator.Example())
                {
                    var player = new Player().FindOne(playerId);
                    var response = new Dictionary<string, string>
                    {
                        { "player_name", player.Name }
                    };

                    var byteArray = new ArraySegment<Byte>(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(response)));
                    await pWebSocket.SendAsync(byteArray, pWebSocketReceiveResult.MessageType, pWebSocketReceiveResult.EndOfMessage, CancellationToken.None);
                }
            }

            if (!String.IsNullOrEmpty(error))
                await ReplyWithErrorMessage(pWebSocketReceiveResult, pWebSocket, error);
        }
    }
}