using System;
using System.Collections.Generic;
using System.Linq;
using MySqlConnector;
using Newtonsoft.Json;

namespace BossFight.Models
{
    public class PlayerWeapon : PersistableBase, IPersist<PlayerWeapon>
    {
        [JsonIgnore]
        public override string TableName { get; set; } = nameof(PlayerWeapon);
        [JsonIgnore]
        public override string IdColumn { get; set; } = nameof(PlayerWeaponId);

        // Persisted on PlayerWeapon table
        public int PlayerWeaponId { get; set; }
        public int? PlayerId { get; set; }
        public int WeaponId { get; set; }

        // From other tables
        [JsonIgnore]
        public Player Player { get; set; }  // player that owns the weapon
        [JsonIgnore]
        public Weapon Weapon { get; set; }
        public string WeaponName { get => Weapon.LootName; }

        public PlayerWeapon () { }

        #region PersistableBase implementation

        public IEnumerable<PlayerWeapon> FindAll(int? id = null)
        {
            return _findAll(id).Cast<PlayerWeapon>();
        }

        public PlayerWeapon FindOne(int id)
        {
            return (PlayerWeapon)_findOne(id);
        }

        public override IEnumerable<PersistableBase> BuildObjectFromReader(MySqlDataReader reader)
        {
            var result = new List<PersistableBase>();

            while (reader.Read())
            {   
                var playerWeapon = new PlayerWeapon();
                playerWeapon.PlayerWeaponId = reader.GetInt(nameof(PlayerWeaponId));
                playerWeapon.PlayerId = reader.GetInt(nameof(PlayerId));
                playerWeapon.WeaponId = reader.GetInt(nameof(WeaponId));
                playerWeapon.Weapon = new Weapon().FindOne(playerWeapon.WeaponId);
                result.Add(playerWeapon);
            }

            return result;
        }

        public override string AdditionalSearchCriteria(PersistableBase pSearchObject, bool pStartWithAnd = true)
        {
            var pw = pSearchObject as PlayerWeapon;
            var additionalSearchCriteriaText = String.Empty;

            if (pw.PlayerId != null)
                additionalSearchCriteriaText += $"AND { nameof(PlayerId) } = { pw.PlayerId }\n";

            return pStartWithAnd ? additionalSearchCriteriaText : additionalSearchCriteriaText.Substring(4, additionalSearchCriteriaText.Length- 4);
        }

        public void Sell()
        {
            var earnedGold = Weapon.GetSellPrice();
            Player.Gold += earnedGold;
            Player.Persist(Player.PlayerId);
            //this.Delete(); //TODO DELETE DB FUNC
        }

        #endregion PersistableBase implementation
    }
}