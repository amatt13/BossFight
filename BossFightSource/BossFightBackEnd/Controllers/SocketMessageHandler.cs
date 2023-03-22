using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BossFight.Models;
using BossFight.Models.DB;
using Ganss.Xss;
using System.Text.Json;

namespace BossFight.Controllers
{
    public class SocketMessageHandler
    {
        public static string REQUEST_KEY = "request_key";
        public static string REQUEST_DATA = "request_data";

        private readonly Dictionary<string, Func<Dictionary<string, JsonElement>, WebSocketReceiveResult, WebSocket, Task>> methodDictionary = new();
        public SocketMessageHandler() 
        { 
            // Populate the dictionary with method delegates
            methodDictionary[nameof(FetchActiveMonster)] = FetchActiveMonster;
            methodDictionary[nameof(FetchPlayer)] = FetchPlayer;
            methodDictionary[nameof(SellWeapon)] = SellWeapon;
            methodDictionary[nameof(EquipWeapon)] = EquipWeapon;
            methodDictionary[nameof(PlayerAttackMonsterWithEquippedWeapon)] = PlayerAttackMonsterWithEquippedWeapon;
            methodDictionary[nameof(SignIn)] = SignIn;
            methodDictionary[nameof(SendChatMessage)] = SendChatMessage;
            methodDictionary[nameof(FetchMostRecentMessages)] = FetchMostRecentMessages;
            methodDictionary[nameof(VoteForMonsterTier)] = VoteForMonsterTier;
            methodDictionary[nameof(FetchMonsterVotesTotals)] = FetchMonsterVotesTotals;
            methodDictionary[nameof(GetShopForPlayer)] = GetShopForPlayer;
            methodDictionary[nameof(BuyPlayerClass)] = BuyPlayerClass;
            methodDictionary[nameof(GetUnlockedClassesForPlayer)] = GetUnlockedClassesForPlayer;
            //methodDictionary[nameof(Example)] = Example;
        }

        public async Task HandleMessage(Dictionary<string, object> pJsonObject, WebSocketReceiveResult pWebSocketReceiveResult, WebSocket pWebSocket)
        {
            var handler = pJsonObject[REQUEST_KEY].ToString();
            Console.WriteLine($"HandleMessage: '{handler}'");
            var data = pJsonObject[REQUEST_DATA] ?? "{}";
            var dataJsonDictionary = JsonSerializer.Deserialize<Dictionary<String, JsonElement>>(data.ToString());

            if (methodDictionary.TryGetValue(handler, out var method))
            {
                await method(dataJsonDictionary, pWebSocketReceiveResult, pWebSocket);
            }
            else
            {
                throw new ArgumentException($"Method '{handler}' does not exist!\n");
            }
        }

        // takes: no data
        // returns: monster
        private async Task FetchActiveMonster(Dictionary<string, JsonElement> _, WebSocketReceiveResult pWebSocketReceiveResult, WebSocket pWebSocket)
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

        // takes: player_id: "int"
        // returns: player
        private async Task FetchPlayer(Dictionary<string, JsonElement> pJsonParameters, WebSocketReceiveResult pWebSocketReceiveResult, WebSocket pWebSocket)
        {
            var requiredValues = CreateValueList(pJsonParameters, new List<string>{"player_id", "cake"});

            if (RequestValidator.AllValuesAreFilled(requiredValues, out string error))
            {
                var player = new Player().FindOne(Convert.ToInt32(pJsonParameters["player_id"].ToString()));
                var response = new Dictionary<string, Player>
                    {
                        { "update_player", player }
                    };
                string output = JsonSerializer.Serialize(response);
                var byteArray = new ArraySegment<Byte>(Encoding.UTF8.GetBytes(output));
                await pWebSocket.SendAsync(byteArray, pWebSocketReceiveResult.MessageType, pWebSocketReceiveResult.EndOfMessage, CancellationToken.None);
            }
            else
                await ReplyWithErrorMessage(pWebSocketReceiveResult, pWebSocket, error);
        }

        // takes: player_id: "int", weapon_id: "int"
        // returns: dict => gold, weapons
        private async Task SellWeapon(Dictionary<string, JsonElement> pJsonParameters, WebSocketReceiveResult pWebSocketReceiveResult, WebSocket pWebSocket)
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
        private async Task EquipWeapon(Dictionary<string, JsonElement> pJsonParameters, WebSocketReceiveResult pWebSocketReceiveResult, WebSocket pWebSocket)
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
        private async Task PlayerAttackMonsterWithEquippedWeapon(Dictionary<string, JsonElement> pJsonParameters, WebSocketReceiveResult pWebSocketReceiveResult, WebSocket pWebSocket)
        {
            var requiredValues = CreateValueList(pJsonParameters, new List<string> { "player_id" });
            if (RequestValidator.AllValuesAreFilled(requiredValues, out string error) && RequestValidator.PlayerCanAttackMonsterWithEquippedWeapon(pJsonParameters["player_id"].GetInt32(), out error))
            {
                var player = new Player().FindOne(pJsonParameters["player_id"].GetInt32());
                var monster = new MonsterInstance { Active = true }.FindOne();
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
                    await WebSocketConnections.GetInstance().SendMessageToEveryOneElseAsync(pWebSocket, monsterUpdateByteArray);
                }
            }
            else
                await ReplyWithErrorMessage(pWebSocketReceiveResult, pWebSocket, error);
        }

