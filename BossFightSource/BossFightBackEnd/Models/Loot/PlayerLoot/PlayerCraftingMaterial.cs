using System;
using System.Collections.Generic;
using BossFight.Extentions;
using MySqlConnector;
using System.Text.Json.Serialization;

namespace BossFight.Models
{
    public class PlayerCraftingMaterial : PersistableBase<PlayerCraftingMaterial>, IPersist<PlayerCraftingMaterial>, IPlayerLoot
    {
        [JsonIgnore]
        public override string TableName { get; set; } = nameof(PlayerCraftingMaterial);
        [JsonIgnore]
        public override string IdColumn { get; set; } = nameof(PlayerCraftingMaterialId);

        [PersistProperty(true)]
        public int? PlayerCraftingMaterialId { get; set; }

        [PersistProperty]
        public int? PlayerId { get; set; }

        [PersistProperty]
        public int? CraftingMaterialId { get; set; }

        [PersistProperty]
        public int? Quantity { get; set; }

        // From other tables
        [JsonIgnore]
        public Player Player { get; set; }  // player that owns the CraftingMaterial

        [JsonIgnore]
        public CraftingMaterial CraftingMaterial { get; set; }

        public string CraftingMaterialName { get => CraftingMaterial.LootName; }

        public PlayerCraftingMaterial () { }

        public static IPlayerLoot CreateInstance(int pLootId, Player pPlayer, int pQuantity)
        {
            return new PlayerCraftingMaterial{CraftingMaterialId=pLootId, PlayerId=pPlayer.PlayerId, Quantity=pQuantity};
        }

        public IPlayerLoot SearchForExsostingLootEntry(int? pId = null)
        {
            return FindOne(pId);
        }

        #region PersistableBase implementation

        public override IEnumerable<PlayerCraftingMaterial> BuildObjectFromReader(MySqlDataReader reader, MySqlConnection pConnection)
        {
            var result = new List<PlayerCraftingMaterial>();

            while (reader.Read())
            {
                var playerCraftingMaterial = new PlayerCraftingMaterial
                {
                    PlayerCraftingMaterialId = reader.GetInt(nameof(PlayerCraftingMaterialId)),
                    PlayerId = reader.GetInt(nameof(PlayerId)),
                    CraftingMaterialId = reader.GetInt(nameof(CraftingMaterialId)),
                    Quantity = reader.GetInt(nameof(Quantity)),
                };
                playerCraftingMaterial.CraftingMaterial = (CraftingMaterial)new CraftingMaterial().FindOne(playerCraftingMaterial.CraftingMaterialId);
                result.Add(playerCraftingMaterial);
            }

            return result;
        }

        public override string AdditionalSearchCriteria(PersistableBase<PlayerCraftingMaterial> pSearchObject, bool pStartWithAnd = true)
        {
            var pw = pSearchObject as PlayerCraftingMaterial;
            var additionalSearchCriteriaText = String.Empty;

            if (pw.PlayerId.HasValue)
                additionalSearchCriteriaText += $"AND { nameof(PlayerId) } = { pw.PlayerId }\n";

            if (pw.CraftingMaterialId.HasValue)
                additionalSearchCriteriaText += $"AND { nameof(CraftingMaterialId) } = { pw.CraftingMaterialId }\n";

            return TrimAdditionalSearchCriteriaText(additionalSearchCriteriaText, pStartWithAnd);
        }

        #endregion PersistableBase implementation
    }
}
