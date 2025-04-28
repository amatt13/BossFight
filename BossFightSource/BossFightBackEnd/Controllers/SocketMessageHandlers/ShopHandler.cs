using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;
using BossFight.Models;

namespace BossFight.Controllers.SocketMessageHandlers
{
    public static class ShopHandler
    {
        // takes: player_id: "int", weapon_id: "int"
        // returns: dict => gold, weapons
        public static async Task SellWeapon(Dictionary<string, JsonElement> pJsonParameters, WebSocketReceiveResult pWebSocketReceiveResult, WebSocket pWebSocket)
        {
            var requiredValues = SocketMessageHandler.CreateValueList(pJsonParameters, new List<string> { "player_id", "weapon_id" });

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
                await SocketMessageHandler.ReplyWithErrorMessage(pWebSocketReceiveResult, pWebSocket, error);
        }

        public static async Task GetPlayerClassShopForPlayer(Dictionary<string, JsonElement> pJsonParameters, WebSocketReceiveResult pWebSocketReceiveResult, WebSocket pWebSocket)
        {
            var requiredValues = SocketMessageHandler.CreateValueList(pJsonParameters, new List<string> { "player_id" });

            if (RequestValidator.AllValuesAreFilled(requiredValues, out string error))
            {
                var playerId = pJsonParameters["player_id"].GetInt32();
                if (RequestValidator.PlayerExists(playerId, out Player player, out error))
                {
                    var shop = ShopController.GetPlayerClassShopForPlayer(player);
                    var response = new Dictionary<string, object>
                    {
                        { "shopMenu", shop }
                    };

                    var byteArray = new ArraySegment<Byte>(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(response)));
                    await pWebSocket.SendAsync(byteArray, pWebSocketReceiveResult.MessageType, pWebSocketReceiveResult.EndOfMessage, CancellationToken.None);
                }
            }

            if (!String.IsNullOrEmpty(error))
                await SocketMessageHandler.ReplyWithErrorMessage(pWebSocketReceiveResult, pWebSocket, error);
        }

        // takes: player_id: "int", player_class_id: "int"
        // returns: {sucess: bool, updated_player}
        public static async Task BuyPlayerClass(Dictionary<string, JsonElement> pJsonParameters, WebSocketReceiveResult pWebSocketReceiveResult, WebSocket pWebSocket)
        {
            var requiredValues = SocketMessageHandler.CreateValueList(pJsonParameters, new List<string> { "player_id", "player_class_id" });

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
                await SocketMessageHandler.ReplyWithErrorMessage(pWebSocketReceiveResult, pWebSocket, error);
        }
    }
}
