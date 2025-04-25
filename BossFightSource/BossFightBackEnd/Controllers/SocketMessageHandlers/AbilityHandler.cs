using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;
using BossFight.Models;

namespace BossFight.Controllers.SocketMessageHandlers
{
    public static class AbilityHandler
    {
        // takes: player_id: "int", ability_name: "string", target_id: "int"
        public static async Task CastAbility(Dictionary<string, JsonElement> pJsonParameters, WebSocketReceiveResult pWebSocketReceiveResult, WebSocket pWebSocket)
        {
            var requiredValues = SocketMessageHandler.CreateValueList(pJsonParameters, new List<string> { "player_id", "ability_name", "target_id" });

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
                                    if (bfws != null)
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
                                        await MonsterSpawner.NewMonster(abilityCastResult.PlayerAttackSummary.Monster, player);
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
                await SocketMessageHandler.ReplyWithErrorMessage(pWebSocketReceiveResult, pWebSocket, error);
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
    }
}
