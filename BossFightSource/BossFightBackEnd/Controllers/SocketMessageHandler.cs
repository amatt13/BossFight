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
using Microsoft.Extensions.Logging;
using BossFight.BossFightBackEnd.BossFightLogger;

namespace BossFight.Controllers
{
    public class SocketMessageHandler
    {
        private static readonly string REQUEST_KEY = "request_key";
        private static readonly string REQUEST_DATA = "request_data";
        private readonly ILogger<SocketMessageHandler> _logger;

        private readonly Dictionary<string, Func<Dictionary<string, JsonElement>, WebSocketReceiveResult, WebSocket, Task>> methodDictionary = new();
        public SocketMessageHandler()
        {
            ILoggerProvider fileLoggerProvider = new BossFightLoggerProvider("logs/SocketMessageHandler.txt");
            ILoggerFactory _loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
                builder.AddDebug();
                builder.AddProvider(fileLoggerProvider);
                builder.SetMinimumLevel(LogLevel.Trace);
            });
            _logger = _loggerFactory.CreateLogger<SocketMessageHandler>();

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
            methodDictionary[nameof(ChangePlayerClass)] = ChangePlayerClass;
            methodDictionary[nameof(CastAbility)] = CastAbility;
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
                var methodName = method.Method.Name;
                _logger.LogDebug("Executing {methodName}", methodName);
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
            if (RequestValidator.AllValuesAreFilled(requiredValues, out string error)
                && RequestValidator.PlayerExists(pJsonParameters["player_id"].GetInt32(), out Player player, out error)
                && RequestValidator.PlayerCanAttackMonsterWithEquippedWeapon(pJsonParameters["player_id"].GetInt32(), out error))
            {
                var webSocketConnections = WebSocketConnections.GetInstance();

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
                    await webSocketConnections.SendMessageToEveryOneElseWhoAreLoggedInAsync(pWebSocket, monsterUpdateByteArray);
                }

