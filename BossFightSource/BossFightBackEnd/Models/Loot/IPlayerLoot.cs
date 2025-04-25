namespace BossFight.Models
{
    public interface IPlayerLoot
    {
        Player Player { get; set; }
        int? PlayerId { get; set; }

        static abstract IPlayerLoot CreateInstance(int pLootId, Player pPlayer);
        void Persist();
    }
}
