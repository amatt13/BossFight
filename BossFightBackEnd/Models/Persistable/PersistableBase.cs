using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using MySqlConnector;
using BossFight.Extentions;

namespace BossFight.Models
{
    public abstract class PersistableBase<T> : PeristableProperties<T> 
    where T : PeristableProperties<T>
    {
        public IEnumerable<T> FindAll(int? id = null)
        {
            using var connection = GlobalConnection.GetNewOpenConnection();
            using var cmd = connection.CreateCommand();

            var selectString = BuildSelectStatement();
            var whereString = String.Empty;
            if (id.HasValue)
            {
                whereString = $"WHERE `{ IdColumn }` = @id \n";
                whereString += AdditionalSearchCriteria(this);
            }
            else
            {
                var additionalSearchCriteriaString = AdditionalSearchCriteria(this, pStartWithAnd: false);
                if (!String.IsNullOrEmpty(additionalSearchCriteriaString))
                    whereString += $"WHERE { additionalSearchCriteriaString }";
            }

            cmd.CommandText = $"{ selectString }\n{ whereString }";
            cmd.Parameters.AddParameter(id, nameof(id));
            var reader = cmd.ExecuteReader();
            var result = BuildObjectFromReader(reader, connection);
            connection.Close();
            return result;
        }

        public IEnumerable<T> FindTop(uint pRowsToRetrieve, string pOrderByColumn, bool pOrderByDescending = true)
        {
            using var connection = GlobalConnection.GetNewOpenConnection();
            using var cmd = connection.CreateCommand();

            var selectString = BuildSelectStatement();
            var whereString = String.Empty;
            var order = pOrderByDescending ? "DESC" : "ASC";
            var orderByString = $"ORDER BY `{ pOrderByColumn }` { order }\n";
            var limitString = $"LIMIT { pRowsToRetrieve }\n";
            
            var additionalSearchCriteriaString = AdditionalSearchCriteria(this, pStartWithAnd: false);
            if (!String.IsNullOrEmpty(additionalSearchCriteriaString))
                whereString += $"WHERE { additionalSearchCriteriaString }";

            cmd.CommandText = $"{ selectString } { whereString } { orderByString } { limitString }";
            var reader = cmd.ExecuteReader();
            var result = BuildObjectFromReader(reader, connection);
            connection.Close();
            return result;
        }

        public IEnumerable<T> FindAllForParent(int? id, MySqlConnection pConnection)
        {
            using var cmd = pConnection.CreateCommand();

            var selectString = $"SELECT * FROM `{ TableName }`\n";
            var whereString = "";
            if (id.HasValue)
            {
                whereString = $"WHERE `{ IdColumn }` = @id \n";
                whereString += AdditionalSearchCriteria(this);
            }
            else
            {
                var additionalSearchCriteriaString = AdditionalSearchCriteria(this, pStartWithAnd: false);
                if (!String.IsNullOrEmpty(additionalSearchCriteriaString))
                    whereString += $"WHERE { additionalSearchCriteriaString }";
            }

            cmd.CommandText = selectString + whereString;
            cmd.Parameters.AddParameter(id, nameof(id));
            var reader = cmd.ExecuteReader();
            var result = BuildObjectFromReader(reader, pConnection);
            reader.Close();
            return result;
        }

        public virtual T FindOne(int? id)
        {
            T fetched = null;
            var result = FindAll(id);
            if (result.Any())
                fetched = result.First();
            return fetched;
        }

        public virtual T FindOneForParent(int? id, MySqlConnection pConnection)
        {
            T fetched = null;
            var result = FindAllForParent(id, pConnection);
            if (result.Any())
                fetched = result.First();
            return fetched;
        }

        public virtual string AdditionalSearchCriteria(PersistableBase<T> pSearchObject, bool pStartWithAnd = true)
        {
            var additionalSearchCriteriaText = String.Empty;

            return TrimAdditionalSearchCriteriaText(additionalSearchCriteriaText, pStartWithAnd);
        }
    }

