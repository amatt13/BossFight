using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;
using BossFight.BossFightEnums;
using BossFight.Models;

namespace BossFight.Controllers.SocketMessageHandlers
{
    public static class MonsterHandler
    {
        // takes: no data
        // returns: monster
        public static async Task FetchActiveMonster(Dictionary<string, JsonElement> _, WebSocketReceiveResult pWebSocketReceiveResult, WebSocket pWebSocket)
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

        // takes: player_id: "int", monster_instance_id: "int", vote: "int"
        public static async Task VoteForMonsterTier(Dictionary<string, JsonElement> pJsonParameters, WebSocketReceiveResult pWebSocketReceiveResult, WebSocket pWebSocket)
        {
            var requiredValues = SocketMessageHandler.CreateValueList(pJsonParameters, new List<string> { "player_id", "monster_instance_id", "vote" });

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
                await SocketMessageHandler.ReplyWithErrorMessage(pWebSocketReceiveResult, pWebSocket, error);
        }

        public static async Task FetchMonsterVotesTotals(Dictionary<string, JsonElement> _, WebSocketReceiveResult pWebSocketReceiveResult, WebSocket pWebSocket)
        {
            var votesTotal = MonsterTierVoteUpdater.CountMonsterTierVotesTotalForActiveMonster();
            var response = new Dictionary<string, MonsterTierVoteUpdater.MonsterTierVotesTotal>
            {
                { "monster_tier_votes_total", votesTotal }
            };

            var byteArray = new ArraySegment<Byte>(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(response)));
            await pWebSocket.SendAsync(byteArray, pWebSocketReceiveResult.MessageType, pWebSocketReceiveResult.EndOfMessage, CancellationToken.None);
        }

        // takes: player_id: "int"
        // returns: PlayerAttackSummary
        public static async Task PlayerAttackMonsterWithEquippedWeapon(Dictionary<string, JsonElement> pJsonParameters, WebSocketReceiveResult pWebSocketReceiveResult, WebSocket pWebSocket)
        {
            var requiredValues = SocketMessageHandler.CreateValueList(pJsonParameters, new List<string> { "player_id" });
            if (RequestValidator.AllValuesAreFilled(requiredValues, out string error)
                && RequestValidator.PlayerExists(pJsonParameters["player_id"].GetInt32(), out Player player, out error)
                && RequestValidator.PlayerCanAttackMonsterWithEquippedWeapon(pJsonParameters["player_id"].GetInt32(), out error))
            {
                var webSocketConnections = WebSocketConnections.GetInstance();

                var monster = new MonsterInstance { Active = true }.FindOne();
                var summary = DamageDealer.PlayerAttackMonster(player, monster, AttackType.WEAPON_SWING, true);
                var response = new Dictionary<string, PlayerAttackSummary>
                {
                    { "player_attacked_monster_with_weapon", summary }
                };

                var byteArray = new ArraySegment<Byte>(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(response)));
                await pWebSocket.SendAsync(byteArray, pWebSocketReceiveResult.MessageType, pWebSocketReceiveResult.EndOfMessage, CancellationToken.None);

                if (summary.PlayerKilledMonster)
                {
                    monster = new MonsterInstance { Active = true }.FindOne();
                    await MonsterSpawner.NewMonster(monster, player);
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
                await SocketMessageHandler.ReplyWithErrorMessage(pWebSocketReceiveResult, pWebSocket, error);
        }
    }
}