        //TODO: move to another class
        private static async Task NewMonster(MonsterInstance pMonster, Player pPLayer)
        {
            var newMonsterInstance = MonsterSpawner.SpawnNewMonster();
            if (newMonsterInstance != null)
            {
                var monsterWasKilledMessage = $"{(pMonster.IsBossMonster ? "BOSS KILL\n" : String.Empty)}{pPLayer.Name} killed {pMonster.Name}!";
                var monsterDamageInfo = "Player - Damage dealt\n" + String.Join(
                    "\n", pMonster.MonsterDamageTrackerList
                    .OrderBy(x => x.DamageReceivedFromPlayer)
                    .Select(x => $"{x.Player.Name} {x.DamageReceivedFromPlayer}")
                    ) + "\n__________";
                var votesTotal = MonsterTierVoteUpdater.CountMonsterTierVotesTotalForActiveMonster();

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
                var monsterTierVotesTotalMessage = new Dictionary<string, MonsterTierVoteUpdater.MonsterTierVotesTotal>
                {
                    { "monster_tier_votes_total", votesTotal }
                };
                var monsterByteArray = new ArraySegment<Byte>(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(newMonsterMessage)));
                var voteByteArray = new ArraySegment<Byte>(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(monsterTierVotesTotalMessage)));
                foreach (var ws in WebSocketConnections.GetInstance().GetAllOpenConnections())
                {
                    await ws.SendAsync(monsterByteArray, WebSocketMessageType.Text, true, CancellationToken.None);
                    await ws.SendAsync(voteByteArray, WebSocketMessageType.Text, true, CancellationToken.None);
                }
            }
        }

        // takes: userName: "string", password: "string"
        // returns: player & MonsterTierVote
        private async Task SignIn(Dictionary<string, JsonElement> pJsonParameters, WebSocketReceiveResult pWebSocketReceiveResult, WebSocket pWebSocket)
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
        private async Task SendChatMessage(Dictionary<string, JsonElement> pJsonParameters, WebSocketReceiveResult pWebSocketReceiveResult, WebSocket pWebSocket)
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
        private async Task FetchMostRecentMessages(Dictionary<string, JsonElement> pJsonParameters, WebSocketReceiveResult pWebSocketReceiveResult, WebSocket pWebSocket)
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
        private async Task VoteForMonsterTier(Dictionary<string, JsonElement> pJsonParameters, WebSocketReceiveResult pWebSocketReceiveResult, WebSocket pWebSocket)
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
                    
                    // update everyone else about the new vote totals
                    var votesTotal = MonsterTierVoteUpdater.CountMonsterTierVotesTotalForActiveMonster();
                    var response = new Dictionary<string, MonsterTierVoteUpdater.MonsterTierVotesTotal>
                    {
                        { "monster_tier_votes_total", votesTotal }
                    };
                    var byteArray = new ArraySegment<Byte>(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(response)));
                    await WebSocketConnections.GetInstance().SendMessageToEveryOneElseAsync(pWebSocket, byteArray);
                }
            }

            if (!String.IsNullOrEmpty(error))
                await ReplyWithErrorMessage(pWebSocketReceiveResult, pWebSocket, error);
        }

        private async Task FetchMonsterVotesTotals(Dictionary<string, JsonElement> _, WebSocketReceiveResult pWebSocketReceiveResult, WebSocket pWebSocket)
        {
            var votesTotal = MonsterTierVoteUpdater.CountMonsterTierVotesTotalForActiveMonster();
            var response = new Dictionary<string, MonsterTierVoteUpdater.MonsterTierVotesTotal>
            {
                { "monster_tier_votes_total", votesTotal }
            };

            var byteArray = new ArraySegment<Byte>(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(response)));
            await pWebSocket.SendAsync(byteArray, pWebSocketReceiveResult.MessageType, pWebSocketReceiveResult.EndOfMessage, CancellationToken.None);
        }

        private async Task GetShopForPlayer(Dictionary<string, JsonElement> pJsonParameters, WebSocketReceiveResult pWebSocketReceiveResult, WebSocket pWebSocket)
        {
            var requiredValues = CreateValueList(pJsonParameters, new List<string> { "player_id" });

            if (RequestValidator.AllValuesAreFilled(requiredValues, out string error))
            {
                var playerId = pJsonParameters["player_id"].GetInt32();
                if (RequestValidator.PlayerExists(playerId))
                {
                    var shop = ShopController.GetShopForPlayer(playerId);
                    var response = new Dictionary<string, object>
                    {
                        { "shopMenu", shop }
                    };

                    var byteArray = new ArraySegment<Byte>(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(response)));
                    await pWebSocket.SendAsync(byteArray, pWebSocketReceiveResult.MessageType, pWebSocketReceiveResult.EndOfMessage, CancellationToken.None);
                }
            }

            if (!String.IsNullOrEmpty(error))
                await ReplyWithErrorMessage(pWebSocketReceiveResult, pWebSocket, error);
        }

        // takes: player_id: "int", player_class_id: "int"
        // returns: {sucess: bool, updated_player}
        private async Task BuyPlayerClass(Dictionary<string, JsonElement> pJsonParameters, WebSocketReceiveResult pWebSocketReceiveResult, WebSocket pWebSocket)
        {
            var requiredValues = CreateValueList(pJsonParameters, new List<string> { "player_id", "player_class_id" });

            if (RequestValidator.AllValuesAreFilled(requiredValues, out string error))
            {
                var playerId = pJsonParameters["player_id"].GetInt32();
                var playerClassId = pJsonParameters["player_class_id"].GetInt32();
                if (
                    RequestValidator.PlayerExists(playerId, out error) 
                    && RequestValidator.PlayerClassExists(playerClassId, out error) 
                    && RequestValidator.PlayerIsEligibleForPlayerClassAcquisition(playerId, playerClassId, out error))
                {
                    Tuple<bool, string> result = ShopController.BuyPlayerClass(playerClassId, playerId);
                    var updatedPlayer = new Player{}.FindOne(playerId);
                    var response = new Dictionary<string, Dictionary<string, object>>
                    {
                        { 
                            "bought_player_class", new Dictionary<string, object> 
                            {
                                {"sucess", result.Item1},
                                {"message", result.Item2},
                                {"updated_player", updatedPlayer}
                            } 
                        }
                    };

                    var byteArray = new ArraySegment<Byte>(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(response)));
                    await pWebSocket.SendAsync(byteArray, pWebSocketReceiveResult.MessageType, pWebSocketReceiveResult.EndOfMessage, CancellationToken.None);
                }
            }

            if (!String.IsNullOrEmpty(error))
                await ReplyWithErrorMessage(pWebSocketReceiveResult, pWebSocket, error);
        }

        // takes: player_id: "int"
        // return List of PlayerPlayerClass
        private async Task GetUnlockedClassesForPlayer(Dictionary<string, JsonElement> pJsonParameters, WebSocketReceiveResult pWebSocketReceiveResult, WebSocket pWebSocket)
        {
            var requiredValues = CreateValueList(pJsonParameters, new List<string> { "player_id" });

            if (RequestValidator.AllValuesAreFilled(requiredValues, out string error))
            {
                var playerId = pJsonParameters["player_id"].GetInt32();
                if (RequestValidator.PlayerExists(playerId))
                {
                    var player = new Player().FindOne(playerId);
                    IEnumerable<PlayerPlayerClass> unlockedClasses = player.UnlockedPlayerPlayerClassList;
                    var response = new Dictionary<string, object>
                    {
                        { "unlocked_classes", unlockedClasses }
                    };

                    var byteArray = new ArraySegment<Byte>(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(response)));
                    await pWebSocket.SendAsync(byteArray, pWebSocketReceiveResult.MessageType, pWebSocketReceiveResult.EndOfMessage, CancellationToken.None);
                }
            }

            if (!String.IsNullOrEmpty(error))
                await ReplyWithErrorMessage(pWebSocketReceiveResult, pWebSocket, error);
        }

        // takes: player_id: "int", player_class_id: "int"
        private async Task ChangePlayerClass(Dictionary<string, JsonElement> pJsonParameters, WebSocketReceiveResult pWebSocketReceiveResult, WebSocket pWebSocket)
        {
            var requiredValues = CreateValueList(pJsonParameters, new List<string> { "player_id", "player_class_id" });

            if (RequestValidator.AllValuesAreFilled(requiredValues, out string error))
            {
                var playerId = pJsonParameters["player_id"].GetInt32();
                var player_class_id = pJsonParameters["player_class_id"].GetInt32();
                if (
                    RequestValidator.PlayerExists(playerId, out error) 
                    && RequestValidator.PlayerClassExists(player_class_id, out error) 
                    && RequestValidator.PlayerOwnsPlayerClass(playerId, player_class_id, out error))
                {
                    var oldPlayerPlayerClassRelation = new PlayerPlayerClass{ PlayerId = playerId, Active = true }.FindOne();
                    oldPlayerPlayerClassRelation.Active = false;
                    oldPlayerPlayerClassRelation.Persist();

                    var newPlayerPlayerClassActiveRelation = new PlayerPlayerClass{ PlayerId = playerId, PlayerClassId = player_class_id }.FindOne();
                    newPlayerPlayerClassActiveRelation.Active = true;
                    newPlayerPlayerClassActiveRelation.Persist();
                    
                    var player = new Player().FindOne(playerId);
                    var response = new Dictionary<string, Player>
                    {
                        { "update_player", player }
                    };

                    var byteArray = new ArraySegment<Byte>(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(response)));
                    await pWebSocket.SendAsync(byteArray, pWebSocketReceiveResult.MessageType, pWebSocketReceiveResult.EndOfMessage, CancellationToken.None);
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
        private async Task Example(Dictionary<string, JsonElement> pJsonParameters, WebSocketReceiveResult pWebSocketReceiveResult, WebSocket pWebSocket)
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