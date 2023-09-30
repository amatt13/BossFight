namespace BossFight.Models
{
    public class DamageTrackerEntry
    {
        private int _playerId { get; set; }
        private string _playerName { get; set; }
        private int _duration { get; set; }
        private int _damage { get; set; }

        public DamageTrackerEntry(int player_id, string player_name, int duration, int damage)
        {
            _playerId = player_id;
            _playerName = player_name;
            _duration = duration;
            _damage = damage;
        }

        public void SubtractTurn(int n = 1)
        {
            _duration -= n;
        }

        public int GetDuration()
        {
            return _duration;
        }

        public int GetPlayerId()
        {
            return _playerId;
        }

        public string GetPlayerName()
        {
            return _playerName;
        }

        public int GetDamage()
        {
            return _damage;
        }
    }
}
