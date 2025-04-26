using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;
using BossFight.Models;
using BossFight.Extentions;

namespace BossFight.Controllers
{
    public class MonsterSpawner
    {
        private static readonly object _spawnNewMonsterLock = new();
        public static readonly int MAX_MONSTER_TIER = 5;

        public static async Task NewMonster(MonsterInstance pCurrentMonster, Player pPLayer)
        {
            var newMonsterInstance = SpawnNewMonster();
            if (newMonsterInstance != null)
            {
                var goldResult = LootController.DistributeEarnedGoldForInvolvedPlayers(pCurrentMonster);
                var loot = LootController.GenerateLoot(pCurrentMonster);
                var lootResult = LootController.DistributeDroppedLootForInvolvedPlayers(loot, pCurrentMonster.MonsterDamageTrackerList);
                var monsterDamageInfo = BuildMonsterDamageInfoText(pCurrentMonster, goldResult);
                var monsterWasKilledMessage = $"{(pCurrentMonster.IsBossMonster ? "BOSS KILL\n" : String.Empty)}{pPLayer.Name} killed {pCurrentMonster.Name}!";

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
                var webSocketConnections = WebSocketConnections.GetInstance();
                foreach (var ws in webSocketConnections.GetAllOpenConnections())
                {
                    await ws.WebSocket.SendAsync(monsterByteArray, WebSocketMessageType.Text, true, CancellationToken.None);
                    await ws.WebSocket.SendAsync(voteByteArray, WebSocketMessageType.Text, true, CancellationToken.None);
                }

                foreach (var player in lootResult)
                {
                    var lootDropsForPlayer = lootResult[player];
                    var playerConnection = webSocketConnections.GetConnection(player);
                    if (playerConnection != null)
                    {
                        var sb = new StringBuilder();
                        foreach(var lootEntry in lootDropsForPlayer)
                        {
                            var item = lootEntry.Item1;
                            var quantity = lootEntry.Item2;
                            var msg = item.LootName;
                            if (quantity > 1)
                                msg += " x" + quantity;
                            sb.AppendLine(msg);
                        }
                        var plural = lootDropsForPlayer.Count > 1 ? "s" : "";
                        var lootMessage = $"You have obtained the following item{plural}:\n" + sb.ToString();
                        var lootObtainedMessage = new Dictionary<string, object>
                        {
                            { "loot_obtained_message", new Dictionary<string, object>
                                {
                                    { "lootMessage", lootMessage },
                                }
                            }
                        };
                        var lootObtainedByteArray = new ArraySegment<Byte>(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(lootObtainedMessage)));
                        await playerConnection.WebSocket.SendAsync(lootObtainedByteArray, WebSocketMessageType.Text, true, CancellationToken.None);
                    }
                }
            }
        }

        private static string BuildMonsterDamageInfoText(MonsterInstance pMonster, List<LootEntry> pGoldResultList)
        {
            var longestPlayerName = pMonster.MonsterDamageTrackerList.Max(x => x.Player.Name.Length);
            var longestDamageNumber = pMonster.MonsterDamageTrackerList.Max(x => x.DamageReceivedFromPlayer.ToString().Length);
            var longestGoldEarnedNumber = pGoldResultList.Max(x => x.GoldEarned.ToString().Length);

            var playerNameColumn = "Player";
            playerNameColumn = playerNameColumn.PadRight(longestPlayerName);

            var damageDealtColumn = "Damage Dealt";
            damageDealtColumn = damageDealtColumn.PadRight(longestDamageNumber);

            var goldEarnedColumn = "Gold Earned";
            goldEarnedColumn = goldEarnedColumn.PadRight(longestGoldEarnedNumber);

            var header = $"{playerNameColumn} {damageDealtColumn} {goldEarnedColumn}";
            var footer = "".PadRight(header.Length, '_');

            var rows = new StringBuilder();
            rows.AppendLine(header);
            rows.AppendLine(string.Join("\n", pMonster.MonsterDamageTrackerList
                .OrderBy(x => x.DamageReceivedFromPlayer)
                .Select(x => $"{x.Player.Name.PadRight(playerNameColumn.Length)} {x.DamageReceivedFromPlayer.ToString().PadRight(damageDealtColumn.Length)} {pGoldResultList.Where(gr => gr.PlayerId == x.PlayerId).FirstOrDefault(new LootEntry{GoldEarned = -1}).GoldEarned.ToString().PadRight(goldEarnedColumn.Length)}")));
            rows.AppendLine(footer);


            var monsterDamageInfo = rows.ToString();

            return monsterDamageInfo;
        }

        private static MonsterInstance SpawnNewMonster()
        {
            MonsterInstance newMonster = null;
            lock(_spawnNewMonsterLock)
            {
                var currentMonster = new MonsterInstance{ Active = true }.FindOne(null);
                // make sure the monster is dead before we spawn a new one
                if (currentMonster != null && currentMonster.IsDead())
                {
                    var nextMonsterTier = NextMonsterTier(currentMonster.MonsterTemplate.Tier.GetValueOrDefault(1));
                    var randomMonsterTemplate = new MonsterTemplate{ SearchRandomTopOne = true, Tier = nextMonsterTier, BossMonster = false }.FindOne(null);
                    if (randomMonsterTemplate != null)
                    {
                        newMonster = new MonsterInstance(randomMonsterTemplate)
                        {
                            Level = randomMonsterTemplate.Tier.Value * 5 + new Random().Next(1, 6),
                            Active = true
                        };
                        newMonster.CalcHealth();
                        newMonster.Persist();

                        currentMonster.Active = false;
                        currentMonster.ActiveEffects.ForEach(effect => effect.Delete(effect.EffectId.Value));
                        currentMonster.Persist();

                        newMonster = newMonster.FindOne(null);
                    }
                }
            }
            return newMonster;
        }

        ///<summary>
        /// Finds the next monster tier, based on the players's votes
        ///</summary>
        private static int NextMonsterTier(int pCurrentMonsterTier)
        {
            var nextTier = pCurrentMonsterTier;

            var sql = $@"SELECT SUM(mtv.Vote)
FROM MonsterInstance mi
JOIN MonsterTierVote mtv
	ON mtv.MonsterInstanceId = mi.MonsterInstanceId
WHERE mi.Active  = 1";

            var votes = GlobalConnection.SingleValue<decimal?>(sql).GetValueOrDefault(0);

            if (votes > 0 )
            {
                nextTier += 1;
            }
            else if (votes < 0)
                nextTier -= 1;

            if (nextTier < 0)
            {
                nextTier = 0;
            }
            else if (nextTier > MAX_MONSTER_TIER)
                nextTier = MAX_MONSTER_TIER;

            return nextTier;
        }
    }
}