                var wsc = webSocketConnections.GetConnection(pWebSocket);
                if (wsc != null)
                {
                    wsc.Player = player;
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
                    await ws.WebSocket.SendAsync(monsterByteArray, WebSocketMessageType.Text, true, CancellationToken.None);
                    await ws.WebSocket.SendAsync(voteByteArray, WebSocketMessageType.Text, true, CancellationToken.None);
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
                    var bossFightWebSocket = WebSocketConnections.GetInstance().GetConnection(pWebSocket);
                    if (bossFightWebSocket != null)
                        bossFightWebSocket.Player = player;
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
                        await ws.WebSocket.SendAsync(byteArray, pWebSocketReceiveResult.MessageType, pWebSocketReceiveResult.EndOfMessage, CancellationToken.None);
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
                    await WebSocketConnections.GetInstance().SendMessageToEveryOneElseWhoAreLoggedInAsync(pWebSocket, byteArray);
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
                if (RequestValidator.PlayerExists(playerId, out Player player, out error))
                {
                    var shop = ShopController.GetShopForPlayer(player);
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
                    RequestValidator.PlayerExists(playerId, out Player player, out error)
                    && RequestValidator.PlayerClassExists(playerClassId, out PlayerClass playerClass, out error)
                    && RequestValidator.PlayerIsEligibleForPlayerClassAcquisition(player, playerClass, out error))
                {
                    Tuple<bool, string> result = ShopController.BuyPlayerClass(playerClass, player);
                    var updatedPlayer = new Player().FindOne(playerId);
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
                if (RequestValidator.PlayerExists(playerId, out Player player, out error))
                {
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

        // takes: player_id: "int", player_class_id: "int", preffered_body_type: "string"
        private async Task ChangePlayerClass(Dictionary<string, JsonElement> pJsonParameters, WebSocketReceiveResult pWebSocketReceiveResult, WebSocket pWebSocket)
        {
            var requiredValues = CreateValueList(pJsonParameters, new List<string> { "player_id", "player_class_id" });

            if (RequestValidator.AllValuesAreFilled(requiredValues, out string error))
            {
                var playerId = pJsonParameters["player_id"].GetInt32();
                var playerClassId = pJsonParameters["player_class_id"].GetInt32();
                var prefferedBodyTypeName = pJsonParameters["preffered_body_type"].GetString();
                if (
                    RequestValidator.PlayerExists(playerId, out Player player, out error)
                    && RequestValidator.PlayerClassExists(playerClassId, out PlayerClass playerClass, out error)
                    && RequestValidator.PlayerOwnsPlayerClass(playerId, playerClass, out error)
                    && RequestValidator.BodyTypeNameExists(prefferedBodyTypeName, out error))
                {
                    var updatePlayer = false;

                    // Do we need to update the PlayerPlayerClass relation?
                    var currentPlayerPlayerClassRelation = new PlayerPlayerClass{ PlayerId = playerId, Active = true }.FindOne();
                    if (playerClass.PlayerClassId != currentPlayerPlayerClassRelation.PlayerClass.PlayerClassId)
                    {
                        currentPlayerPlayerClassRelation.Active = false;
                        currentPlayerPlayerClassRelation.Player = player;
                        currentPlayerPlayerClassRelation.Persist();

                        var newPlayerPlayerClassActiveRelation = new PlayerPlayerClass{ PlayerId = playerId, PlayerClass = playerClass, PlayerClassId = playerClass.PlayerClassId }.FindOne();
                        newPlayerPlayerClassActiveRelation.Active = true;
                        player.PlayerPlayerClass = newPlayerPlayerClassActiveRelation;
                        newPlayerPlayerClassActiveRelation.Player = player;
                        newPlayerPlayerClassActiveRelation.Persist();

                        if (newPlayerPlayerClassActiveRelation.MaxHp < player.Hp)
                        {
                            player.Hp = newPlayerPlayerClassActiveRelation.MaxHp;
                            updatePlayer = true;
                        }

                        if (newPlayerPlayerClassActiveRelation.MaxMana < player.Mana)
                        {
                            player.Mana = newPlayerPlayerClassActiveRelation.MaxMana;
                            updatePlayer = true;
                        }
                    }

                    // Do we need to update the player's BodyType?
                    var newBodyType = new BodyType{ Name = prefferedBodyTypeName }.FindOne();
                    if (player.PreferredBodyTypeId != newBodyType.BodyTypeId)
                    {
                        player.PrefferedBodyType = newBodyType;
                        player.PreferredBodyTypeId = newBodyType.BodyTypeId.Value;
                        updatePlayer = true;
                    }


                    if (updatePlayer)
                    {
                        player.Persist();
                        player = new Player().FindOne(player.PlayerId);
                    }

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

        private static async Task _castAbilityUpdatePlayerTarget(Player pTargetPlayer, Player pCasterPlayer, BossFightWebSocket pWebSocket, AbilityResult pAbilityResult)
        {
            var response = new Dictionary<string, object>
            {
                {
                    "player_cast_ability_on_you", new Dictionary<string, object>
                    {
                        {"caster_player", pCasterPlayer},
                        {"update_player", pTargetPlayer},
                        {"ability_text_result", pAbilityResult.AbilityResultText},
                    }
                }
            };

            var byteArray = new ArraySegment<Byte>(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(response)));
            await pWebSocket.WebSocket.SendAsync(byteArray, WebSocketMessageType.Text, true, CancellationToken.None);
        }

        // takes: player_id: "int", ability_name: "string", target_id: "int"
        private async Task CastAbility(Dictionary<string, JsonElement> pJsonParameters, WebSocketReceiveResult pWebSocketReceiveResult, WebSocket pWebSocket)
        {
            var requiredValues = CreateValueList(pJsonParameters, new List<string> { "player_id", "ability_name", "target_id" });

            if (RequestValidator.AllValuesAreFilled(requiredValues, out string error))
            {
                var playerId = pJsonParameters["player_id"].GetInt32();
                var abilityName = pJsonParameters["ability_name"].GetString();
                var targetId = pJsonParameters["target_id"].GetInt32();

                if (RequestValidator.PlayerExists(playerId, out Player player, out error))
                {
                    var ability = AbilityController.CreateAbility(abilityName, ref error);
                    if (ability != null)
                    {
                        ITarget target = AbilityController.FindTargetForAbility(targetId, player, ability, ref error);
                        if (target != null)
                        {
                            var abilityCastResult = ability.UseAbility(player, target);

                            if (abilityCastResult.CastSuccess)
                            {
                                var webSocketConnections = WebSocketConnections.GetInstance();

                                player.Persist();
                                if (target.Id != player.PlayerId.Value && target is Player targetPlayer)
                                {
                                    targetPlayer.Persist();
                                    var bfws = webSocketConnections.GetConnection(targetPlayer);
                                    await _castAbilityUpdatePlayerTarget(targetPlayer, player, bfws, abilityCastResult);
                                }

                                var response = new Dictionary<string, object>
                                {
                                    {
                                        "ability_cast_result", new Dictionary<string, object>
                                        {
                                            {"cast_success", abilityCastResult.CastSuccess},
                                            {"update_player", player},
                                            {"ability_text_result", abilityCastResult.AbilityResultText},
                                            {"attack_summary", abilityCastResult.PlayerAttackSummary}
                                        }
                                    }
                                };

                                var byteArray = new ArraySegment<Byte>(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(response)));
                                await pWebSocket.SendAsync(byteArray, pWebSocketReceiveResult.MessageType, pWebSocketReceiveResult.EndOfMessage, CancellationToken.None);

                                if (abilityCastResult.ReloadMonster && abilityCastResult.PlayerAttackSummary != null)
                                {
                                    if (abilityCastResult.PlayerAttackSummary.PlayerKilledMonster)
                                    {
                                        await NewMonster(abilityCastResult.PlayerAttackSummary.Monster, player);
                                    }
                                    else
                                    {
                                        // monster is still alive. Update everyone with the new monster
                                        var monsterUpdate = new Dictionary<string, MonsterInstance>
                                        {
                                            { "fetch_active_monster", abilityCastResult.PlayerAttackSummary.Monster }
                                        };
                                        string output = JsonSerializer.Serialize(monsterUpdate);
                                        var monsterUpdateByteArray = new ArraySegment<Byte>(Encoding.UTF8.GetBytes(output));
                                        await webSocketConnections.SendMessageToEveryOneElseWhoAreLoggedInAsync(pWebSocket, monsterUpdateByteArray);
                                    }
                                }
                            }
                            else
                            {
                                var response = new Dictionary<string, object>
                                {
                                    {
                                        "ability_cast_result", new Dictionary<string, object>
                                        {
                                            {"cast_success", abilityCastResult.CastSuccess},
                                            {"ability_text_result", abilityCastResult.Error}
                                        }
                                    }
                                };

                                var byteArray = new ArraySegment<Byte>(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(response)));
                                await pWebSocket.SendAsync(byteArray, pWebSocketReceiveResult.MessageType, pWebSocketReceiveResult.EndOfMessage, CancellationToken.None);
                            }
                        }
                    }
                }
            }

            if (!String.IsNullOrEmpty(error))
                await ReplyWithErrorMessage(pWebSocketReceiveResult, pWebSocket, error);
        }

        private static List<Tuple<object, string>> CreateValueList(Dictionary<string, JsonElement> pDict, List<string> pRequiredValues)
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

        async static private Task ReplyWithErrorMessage(WebSocketReceiveResult pWebSocketReceiveResult, WebSocket pWebSocket, string pError)
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
