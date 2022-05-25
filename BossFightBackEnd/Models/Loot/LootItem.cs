using System;

namespace BossFight.Models.Loot
{
    public interface iLootItem
    { 
        int? LootId { get; set; }
        string LootName { get; set; }

        int GetSellPrice();
    }

    public abstract class LootItem<T> : PersistableBase<T>, iLootItem
    where T: PersistableBase<T>
    {
        [PersistProperty(true)]
        public int? LootId { get; set; }

        [PersistProperty]
        public string LootName { get; set; }

        [PersistProperty]
        public float LootDropChance { get; set; }

        [PersistProperty]
        public int Cost { get; set; }

        public LootItem() { }

        public LootItem(int pLootId, string pLootName, float pLootDropChance, int pCost)
        {
            LootId = pLootId;
            LootName = pLootName;
            LootDropChance = pLootDropChance;
            Cost = pCost;
        }

        public int GetSellPrice()
        {
            return (int)Math.Ceiling((double)Cost / 4);
        }
    }
}