using System;
using System.Collections.Generic;
using System.Linq;
using BossFight.Models;
using BossFight.Extentions;
using BossFight.BossFightEnums;

namespace BossFight.Controllers
{
    public static class LootController
    {
        private static readonly Random _random = new();

        public static List<LootEntry> DistributeEarnedGoldForInvolvedPlayers(MonsterInstance pDeadMonster)
        {
            var goldResult = new List<LootEntry>();
            using var connection = GlobalConnection.GetNewOpenConnection();

            foreach(var trackerEntry in pDeadMonster.MonsterDamageTrackerList)
            {
                var goldEarned = (int)Math.Floor(1 + trackerEntry.DamageReceivedFromPlayer * pDeadMonster.Level * 0.50 / 10);
                if (pDeadMonster.IsBossMonster)
                    goldEarned = (int)Math.Floor(goldEarned * 1.2);

                goldResult.Add(new LootEntry(trackerEntry, goldEarned));

                var goldGainCmd = @"UPDATE Player p
SET p.Gold = p.Gold + @goldToAdd
WHERE p.PlayerId = @playerId
AND p.Gold + @goldToAdd <= 999999999999";

                try
                {
                    using var goldCmd = connection.CreateCommand();
                    goldCmd.CommandText = goldGainCmd;
                    goldCmd.Parameters.AddParameter(goldEarned.ToDbString(), "@goldToAdd");
                    goldCmd.Parameters.AddParameter(trackerEntry.PlayerId.ToDbString(), "@playerId");
                    goldCmd.ExecuteNonQuery();
                }
                catch (Exception)
                {
                    connection.Close();
                    throw;
                }
            }
            connection.Close();
            return goldResult;
        }

        public static List<ILootItem> GenerateLoot(MonsterInstance pMonster)
        {
            var rules = GetLootRulesForMonster(pMonster);

            var loot = new List<ILootItem>();
            foreach (var rule in rules)
            {
                var roll = _random.Next(0, 100);
                if (roll <= rule.DropChance * 100)
                {
                    var dropCount = _random.Next(rule.MinQuantity.Value, rule.MaxQuantity.Value);
                    for (int i = 0; i < dropCount; i++)
                    {
                        var lootItem = GetLootByType(rule.LootType, rule.LootId);
                        loot.Add(lootItem);
                    }
                }
            }

            return loot;
        }

        public static LootDropTracker DistributeDroppedLootForInvolvedPlayers(IEnumerable<ILootItem> pLoot, IEnumerable<MonsterDamageTracker> pMonsterDamageTrackerList)
        {
            // TODO: this does not include players that only played a supportive role 🤔
            // Overkill damage counts towards the player's contribution. We need a way to track the overkill damage on the MonsterDamageTracker
            var lootDropTracker = new LootDropTracker();
            var instanceId = pMonsterDamageTrackerList.First().MonsterInstanceId;

            List<Player> involvedPlayers = pMonsterDamageTrackerList
                .Where(x => x.DamageReceivedFromPlayer > 0)
                .Select(x => x.Player)
                .ToList();
            if (involvedPlayers.GroupBy(p => p.PlayerId).Select(g => g.Count()).Max() > 1)
            {
                throw new Exception($"Player is listed more than once for monster instanceId='{instanceId}'");
            }

            var totalDamageDealt = pMonsterDamageTrackerList.Sum(x => x.DamageReceivedFromPlayer);
            if (totalDamageDealt <= 0)
            {
                throw new Exception($"Total damage dealt was 0! instanceId='{instanceId}'");
            }

            foreach(var loot in pLoot)
            {
                var roll = _random.Next(1, totalDamageDealt);
                var progress = 0;
                Player winner = null;
                foreach(var entry in pMonsterDamageTrackerList)
                {
                    progress += entry.DamageReceivedFromPlayer;
                    if (roll <= progress)
                    {
                        winner = entry.Player;
                        break;
                    }
                }
                if (winner != null)
                {
                    lootDropTracker.Add(winner, loot);
                    AwardLootToPlayer(winner, loot);
                }
                else
                {
                    throw new Exception($"No winner found! roll={roll}, totalDamageDealt={totalDamageDealt}, progress={progress}");
                }

            }

            return lootDropTracker;
        }

        private static ILootItem GetLootByType(LootType pType, int pLootId)
        {
            return pType switch
            {
                LootType.WEAPON => new Weapon().FindOne(pLootId),
                // LootType.RING => new Ring().FindOne(pLootId),
                // LootType.CONSUMABLE => new Consumable().FindOne(pLootId),
                // LootType.ARMOR => new Armor().FindOne(pLootId),
                // LootType.CRAFTING_MATERIAL => new CraftingMaterial().FindOne(pLootId),
                _ => throw new ArgumentOutOfRangeException(nameof(pType), $"Unhandled loot type: '{pType}'")
            };
        }

        private static IPlayerLoot CreateLootInstanceByType(ILootItem pLootItem, Player pPlayer)
        {
            return pLootItem switch
            {
                Weapon => PlayerWeapon.CreateInstance(pLootItem.LootId.Value, pPlayer),
                _ => throw new ArgumentOutOfRangeException(nameof(pLootItem), $"Unhandled loot item: '{pLootItem}'")
            };
        }

        private static IEnumerable<LootRule> GetLootRulesForMonster(MonsterInstance pMonster)
        {
            var monsterTemplateId = pMonster.MonsterTemplateId;
            var monsterTypes = "(" + String.Join(", ", pMonster.MonsterTypeList.Select(x => (int)x)).ToString() + ")";
            var monsterLevel = pMonster.Level;
            var isBoss = pMonster.MonsterTemplate.BossMonster;

            using var connection = GlobalConnection.GetNewOpenConnection();
            using var rulesCmd = connection.CreateCommand();
            rulesCmd.CommandText = "SELECT * FROM MonsterLootRule WHERE " +
                "(MonsterTemplateId IS NULL OR MonsterTemplateId = @templateId) AND " +
                $"(MonsterTypeId IS NULL OR MonsterTypeId  IN {monsterTypes}) AND " +
                "(MinLevel IS NULL OR @level >= MinLevel) AND " +
                "(MaxLevel IS NULL OR @level <= MaxLevel) AND " +
                "(BossOnly IS NULL or @isBoss = BossOnly)";
            rulesCmd.Parameters.AddParameter(monsterTemplateId, "@templateId");
            //rulesCmd.Parameters.Add(new MySqlParameter("@monsterTypes", monsterTypes));
            rulesCmd.Parameters.AddParameter(monsterLevel, "@level");
            rulesCmd.Parameters.AddParameter(isBoss, "@isBoss");

            var reader = rulesCmd.ExecuteReader();
            var rules = new LootRule().BuildObjectFromReader(reader, connection);
            reader.Close();

            return rules;
        }

        private static void AwardLootToPlayer(Player pPlayer, ILootItem pLootItem)
        {
            var loot = CreateLootInstanceByType(pLootItem, pPlayer);
            loot.Persist();
        }
    }
}
