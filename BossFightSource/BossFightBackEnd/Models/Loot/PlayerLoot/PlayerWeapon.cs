using System;
using System.Collections.Generic;
using System.Linq;
using BossFight.Extentions;
using MySqlConnector;
using System.Text.Json.Serialization;

namespace BossFight.Models
{
    public class PlayerWeapon : PersistableBase<PlayerWeapon>, IPersist<PlayerWeapon>, IPlayerLoot
    {
        [JsonIgnore]
        public override string TableName { get; set; } = nameof(PlayerWeapon);
        [JsonIgnore]
        public override string IdColumn { get; set; } = nameof(PlayerWeaponId);

        // Persisted on PlayerWeapon table
        [PersistProperty(true)]
        public int? PlayerWeaponId { get; set; }

        [PersistProperty]
        public int? PlayerId { get; set; }

        [PersistProperty]
        public int? WeaponId { get; set; }

        [PersistProperty]
        public int? Quantity { get; set; }

        // From other tables
        [JsonIgnore]
        public Player Player { get; set; }  // player that owns the weapon

        [JsonIgnore]
        public Weapon Weapon { get; set; }

        public string WeaponName { get => Weapon.LootName; }

        public PlayerWeapon () { }

        public static IPlayerLoot CreateInstance(int pLootId, Player pPlayer, int pQuantity)
        {
            return new PlayerWeapon{WeaponId=pLootId, PlayerId=pPlayer.PlayerId, Quantity=pQuantity};
        }

        public IPlayerLoot SearchForExsostingLootEntry(int? pId = null)
        {
            return FindOne(pId);
        }

        #region PersistableBase implementation

        public override IEnumerable<PlayerWeapon> BuildObjectFromReader(MySqlDataReader reader, MySqlConnection pConnection)
        {
            var result = new List<PlayerWeapon>();

            while (reader.Read())
            {
                var playerWeapon = new PlayerWeapon
                {
                    PlayerWeaponId = reader.GetInt(nameof(PlayerWeaponId)),
                    PlayerId = reader.GetInt(nameof(PlayerId)),
                    WeaponId = reader.GetInt(nameof(WeaponId)),
                    Quantity = reader.GetInt(nameof(Quantity)),
                };
                playerWeapon.Weapon = (Weapon)new Weapon().FindOne(playerWeapon.WeaponId);
                result.Add(playerWeapon);
            }

            return result;
        }

        public override string AdditionalSearchCriteria(PersistableBase<PlayerWeapon> pSearchObject, bool pStartWithAnd = true)
        {
            var pw = pSearchObject as PlayerWeapon;
            var additionalSearchCriteriaText = String.Empty;

            if (pw.PlayerId != null)
                additionalSearchCriteriaText += $"AND { nameof(PlayerId) } = { pw.PlayerId }\n";

            if (pw.WeaponId.HasValue)
                additionalSearchCriteriaText += $"AND { nameof(WeaponId) } = { pw.WeaponId }\n";

            return TrimAdditionalSearchCriteriaText(additionalSearchCriteriaText, pStartWithAnd);
        }

        #endregion PersistableBase implementation

        public void Sell()
        {
            Player.Gold += Weapon.GetSellPrice();
            Player.PlayerWeaponList = Player.PlayerWeaponList.Where(x => x.PlayerWeaponId != this.PlayerWeaponId);
            Player.Persist();
            Delete(PlayerWeaponId.Value);
        }
    }
}
