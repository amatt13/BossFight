using System;
using System.Data;
using MySqlConnector;

namespace BossFight.Extentions
{
    public static class MySqlConnectorExtensions
    {
        public static void AddParameter(this MySqlParameterCollection pMySqlParameterCollection, object pParameterValue, string pParameterName, int? pSize = null)
        {
            DbType dbType;
            switch (pParameterValue)
            {
                case int i:
                    dbType = DbType.Int32;
                    break;
                case long l:
                    dbType = DbType.Int64;
                    break;
                case bool b:
                    dbType = DbType.Boolean;
                    break;
                case DateTime dt:
                    dbType = DbType.DateTime;
                    break;
                case decimal dec:
                    dbType = DbType.Decimal;
                    break;
                case double d:
                    dbType = DbType.Double;
                    break;
                case Guid g:
                    dbType = DbType.Guid;
                    break;
                case string s:
                    dbType = DbType.StringFixedLength;
                    break;
                case uint ui:
                    dbType = DbType.UInt32;
                    break;
                case ulong ul:
                    dbType = DbType.UInt64;
                    break;

                default:
                    dbType = DbType.Object;
                    break;
            }

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