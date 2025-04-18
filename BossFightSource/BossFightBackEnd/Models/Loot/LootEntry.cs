namespace BossFight.Models.Loot
{
    public class LootEntry
    {
        public int PlayerId { get; set; }
        public int DamageDealtByPlayer { get; set; }
        public int GoldEarned { get; set; }

        public LootEntry() {}

        public LootEntry(int pPlayerId, int pDamageDealtByPlayer, int pGoldEarned)
        {
            PlayerId = pPlayerId;
            DamageDealtByPlayer = pDamageDealtByPlayer;
            GoldEarned = pGoldEarned;
        }

        public LootEntry(MonsterDamageTracker pMonsterDamageTracker, int pGoldEarned)
        {
            PlayerId = pMonsterDamageTracker.PlayerId;
            DamageDealtByPlayer = pMonsterDamageTracker.DamageReceivedFromPlayer;
            GoldEarned = pGoldEarned;
        }
    }
}
