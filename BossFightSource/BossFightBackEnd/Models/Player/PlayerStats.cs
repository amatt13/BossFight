namespace BossFight.Models
{
    public class PlayerStats
    {
        private readonly Player _player;

        public PlayerStats(Player player)
        {
            _player = player;
        }

        public int GetMaxHp()
        {
            return _player.PlayerPlayerClass.MaxHp;
        }

        public int GetMaxMana()
        {
            return _player.PlayerPlayerClass.MaxMana;
        }

        public int GetLevel()
        {
            return _player.PlayerPlayerClass.Level;
        }

        public void RestoreAllHealthAndMana()
        {
            _player.Hp = GetMaxHp();
            _player.Mana = GetMaxMana();
        }
    }
}
