using System;

namespace BossFight.Models
{
    public abstract class LootItem<T> : PersistableBase<LootItem<T>>, ILootItem
    {
        [PersistProperty(true)]
        public int? LootId { get; set; }

        [PersistProperty]
        public string LootName { get; set; }

        [PersistProperty]
        public int Cost { get; set; }

        public LootItem() { }

        public LootItem(int pLootId, string pLootName, int pCost)
        {
            LootId = pLootId;
            LootName = pLootName;
            Cost = pCost;
        }

        public int GetSellPrice()
        {
            return (int)Math.Ceiling((double)Cost / 4);
        }
    }
}
