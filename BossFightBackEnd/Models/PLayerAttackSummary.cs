namespace BossFight.Models
{
    public class PlayerAttackSummary
    {
        public Player Player { get; set; }
        public MonsterInstance Monster { get; set; }
        public int PlayerTotalDamage { get; set; }
        public bool PlayerCrit { get; set; }
        public bool MonsterCrit { get; set; }
        public int PlayerExtraDamageFromBuffs { get; set; }
        public int PlayerXpEarned { get; set; }
        public string MonsterAffectedByDots { get; set; }
        public string MonsterRetaliateMessage { get; set; }

        public PlayerAttackSummary(Player pPlayer, MonsterInstance pMonster)
        {
            Player = pPlayer;
            Monster = pMonster;
        }
    }
}
