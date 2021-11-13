using System.Data;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Linq;
using BossFight.Extentions;

namespace BossFight.Models
{
    public abstract class PersistableBase
    {
        public static string BackUpConnString;

        public abstract string TableName { get; set; }
        public abstract string IdColumn { get; set; }

        protected sealed class PersistPropertyAttribute : System.Attribute
        {
            public bool IsIdProperty { get; set; }

            public PersistPropertyAttribute(bool pIsIdProperty = false)
            {
                IsIdProperty = pIsIdProperty;
            }
        }

        public PersistableBase()
        { }

        protected virtual IEnumerable<PersistableBase> _findAll(int? id)
        {
            using var connection = GlobalConnection.GetNewOpenConnection();
            using var cmd = connection.CreateCommand();

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
            var result = BuildObjectFromReader(reader);
            connection.Close();
            return result;
        }

        protected virtual PersistableBase _findOne(int? id)
        {
            PersistableBase fetched = null;
            var result = _findAll(id);
            if (result.Any())
                fetched = result.First();
            return fetched;
        }

        public virtual string AdditionalSearchCriteria(PersistableBase pSearchObject, bool pStartWithAnd = true)
        {
            var additionalSearchCriteriaText = String.Empty;

            return pStartWithAnd ? additionalSearchCriteriaText : additionalSearchCriteriaText.Substring(4, additionalSearchCriteriaText.Length - 4);
        }

        public abstract IEnumerable<PersistableBase> BuildObjectFromReader(MySqlConnector.MySqlDataReader reader);

        public virtual void Persist()
        {
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
                var values = String.Join(", ", propsToPersist.Where(prop => (prop.GetCustomAttributes(true).First(x => x is PersistPropertyAttribute) as PersistPropertyAttribute).IsIdProperty == false).Select(p => p.GetValue(this) ?? "NULL"));
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
                                                                .Select(p => $"{ p.Name } = { p.GetValue(this) }"));  //TODO use parameters instead?
                cmd.CommandText = $"{ updateTableString }\n{ setString }\n{ whereString }\nLIMIT 1";
                cmd.Parameters.AddParameter(id, nameof(id));
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
