using System;
using System.Collections.Generic;
using System.Linq;
using BossFight.Extentions;
using MySqlConnector;
using System.Text.Json.Serialization;

namespace BossFight.Models
{
    public class BodyType : PersistableBase<BodyType>, IPersist<BodyType>
    {
        [JsonIgnore]
        public override string TableName { get; set; } = nameof(BodyType);
        [JsonIgnore]
        public override string IdColumn { get; set; } = nameof(BodyTypeId);

        // Persisted on BodyType table
        [PersistProperty(true)]
        public int? BodyTypeId { get; set; }

        [PersistProperty]
        public string Name { get; set; }

        public BodyType () { }

        #region PersistableBase implementation

        public override IEnumerable<BodyType> BuildObjectFromReader(MySqlDataReader reader, MySqlConnection pConnection)
        {
            var result = new List<BodyType>();

            while (reader.Read())
            {   
                var BodyType = new BodyType();
                BodyType.BodyTypeId = reader.GetInt(nameof(BodyTypeId));
                BodyType.Name = reader.GetString(nameof(Name));
                result.Add(BodyType);
            }
            reader.Close();

            return result;
        }

        public override string AdditionalSearchCriteria(PersistableBase<BodyType> pSearchObject, bool pStartWithAnd = true)
        {
            var bt = pSearchObject as BodyType;
            var additionalSearchCriteriaText = String.Empty;
            if (bt.Name.HasText())
                additionalSearchCriteriaText += $" AND { nameof(Name) } = { bt.Name.ToDbString() }\n";

            return TrimAdditionalSearchCriteriaText(additionalSearchCriteriaText, pStartWithAnd);
        }

        #endregion PersistableBase implementation
    }
}