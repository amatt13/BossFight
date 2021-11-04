using System;
using BossFight.Models.DB;

namespace BossFight.Models.Loot
{
    public abstract class LootItem : PersistableBase
    {
        public int LootId { get; set; }
        public string LootName { get; set; }
        public float LootDropChance { get; set; }
        public int Cost { get; set; }

        public LootItem() { }

        // public LootItem(AppDb db) : base(db)
        // {}

        public LootItem(int pLootId, string pLootName, float pLootDropChance, int pCost)
        {
            LootId = pLootId;
            LootName = pLootName;
            LootDropChance = pLootDropChance;
            Cost = pCost;
        }

        public void SetName(string pName)
        {
            LootName = pName;
        }

        public string GetName()
        {
            return LootName;
        }

        public int GetSellPrice()
        {
            return (int)Math.Ceiling((double)Cost / 4);
        }
    }
}