    public abstract class PeristableProperties<T> 
    where T: class
    {
        public abstract string TableName { get; set; }
        public abstract string IdColumn { get; set; }

        public PeristableProperties()
        { }

        protected virtual string BuildSelectStatement()
        {
            var select = $"SELECT *\nFROM `{ TableName }`\n";
            return select;
        }

        protected string TrimAdditionalSearchCriteriaText(string pAdditionalSearchCriteriaText, bool pStartWithAnd)
        {
            return pStartWithAnd || String.IsNullOrEmpty(pAdditionalSearchCriteriaText) ? pAdditionalSearchCriteriaText : pAdditionalSearchCriteriaText.Substring(4, pAdditionalSearchCriteriaText.Length- 4);
        }

        public abstract IEnumerable<T> BuildObjectFromReader(MySqlConnector.MySqlDataReader reader, MySqlConnection pConnection);

        public virtual void BeforePersist()
        { }

        private object GetValueFromProperyInfo(PropertyInfo pPropertyInfo)
        {
            object resultString = null;
            if (pPropertyInfo.PropertyType == typeof(DateTime))
            {
                resultString = ((DateTime)pPropertyInfo.GetValue(this)).ToString("yyyy-MM-dd HH:mm:ss");
                resultString = $"\"{ resultString }\"";
            }
            else if (pPropertyInfo.PropertyType == typeof(String))
            {
                resultString = pPropertyInfo.GetValue(this);
                resultString = $"\"{ resultString }\"";
            }
            else if (pPropertyInfo.PropertyType.IsEnum)
            {
                resultString = (int)pPropertyInfo.GetValue(this);
            }
            else if (Nullable.GetUnderlyingType(pPropertyInfo.PropertyType) != null && Nullable.GetUnderlyingType(pPropertyInfo.PropertyType).IsEnum)
            {
                resultString = (int)pPropertyInfo.GetValue(this);
            }
            else
                resultString = pPropertyInfo.GetValue(this);

            return resultString;
        }

        public virtual void Persist()
        {
            BeforePersist();

            var id = (int?)GetType().GetProperty(IdColumn).GetValue(this);
            using var connection = GlobalConnection.GetNewOpenConnection();

            // null for new objects
            using var cmd = connection.CreateCommand();
            if (id == null)
            {
                // Insert new
                var propsToPersist = GetType().GetProperties().Where(prop => prop.IsDefined(typeof(PersistPropertyAttribute), true));
                var insert = $"INSERT { TableName }";
                var colums = String.Join(", ", propsToPersist.Where(prop => (prop.GetCustomAttributes(true).First(x => x is PersistPropertyAttribute) as PersistPropertyAttribute).IsIdProperty == false).Select(p => p.Name));
                var values = String.Join(", ", propsToPersist.Where(prop => (prop.GetCustomAttributes(true).First(x => x is PersistPropertyAttribute) as PersistPropertyAttribute).IsIdProperty == false).Select(p => GetValueFromProperyInfo(p) ?? "NULL"));
                cmd.CommandText = $"{ insert }\n({ colums })\nVALUES\n({ values })\n";
                cmd.ExecuteNonQuery();
                connection.Close();
            }
            else
            {
                // Update existing
                var propsToPersist = GetType().GetProperties().Where(prop => prop.IsDefined(typeof(PersistPropertyAttribute), true));
                var updateTableString = $"UPDATE { TableName }";
                var whereString = $"WHERE { IdColumn } = @id";
                var setString = "SET ";
                setString += String.Join(",\n ", propsToPersist  // don't persist IdCoulm if it already has a value
                                                                .Where(prop => !((prop.GetCustomAttributes(true).First(x => x is PersistPropertyAttribute) as PersistPropertyAttribute).IsIdProperty && prop.GetValue(this) != null))
                                                                .Select(p => $"{ p.Name } = { p.GetValue(this).ToDbString() }"));  //TODO use parameters instead?
                cmd.CommandText = $"{ updateTableString }\n{ setString }\n{ whereString }\nLIMIT 1";
                cmd.Parameters.AddParameter(id, "@id");
                cmd.ExecuteNonQuery();
                connection.Close();
            }
        }

        private int? GetNextId(MySqlConnection connection)
        {
            int? nextId = null;
            using var cmd = connection.CreateCommand();

            cmd.CommandText = $@"SELECT IFNULL(MAX({ IdColumn }) + 1, 0)
            FROM ({ TableName })";

            nextId = Convert.ToInt32(cmd.ExecuteScalar());

            return nextId.Value;
        }

        public virtual void Delete(int id)
        {
            using var connection = GlobalConnection.GetNewOpenConnection();
            using var cmd = connection.CreateCommand();

            cmd.CommandText = $"DELETE FROM { TableName } WHERE { IdColumn } = @id LIMIT 1";
            cmd.Parameters.AddParameter(id, nameof(id));
            cmd.ExecuteNonQuery();
            connection.Close();
        }

        protected int? GetIdValue()
        {
            return (int?)GetType().GetProperty(IdColumn).GetValue(this);
        }
    }
}
