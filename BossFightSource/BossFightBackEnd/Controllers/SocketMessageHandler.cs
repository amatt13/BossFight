using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using BossFight.BossFightBackEnd.BossFightLogger;
using BossFight.Controllers.SocketMessageHandlers;
using BossFight.Models;

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
            methodDictionary[nameof(AbilityHandler.CastAbility)] = AbilityHandler.CastAbility;
            methodDictionary[nameof(ChatHandler.SendChatMessage)] = ChatHandler.SendChatMessage;
            methodDictionary[nameof(ChatHandler.FetchMostRecentMessages)] = ChatHandler.FetchMostRecentMessages;
            methodDictionary[nameof(MonsterHandler.FetchActiveMonster)] = MonsterHandler.FetchActiveMonster;
            methodDictionary[nameof(MonsterHandler.PlayerAttackMonsterWithEquippedWeapon)] = MonsterHandler.PlayerAttackMonsterWithEquippedWeapon;
            methodDictionary[nameof(MonsterHandler.VoteForMonsterTier)] = MonsterHandler.VoteForMonsterTier;
            methodDictionary[nameof(MonsterHandler.FetchMonsterVotesTotals)] = MonsterHandler.FetchMonsterVotesTotals;
            methodDictionary[nameof(PlayerHandler.FetchPlayer)] = PlayerHandler.FetchPlayer;
            methodDictionary[nameof(PlayerHandler.SignIn)] = PlayerHandler.SignIn;
            methodDictionary[nameof(PlayerHandler.ChangePlayerClass)] = PlayerHandler.ChangePlayerClass;
            methodDictionary[nameof(PlayerHandler.EquipWeapon)] = PlayerHandler.EquipWeapon;
            methodDictionary[nameof(PlayerHandler.GetUnlockedClassesForPlayer)] = PlayerHandler.GetUnlockedClassesForPlayer;
            methodDictionary[nameof(ShopHandler.SellWeapon)] = ShopHandler.SellWeapon;
            methodDictionary[nameof(ShopHandler.GetShopForPlayer)] = ShopHandler.GetShopForPlayer;
            methodDictionary[nameof(ShopHandler.BuyPlayerClass)] = ShopHandler.BuyPlayerClass;
            //methodDictionary[nameof(MyHandler.Example)] = MyHandler.Example;
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
                try
                {
                    await method(dataJsonDictionary, pWebSocketReceiveResult, pWebSocket);
                }
                catch (WebSocketException e)
                {
                    var error = (WebSocketError)e.ErrorCode;
                    if (error == WebSocketError.InvalidState)
                    {
                        var errorMessage = e.Message;
                        _logger.LogWarning("Invalid state for socket: {errorMessage}", errorMessage);

                        await pWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Invalid state", CancellationToken.None);
                        var webSocketConnections = WebSocketConnections.GetInstance();
                        webSocketConnections.RemoveConnection(pWebSocket);
                        _logger.LogInformation("Websocket removed");
                    }
                    else
                    {
                        _logger.LogError("Unexpected WebSocketError '{error}'", error);
                        throw;
                    }
                }
                catch (Exception e)
                {
                    var message = e.Message;
                    _logger.LogError("Unexpected exception '{error}'", message);
                    throw;
                }
            }
            else
            {
                throw new ArgumentException($"Method '{handler}' does not exist!\n");
            }
        }

        public static List<Tuple<object, string>> CreateValueList(Dictionary<string, JsonElement> pDict, List<string> pRequiredValues)
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

        async static public Task ReplyWithErrorMessage(WebSocketReceiveResult pWebSocketReceiveResult, WebSocket pWebSocket, string pError)
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
