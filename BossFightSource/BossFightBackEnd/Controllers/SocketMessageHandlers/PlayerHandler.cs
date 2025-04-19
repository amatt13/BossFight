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
    public static class PlayerHandler
    {
        // takes: player_id: "int"
        // returns: player
        public static async Task FetchPlayer(Dictionary<string, JsonElement> pJsonParameters, WebSocketReceiveResult pWebSocketReceiveResult, WebSocket pWebSocket)
        {
            var requiredValues = SocketMessageHandler.CreateValueList(pJsonParameters, new List<string>{"player_id", "cake"});

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
                await SocketMessageHandler.ReplyWithErrorMessage(pWebSocketReceiveResult, pWebSocket, error);
        }

        // takes: userName: "string", password: "string"
        // returns: player & MonsterTierVote
        public static async Task SignIn(Dictionary<string, JsonElement> pJsonParameters, WebSocketReceiveResult pWebSocketReceiveResult, WebSocket pWebSocket)
        {
            var requiredValues = SocketMessageHandler.CreateValueList(pJsonParameters, new List<string> { "userName", "password" });
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
                await SocketMessageHandler.ReplyWithErrorMessage(pWebSocketReceiveResult, pWebSocket, error);
        }

        // takes: player_id: "int", player_class_id: "int", preffered_body_type: "string"
        public static async Task ChangePlayerClass(Dictionary<string, JsonElement> pJsonParameters, WebSocketReceiveResult pWebSocketReceiveResult, WebSocket pWebSocket)
        {
            var requiredValues = SocketMessageHandler.CreateValueList(pJsonParameters, new List<string> { "player_id", "player_class_id" });

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
                await SocketMessageHandler.ReplyWithErrorMessage(pWebSocketReceiveResult, pWebSocket, error);
        }

        // takes: player_id: "int", weapon_id: "int
        // returns: weapon
        public static async Task EquipWeapon(Dictionary<string, JsonElement> pJsonParameters, WebSocketReceiveResult pWebSocketReceiveResult, WebSocket pWebSocket)
        {
            var requiredValues = SocketMessageHandler.CreateValueList(pJsonParameters, new List<string> { "player_id", "weapon_id" });
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
                await SocketMessageHandler.ReplyWithErrorMessage(pWebSocketReceiveResult, pWebSocket, error);
        }

        // takes: player_id: "int"
        // return List of PlayerPlayerClass
        public static async Task GetUnlockedClassesForPlayer(Dictionary<string, JsonElement> pJsonParameters, WebSocketReceiveResult pWebSocketReceiveResult, WebSocket pWebSocket)
        {
            var requiredValues = SocketMessageHandler.CreateValueList(pJsonParameters, new List<string> { "player_id" });

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
                await SocketMessageHandler.ReplyWithErrorMessage(pWebSocketReceiveResult, pWebSocket, error);
        }
    }
}
