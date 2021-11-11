using System;
using System.Data;
using BossFight.Models;
using MySqlConnector;

namespace BossFight.Controllers
{
    public static class RequestValidator
    {
        public static bool PlayerCanSellWeapon(string pPlayerId, string pWeaponId, out string pError)
        {
            bool playerCanSellWeapon = false;
            var parsed = int.TryParse(pPlayerId, out int playerId);
            parsed &= int.TryParse(pWeaponId, out int weaponID);

            if (parsed)
            {
                using var connection = GlobalConnection.GetNewOpenConnection();
                using var cmd = connection.CreateCommand();

                /*
                return true if:
                    player owns weapon (and it is not equipped)
                    player owns at least two coppies of weapon and it is equipped
                */
                var sql = @$"SELECT
CASE 
	WHEN p.WeaponId != pw.WeaponId 
		THEN TRUE
	WHEN COUNT(pw.WeaponId) > 1 
		THEN TRUE
	ELSE FALSE
END AS CanSell
FROM { nameof(PlayerWeapon) } pw
JOIN { nameof(Player) } p 
	ON p.PlayerId = pw.PlayerId 
WHERE pw.PlayerId = @player_id
AND pw.WeaponId = @weapon_id
GROUP BY pw.WeaponId";
                cmd.CommandText = sql;
                cmd.Parameters.Add(new MySqlParameter
                {
                    ParameterName = "@player_id",
                    DbType = DbType.Int32,
                    Value = playerId.ToString(),
                });
                cmd.Parameters.Add(new MySqlParameter
                {
                    ParameterName = "@weapon_id",
                    DbType = DbType.Int32,
                    Value = weaponID.ToString(),
                });
                var reader = cmd.ExecuteReader();
                reader.Read();
                playerCanSellWeapon = reader.GetBooleanNullable("CanSell").GetValueOrDefault(false);
                reader.Close();
                connection.Close();
                if (playerCanSellWeapon)
                {
                    pError = String.Empty;
                }
                else
                    pError = "Could not sell weapon. Make sure it is not your only copy and you haven't equipped it.";
            }
            else
                pError = $"player ({ pPlayerId }) or weapon id ({ pWeaponId }) is not a valid id";

            return playerCanSellWeapon;
        }

        public static bool PlayerCanEquipWeapon(string pPlayerId, string pWeaponId, out string pError)
        {
            bool playerCanEquipWeapon = false;
            var parsed = int.TryParse(pPlayerId, out int playerId);
            parsed &= int.TryParse(pWeaponId, out int weaponID);

            if (parsed)
            {
                using var connection = GlobalConnection.GetNewOpenConnection();
                using var cmd = connection.CreateCommand();

                /*
                return true if:
                    player owns weapon (and it is not equipped)
                */
                var sql = @$"SELECT
CASE 
	WHEN p.WeaponId != pw.WeaponId 
		THEN TRUE
	ELSE FALSE
END AS CanEquip
FROM { nameof(PlayerWeapon) } pw
JOIN { nameof(Player) } p 
	ON p.PlayerId = pw.PlayerId 
WHERE pw.PlayerId = @player_id
AND pw.WeaponId = @weapon_id
GROUP BY pw.WeaponId";
                cmd.CommandText = sql;
                cmd.Parameters.Add(new MySqlParameter
                {
                    ParameterName = "@player_id",
                    DbType = DbType.Int32,
                    Value = playerId.ToString(),
                });
                cmd.Parameters.Add(new MySqlParameter
                {
                    ParameterName = "@weapon_id",
                    DbType = DbType.Int32,
                    Value = weaponID.ToString(),
                });
                var reader = cmd.ExecuteReader();
                reader.Read();
                playerCanEquipWeapon = reader.GetBooleanNullable("CanEquip").GetValueOrDefault(false);
                reader.Close();
                connection.Close();
                if (playerCanEquipWeapon)
                {
                    pError = String.Empty;
                }
                else
                    pError = "Could not equip weapon. Make sure you haven't already equipped it.";
            }
            else
                pError = $"player ({ pPlayerId }) or weapon id ({ pWeaponId }) is not a valid id";

            return playerCanEquipWeapon;
        }
    }
}