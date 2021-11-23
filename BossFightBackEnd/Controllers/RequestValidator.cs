using System;
using System.Collections.Generic;
using System.Data;
using BossFight.Extentions;
using BossFight.Models;
using Ganss.XSS;
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
                cmd.CommandText = @$"SELECT
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

                cmd.Parameters.AddParameter(playerId, "@player_id");
                cmd.Parameters.AddParameter(weaponID, "@weapon_id");
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
                cmd.CommandText = @$"SELECT
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

                cmd.Parameters.AddParameter(playerId, "@player_id");
                cmd.Parameters.AddParameter(weaponID, "@weapon_id");
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

        public static bool PlayerCanAttackMonsterWithEquippedWeapon(string pPlayerId, out string pError)
        {
            var playerCanAttackMonsterWithEquippedWeapon = false;
            var parsed = int.TryParse(pPlayerId, out int playerId);

            if (parsed)
            {
                using var connection = GlobalConnection.GetNewOpenConnection();
                using var cmd = connection.CreateCommand();

                cmd.CommandText = $@"SELECT 
	p.Hp > 0 AND p.{ nameof(Player.WeaponId) } IS NOT NULL AND mi.{ nameof(MonsterInstance.MonsterInstanceId) } IS NOT NULL AND mi.Hp > 0 as ""can attack"",
	CASE 
		WHEN p.Hp <= 0
			THEN 'Not enough health'
		WHEN p.WeaponId IS NULL
			THEN 'No weapon equpped'
		WHEN mi.MonsterInstanceId IS NULL
			THEN 'No active monster to attack'
		WHEN mi.Hp <= 0
			THEN 'Monster is already dead'
	END AS error
FROM Player p 
LEFT JOIN MonsterInstance mi 
	ON mi.Active = TRUE 
WHERE p.PlayerId = @{ nameof(playerId) }";
                
                cmd.Parameters.AddParameter(playerId, nameof(playerId));
                var reader = cmd.ExecuteReader();
                reader.Read();
                playerCanAttackMonsterWithEquippedWeapon = reader.GetBooleanNullable("can attack").GetValueOrDefault(false);
                pError = reader.IsDBNull("error") ? String.Empty : reader.GetString("error");
                reader.Close();
                connection.Close();
            }
            else
                pError = $"player ({ pPlayerId }) is not a valid id";

            return playerCanAttackMonsterWithEquippedWeapon;
        }

        public static bool ValidateUserLoginCredentials(string pUserName, string pPassword, out string pError)
        {
            var errors = new List<String>();

            if (String.IsNullOrWhiteSpace(pUserName))
                errors.Add("Username was empty");

            if (String.IsNullOrWhiteSpace(pPassword))
                errors.Add("Password was empty");

            if (pUserName.Length > 100)
                errors.Add("Username was too long");

            if (pPassword.Length > 100)
                errors.Add("Password was too long");

            if (errors.Count == 0)
            {
                using var connection = GlobalConnection.GetNewOpenConnection();
                using var cmd = connection.CreateCommand();

                cmd.CommandText = $@"SELECT COUNT(*) as PlayersWithMatchingCredentials
FROM Player p
WHERE p.UserName = @userName
AND p.Password = @password";
                cmd.Parameters.AddParameter(pUserName.ToDbString(), "@userName");
                cmd.Parameters.AddParameter(pPassword.ToDbString(), "@password");

                var test = cmd.ToSqlString();

                var reader = cmd.ExecuteReader();
                reader.Read();
                var playersWithMatchingCredentials = reader.GetIntNullable("PlayersWithMatchingCredentials").GetValueOrDefault(0);
                reader.Close();
                connection.Close();

                if (playersWithMatchingCredentials == 0)
                {
                    errors.Add("Invalid user name/password combination");
                }
                else if (playersWithMatchingCredentials >= 2)
                {
                    // should never happen. Constraint in DB that makes it impossible to have multiple users with the same username.
                    throw new Exception($"playersWithMatchingCredentials >= 2. un: '{pUserName}' pw: '{pPassword}'");
                }
            }

            pError = String.Join("\n", errors);
            return errors.Count == 0;
        }

        public static bool ValidateChatMessage(string pMessage, out string pError)
        {
            pError = String.Empty;

            if (pMessage.HasText())
            {
                try
                {
                    new HtmlSanitizer().Sanitize(pMessage);                     
                }
                catch (Exception e)
                {
                    pError = $"Failed to sanitize message:\n{ e }";
                }
            }
            else
                pError = "Chat meesage was empty";

            return String.IsNullOrEmpty(pError);
        }
    }
}