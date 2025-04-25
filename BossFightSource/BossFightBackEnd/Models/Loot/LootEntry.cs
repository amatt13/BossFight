namespace BossFight.Models
{
    public class LootEntry
    {
        public int PlayerId { get; set; }
        public int GoldEarned { get; set; }

        public LootEntry() {}

        public LootEntry(int pPlayerId, int pGoldEarned)
        {
            PlayerId = pPlayerId;
            GoldEarned = pGoldEarned;
        }

        public LootEntry(MonsterDamageTracker pMonsterDamageTracker, int pGoldEarned)
        {
            PlayerId = pMonsterDamageTracker.PlayerId;
            GoldEarned = pGoldEarned;
        }
    }
}
