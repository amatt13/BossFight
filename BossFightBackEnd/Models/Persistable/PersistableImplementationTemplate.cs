using System;
using System.Collections.Generic;
using System.Linq;
using BossFight.Extentions;
using MySqlConnector;
using System.Text.Json.Serialization;

namespace BossFight.Models.DB
{
    public class PersistableImplementationTemplate : PersistableBase<PersistableImplementationTemplate>, IPersist<PersistableImplementationTemplate>
    {
        [JsonIgnore]
        public override string TableName { get; set; } = nameof(PersistableImplementationTemplate);
        [JsonIgnore]
        public override string IdColumn { get; set; } = nameof(PersistableImplementationTemplateId);

        // Persisted on PersistableImplementationTemplate table
        [PersistPropertyAttribute(true)]
        public int? PersistableImplementationTemplateId { get; set; }

        // From other tables
        // bla bla bla

        public PersistableImplementationTemplate () { }

        #region PersistableBase implementation

        public override IEnumerable<PersistableImplementationTemplate> BuildObjectFromReader(MySqlDataReader reader, MySqlConnection pConnection)
        {
            var result = new List<PersistableImplementationTemplate>();

            while (reader.Read())
            {   
                var persistableImplementationTemplate = new PersistableImplementationTemplate();
                persistableImplementationTemplate.PersistableImplementationTemplateId = reader.GetInt(nameof(PersistableImplementationTemplateId));
                result.Add(persistableImplementationTemplate);
            }
            reader.Close();

            foreach (var PersistableBase in result)
            {
                //persistableBase.Player = new Player().FindOneForParent(persistableBase.PlayerId, pConnection);
            }

            return result;
        }

        #endregion PersistableBase implementation
    }
}