namespace BossFight.Models
{
    public interface IPlayerLoot
    {
        Player Player { get; set; }
        int? PlayerId { get; set; }
        public int? Quantity { get; set; }

        static abstract IPlayerLoot CreateInstance(int pLootId, Player pPlayer, int pQuantity);
        void Persist();
        IPlayerLoot SearchForExsostingLootEntry(int? pId = null);
    }
}
