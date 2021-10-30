namespace BossFight.Models.Loot
{
    public class LootEntry
    {
        public int PlayerId { get; set; }
        public int DamageDealtByPlayer { get; set; }
        public double RelativeDamageDealtByPlayer { get; set; }
        public int GoldEarned { get; set; }
        public Player Player { get; set; }

        public LootEntry(int pPlayerId, int pDamageDealtByPlayer, int pGoldEarned)
        {
            PlayerId = pPlayerId;
            DamageDealtByPlayer = pDamageDealtByPlayer;
            RelativeDamageDealtByPlayer = 0.0d;
            GoldEarned = pGoldEarned;
            Player = new Player("NA", -1);
        }

        public void SetPlayer(Player pPlayer)
        {
            Player = pPlayer;
        }
    }
}
