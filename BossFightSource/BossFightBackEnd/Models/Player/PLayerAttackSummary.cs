using System;
using System.Text;
using System.Threading.Channels;

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
        private StringBuilder _monsterRetaliateMessageBuilder = new();
        private static char[] _trim = new char[] { '\r', '\n' };
        public string MonsterRetaliateMessage
        {
            get
            {
                return String.Join("\n", _monsterRetaliateMessageBuilder).TrimEnd(_trim);;
            }
        }
        public bool PlayerKilledMonster { get; set; }
        public int MonsterTotalDamage {get; set;}

        public PlayerAttackSummary(Player pPlayer, MonsterInstance pMonster)
        {
            Player = pPlayer;
            Monster = pMonster;
        }

        public string AddMonsterRetaliateMessage(StringBuilder pMessages)
        {
            var text = String.Join("\n", pMessages);
            AddMonsterRetaliateMessage(text);
            return MonsterRetaliateMessage;
        }

        public string AddMonsterRetaliateMessage(string pMessage)
        {
            _monsterRetaliateMessageBuilder.AppendLine(pMessage);
            return MonsterRetaliateMessage;
        }
    }
}
