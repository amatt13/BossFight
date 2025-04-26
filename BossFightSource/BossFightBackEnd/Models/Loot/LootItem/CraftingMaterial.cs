using System;
using System.Collections.Generic;
using MySqlConnector;
using System.Text.Json.Serialization;

namespace BossFight.Models
{
    public class CraftingMaterial : LootItem<CraftingMaterial>
    {
        [JsonIgnore]
        public override string TableName { get; set; } = nameof(CraftingMaterial);

        [JsonIgnore]
        public override string IdColumn { get; set; } = "CraftingMaterialId";

        public CraftingMaterial() { }

        public CraftingMaterial(int pCraftingMaterialId, string pName, int pCost = 0)
            : base(pCraftingMaterialId, pName, pCost)
        { }

        public override IEnumerable<CraftingMaterial> BuildObjectFromReader(MySqlDataReader reader, MySqlConnection pConnection)
        {
            var result = new List<CraftingMaterial>();

            while (reader.Read())
            {
                var craftingMaterial = new CraftingMaterial
                {
                    LootId = reader.GetInt32("CraftingMaterialId"),
                    LootName = reader.GetString("Name"),
                    Cost = reader.GetInt32(nameof(Cost))
                };
                result.Add(craftingMaterial);
            }

            return result;
        }

        public override string ToString()
        {
            return LootName;
        }
    }
}
