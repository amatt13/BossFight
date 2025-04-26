namespace BossFight.Models
{

    public interface ILootItem
    {
        int? LootId { get; set; }
        string LootName { get; set; }

        int GetSellPrice();
    }
}
