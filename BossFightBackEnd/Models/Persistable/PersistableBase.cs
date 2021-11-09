using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Reflection;
using System.Data.Common;
using BossFight.Models.DB;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BossFight.Models
{
    public abstract class PersistableBase
    {
        public static string BackUpConnString;

        public abstract string TableName { get; set; }
        public abstract string IdColumn { get; set; }

        protected sealed class PersistPropertyAttribute : System.Attribute
        {
            //TODO add propty that species column name (if prop name != column name)
            public PersistPropertyAttribute() { }            
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
            cmd.Parameters.Add(new MySqlParameter
            {
                ParameterName = "@id",
                DbType = DbType.String,
                Value = id.ToString(),
            });
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

            return pStartWithAnd ? additionalSearchCriteriaText : additionalSearchCriteriaText.Substring(4, additionalSearchCriteriaText.Length- 4);
        }

        public abstract IEnumerable<PersistableBase> BuildObjectFromReader(MySqlConnector.MySqlDataReader reader);

        public virtual void Persist(int id)
        {
            using var connection = GlobalConnection.GetNewOpenConnection();
            using var cmd = connection.CreateCommand();

            var propsToPersist = this.GetType().GetProperties().Where(prop => prop.IsDefined(typeof(PersistPropertyAttribute), false));   
            var updateTableString = $"UPDATE { TableName }"; 
            var whereString = $"WHERE { IdColumn } = @id";
            var setString = "SET ";
            setString += String.Join(",\n ", propsToPersist.Select(p => $"{ p.Name } = { p.GetValue(this) }"));  //TODO use parameters instead?
            cmd.CommandText = $"{ updateTableString }\n{ setString }\n{ whereString }\nLIMIT 1";
            cmd.Parameters.Add(new MySqlParameter
            {
                ParameterName = "@id",
                DbType = DbType.String,
                Value = id.ToString(),
            });
            cmd.ExecuteNonQuery();
            connection.Close();
        }

        public virtual void Delete(int id)
        {
            using var connection = GlobalConnection.GetNewOpenConnection();
            using var cmd = connection.CreateCommand();

            cmd.CommandText = $"DELETE FROM { TableName } WHERE { IdColumn } = @id LIMIT 1";
            cmd.Parameters.Add(new MySqlParameter
            {
                ParameterName = "@id",
                DbType = DbType.String,
                Value = id.ToString(),
            });
            cmd.ExecuteNonQuery();
            connection.Close();
        }
    }
}