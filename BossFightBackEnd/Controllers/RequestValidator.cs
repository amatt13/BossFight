using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using BossFight.BossFightEnums;
using BossFight.Extentions;
using BossFight.Models;
using Ganss.XSS;
using MySqlConnector;

namespace BossFight.Controllers
{
    public static class RequestValidator
    {
        public static bool PlayerCanSellWeapon(int pPlayerId, int pWeaponId, out string pError)
        {
            bool playerCanSellWeapon = false;
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

            cmd.Parameters.AddParameter(pPlayerId, "@player_id");
            cmd.Parameters.AddParameter(pWeaponId, "@weapon_id");
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

            return playerCanSellWeapon;
        }

        public static bool PlayerCanEquipWeapon(int pPlayerId, int pWeaponId, out string pError)
        {
            bool playerCanEquipWeapon = false;
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

            cmd.Parameters.AddParameter(pPlayerId, "@player_id");
            cmd.Parameters.AddParameter(pWeaponId, "@weapon_id");
            var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                playerCanEquipWeapon = reader.GetBooleanNullable("CanEquip").GetValueOrDefault(false);
            }
            reader.Close();
            connection.Close();
            if (playerCanEquipWeapon)
            {
                pError = String.Empty;
            }
            else
                pError = "Could not equip weapon. Make sure you haven't already equipped it.";

            return playerCanEquipWeapon;
        }

        public static bool PlayerCanAttackMonsterWithEquippedWeapon(int pPlayerId, out string pError)
        {
            var playerCanAttackMonsterWithEquippedWeapon = false;

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
WHERE p.PlayerId = @{ nameof(pPlayerId) }";
            
            cmd.Parameters.AddParameter(pPlayerId, nameof(pPlayerId));
            var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                playerCanAttackMonsterWithEquippedWeapon = reader.GetBooleanNullable("can attack").GetValueOrDefault(false);
                pError = reader.IsDBNull("error") ? String.Empty : reader.GetString("error");
            }
            else
            {
                pError = "Could not attack monster for an unkown reason (try reloading the site)";
            }
            reader.Close();
            connection.Close();
            
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
WHERE p.UserName = {pUserName.ToDbString()}
AND p.Password = {pPassword.ToDbString()}";

                var reader = cmd.ExecuteReader();
                var playersWithMatchingCredentials = 0;
                if (reader.Read())
                {
                    playersWithMatchingCredentials = reader.GetIntNullable("PlayersWithMatchingCredentials").GetValueOrDefault(0);
                }
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

        public static bool ValidateChatMessage(string pMessage, int pPlayerId, out string pError)
        {
            pError = String.Empty;

            if (pMessage.HasText())
            {
                if (pMessage.Length > 500)
                {
                    pError = $"Message too long. Max 500 characters are allowed.";
                }
                else
                {
                    try
                    {
                        new HtmlSanitizer().Sanitize(pMessage);                     
                    }
                    catch (Exception e)
                    {
                        pError = $"Failed to sanitize message:\n{ e }";
                    }

                    if (!PlayerExists(pPlayerId))
                    {
                        pError = "Could not find player";
                    }
                }
            }
            else
                pError = "Chat meesage was empty";

            return String.IsNullOrEmpty(pError);
        }

        public static bool ValidateMaxMessageRequestNumberNotExceeded(int? pRequestedMessages, out string pError)
        {
            pError = String.Empty;
            var maxMessages = 100;
            var requested = pRequestedMessages.GetValueOrDefault(0);

            if (requested > maxMessages)
            {
                pError = $"Too many messages was requestd ({requested}). Maximum allowed is {maxMessages}";
            }
            else if (requested <= 0)
            {
                pError = $"Must request 1 or more messages. Requested was {requested}.";
            }

            return String.IsNullOrEmpty(pError);
        }

        public static bool ValidateVoteForMonsterTier(int pPlayerId, int pMonsterInstanceId, int pVote, out string pError)
        {
            var errors = new List<String>();
            
            if (!PlayerExists(pPlayerId))
            {
                errors.Add("Could not find player");
            }

            if (!MonsterInstanceExists(pMonsterInstanceId))
            {
                errors.Add("Could not find monster");
            }

            if (!ValidEnumValue(MonsterTierVoteChoice.DECREASE_DIFFICULTY, pVote))
            {
                errors.Add("Invalid choice");
            }

            pError = String.Join(Environment.NewLine, errors);
            return String.IsNullOrEmpty(pError);
        }

        public static bool AllValuesAreFilled(List<Tuple<object, string>> pValueList, out string  pError)
        {
            var allValuesAreFilled = true;
            pError = String.Empty;
            var fieldsWithMissingValues = pValueList.Where(vp => vp.Item1 == null || vp.Item1 is DBNull);
            if (fieldsWithMissingValues.Any())
            {
                pError = String.Join(Environment.NewLine, fieldsWithMissingValues.Select(vp => $"Missing required value for '{ vp.Item2 }'"));
                allValuesAreFilled = false;
            }

            return allValuesAreFilled;
        }

        private static bool PlayerExists(int pPlayerId)
        {
            var sql = $@"SELECT TRUE FROM { nameof(Player) } WHERE { nameof(Player.PlayerId) } = @{ nameof(Player.PlayerId) }";
            var cmd = new MySqlCommand(sql);
            cmd.Parameters.AddParameter(pPlayerId, nameof(Player.PlayerId));
            var playerExists = GlobalConnection.SingleValue<bool>(cmd);

            return playerExists;
        }

        private static bool MonsterInstanceExists(int pMonsterInstanceId)
        {
            var sql = $@"SELECT TRUE FROM { nameof(MonsterInstance) } WHERE { nameof(MonsterInstance.MonsterInstanceId) } = @{ nameof(MonsterInstance.MonsterInstanceId) }";
            var cmd = new MySqlCommand(sql);
            cmd.Parameters.AddParameter(pMonsterInstanceId, nameof(MonsterInstance.MonsterInstanceId));
            var monsterInstanceExists = GlobalConnection.SingleValue<bool>(cmd);

            return monsterInstanceExists;
        }

        /// <summary>
        /// The pEnum does not matter and can be any one value from the Enum
        /// </summary>
        private static bool ValidEnumValue(Enum pEnum, int pValue)
        {
            return Enum.IsDefined(pEnum.GetType(), pValue);
        }

        public static bool Example() {return false;}
    }
}