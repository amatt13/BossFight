using System;

namespace BossFight.Models
{
    public class MonsterKilledEventArgs : EventArgs
    {
        public MonsterInstance DeadMonster { get; set; }
        public Player Killer { get; set; }

        public MonsterKilledEventArgs(MonsterInstance pDeadMonster, Player pKiller)
        {
            DeadMonster = pDeadMonster;
            Killer = pKiller;
        }
    }
}
