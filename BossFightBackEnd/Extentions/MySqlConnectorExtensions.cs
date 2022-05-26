using System;
using System.Data;
using MySqlConnector;

namespace BossFight.Extentions
{
    public static class MySqlConnectorExtensions
    {
        public static void AddParameter(this MySqlParameterCollection pMySqlParameterCollection, object pParameterValue, string pParameterName, int? pSize = null)
        {
            var dbType = pParameterValue switch
            {
                int _ => DbType.Int32,
                long _ => DbType.Int64,
                bool _ => DbType.Boolean,
                DateTime _ => DbType.DateTime,
                decimal _ => DbType.Decimal,
                double _ => DbType.Double,
                Guid _ => DbType.Guid,
                string _ => DbType.StringFixedLength,
                uint _ => DbType.UInt32,
                ulong _ => DbType.UInt64,
                _ => DbType.Object,
            };
            
            if (!pParameterName.StartsWith("@"))
                pParameterName = '@' + pParameterName;

            var sqlParam = new MySqlParameter
            {
                ParameterName = pParameterName,
                DbType = dbType,
                Value = pParameterValue?.ToString(),
            };

            if (pSize.HasValue)
                sqlParam.Size = pSize.Value;

            pMySqlParameterCollection.Add(sqlParam);
        }

        public static string ToSqlString(this MySqlCommand pCommand)
        {
            var sql = pCommand.CommandText;
            foreach (MySqlParameter param in pCommand.Parameters)
            {
                sql = sql.Replace(param.ParameterName, param.Value.ToString());
            }
            return sql;
        }
    }
}