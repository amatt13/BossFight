using System;
using System.Collections.Generic;
using System.Linq;
using BossFight.Extentions;
using MySqlConnector;
using System.Text.Json.Serialization;

namespace BossFight.Models.DB
{
    public class ChatMessage : PersistableBase<ChatMessage>, IPersist<ChatMessage>
    {
        [JsonIgnore]
        public override string TableName { get; set; } = nameof(ChatMessage);
        [JsonIgnore]
        public override string IdColumn { get; set; } = nameof(ChatMessageId);

        // Persisted on ChatMessage table
        [PersistPropertyAttribute(true)]
        public int? ChatMessageId { get; set; }

        [JsonIgnore]
        [PersistPropertyAttribute]
        public int? PlayerId 
        { 
            get { return Player?.PlayerId; }
            set 
            { 
                if (Player == null) 
                    Player = new Player();
                Player.PlayerId = value; 
            }
        }

        [PersistPropertyAttribute]
        public string MessageContent { get; set; }

        [PersistPropertyAttribute]
        public DateTime Timestamp { get; set; }

        // From other tables
        [JsonIgnore]
        public Player Player { get; set; }

        public string PlayerName 
        {
            get { return Player.Name; }
        }

        // Not persisted
        [JsonIgnore]
        public DateTime SearchChatMessagesAfterDateTime;

        public ChatMessage () { }

        #region PersistableBase implementation

        public override void BeforePersist()
        {
            base.BeforePersist();
            if (Timestamp == null || Timestamp == DateTime.MinValue)
                Timestamp = DateTime.Now;
        }

        public override IEnumerable<ChatMessage> BuildObjectFromReader(MySqlDataReader reader, MySqlConnection pConnection)
        {
            var result = new List<ChatMessage>();

            while (reader.Read())
            {   
                var ChatMessage = new ChatMessage();
                ChatMessage.ChatMessageId = reader.GetInt(nameof(ChatMessageId));
                ChatMessage.PlayerId = reader.GetInt(nameof(PlayerId));
                ChatMessage.MessageContent = reader.GetString(nameof(MessageContent));
                ChatMessage.Timestamp = reader.GetDateTime(nameof(Timestamp));

                result.Add(ChatMessage);
            }

            reader.Close();

            foreach (ChatMessage chatMessage in result)
            {
                chatMessage.Player = new Player().FindOneForParent(chatMessage.PlayerId.Value, pConnection);
            }

            return result;
        }

        public override string AdditionalSearchCriteria(PersistableBase<ChatMessage> pSearchObject, bool pStartWithAnd = true)
        {
            var cm = pSearchObject as ChatMessage;
            var additionalSearchCriteriaText = String.Empty;

            if (cm.PlayerId.HasValue)
                additionalSearchCriteriaText += $" AND { nameof(PlayerId) } = { cm.PlayerId.Value }\n";

            //TODO allow searching on message content
            //TODO allow searching for before/after Timestamp (use this.SearchChatMessagesAfterDateTime)

            return TrimAdditionalSearchCriteriaText(additionalSearchCriteriaText, pStartWithAnd);
        }

        #endregion PersistableBase implementation
    }
}