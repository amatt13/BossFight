using System;
using MySqlConnector;

namespace BossFight.Extentions
{
    public static class MySqlDataReaderExtensions
    {
        public static int GetInt(this MySqlDataReader pMySqlDataReader, string pColumnName)
        {
            return pMySqlDataReader.GetInt32(pColumnName);
        }

        public static int? GetIntNullable(this MySqlDataReader pMySqlDataReader, string pColumnName, int? pFallbackValue = null)
        {
            return Convert.IsDBNull(pMySqlDataReader[pColumnName]) ? pFallbackValue : (int?)pMySqlDataReader.GetInt32(pColumnName);
        }

        public static string GetStringNullable(this MySqlDataReader pMySqlDataReader, string pColumnName, string pFallbackValue = null)
        {
            return Convert.IsDBNull(pMySqlDataReader[pColumnName]) ? pFallbackValue : pMySqlDataReader.GetString(pColumnName);
        }

        public static float? GetFloatNullable(this MySqlDataReader pMySqlDataReader, string pColumnName, float? pFallbackValue = null)
        {
            return Convert.IsDBNull(pMySqlDataReader[pColumnName]) ? pFallbackValue : (float?)pMySqlDataReader.GetFloat(pColumnName);
        }

        public static bool? GetBooleanNullable(this MySqlDataReader pMySqlDataReader, string pColumnName, bool? pFallbackValue = null)
        {
            return Convert.IsDBNull(pMySqlDataReader[pColumnName]) ? pFallbackValue : (bool?)pMySqlDataReader.GetBoolean(pColumnName);
        }
    }
}
