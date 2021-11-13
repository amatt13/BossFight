namespace BossFight.Models
{
    public class DamageTrackerEntry
    {
        private int _player_id { get; set; }
        private string _player_name { get; set; }
        private int _duration { get; set; }
        private int _damage { get; set; }

        public DamageTrackerEntry(int player_id, string player_name, int duration, int damage)
        {
            _player_id = player_id;
            _player_name = player_name;
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
            return _player_id;
        }

        public string GetPlayerName()
        {
            return _player_name;
        }

        public int GetDamage()
        {
            return _damage;
        }
    }
}