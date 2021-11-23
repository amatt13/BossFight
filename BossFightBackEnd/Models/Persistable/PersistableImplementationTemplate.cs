using System.Collections.Generic;
using System.Linq;
using BossFight.Extentions;
using MySqlConnector;
using Newtonsoft.Json;

namespace BossFight.Models.DB
{
    public class PersistableImplementationTemplate : PersistableBase, IPersist<PersistableImplementationTemplate>
    {
        [JsonIgnore]
        public override string TableName { get; set; } = nameof(PersistableImplementationTemplate);
        [JsonIgnore]
        public override string IdColumn { get; set; } = nameof(PersistableImplementationTemplateId);

        // Persisted on PersistableImplementationTemplate table
        public int? PersistableImplementationTemplateId { get; set; }

        // From other tables
        // bla bla bla

        public PersistableImplementationTemplate () { }

        #region PersistableBase implementation

        public IEnumerable<PersistableImplementationTemplate> FindAll(int? id = null)
        {
            return _findAll(id).Cast<PersistableImplementationTemplate>();
        }

        public PersistableImplementationTemplate FindOne(int id)
        {
            return (PersistableImplementationTemplate)_findOne(id);
        }

        public override IEnumerable<PersistableBase> BuildObjectFromReader(MySqlDataReader reader, MySqlConnection pConnection)
        {
            var result = new List<PersistableBase>();

            while (reader.Read())
            {   
                var persistableImplementationTemplate = new PersistableImplementationTemplate();
                persistableImplementationTemplate.PersistableImplementationTemplateId = reader.GetInt(nameof(PersistableImplementationTemplateId));
                result.Add(persistableImplementationTemplate);
            }

            return result;
        }

        #endregion PersistableBase implementation
    }
}