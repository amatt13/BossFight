using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Ganss.Xss;
using BossFight.Models;
using BossFight.Models.DB;

namespace BossFight.Controllers.SocketMessageHandlers
{
    public static class ChatHandler
    {
        // takes: message: "string", player_id: "int"
        // receive an message from the client. This message must then be sent to all connection
        public static async Task SendChatMessage(Dictionary<string, JsonElement> pJsonParameters, WebSocketReceiveResult pWebSocketReceiveResult, WebSocket pWebSocket)
        {
            var requiredValues = SocketMessageHandler.CreateValueList(pJsonParameters, new List<string> { "message", "player_id" });
            if (RequestValidator.AllValuesAreFilled(requiredValues, out string error))
            {
                var message = pJsonParameters["message"].GetString();
                var playerId = pJsonParameters["player_id"].GetInt32(); //TODO validate valid player
                if (RequestValidator.ValidateChatMessage(message, playerId, out error))
                {
                    var sanitized = new HtmlSanitizer().Sanitize(message);
                    var doubleEscaped = sanitized.Replace(@"\", @"\\");
                    var player = new Player().FindOne(playerId);
                    var chatMessage = new ChatMessage { MessageContent = doubleEscaped, Timestamp = DateTime.Now, Player = player };
                    chatMessage.Persist();
                    chatMessage.MessageContent = sanitized;

                    var response = new Dictionary<string, ChatMessage>
                    {
                        { "receive_chat_message", chatMessage }
                    };
                    string output = JsonSerializer.Serialize(response);
                    var byteArray = new ArraySegment<Byte>(Encoding.UTF8.GetBytes(output));
                    foreach (var ws in WebSocketConnections.GetInstance().GetAllOpenConnections())
                        await ws.WebSocket.SendAsync(byteArray, pWebSocketReceiveResult.MessageType, pWebSocketReceiveResult.EndOfMessage, CancellationToken.None);
                }
            }
            if (!String.IsNullOrEmpty(error))
                await SocketMessageHandler.ReplyWithErrorMessage(pWebSocketReceiveResult, pWebSocket, error);
        }

        // takes: messages_to_fetch: "int"
        // returns: list of ChatMessage
        public static async Task FetchMostRecentMessages(Dictionary<string, JsonElement> pJsonParameters, WebSocketReceiveResult pWebSocketReceiveResult, WebSocket pWebSocket)
        {
            var requiredValues = SocketMessageHandler.CreateValueList(pJsonParameters, new List<string> { "messages_to_fetch" });
            if (RequestValidator.AllValuesAreFilled(requiredValues, out string error))
            {
                var messagesToFetch = pJsonParameters["messages_to_fetch"].GetInt32();

                if (RequestValidator.ValidateMaxMessageRequestNumberNotExceeded(messagesToFetch, out error))
                {
                    var chatMessages = new ChatMessage { }.FindTop((UInt32)messagesToFetch, nameof(ChatMessage.Timestamp));
                    var response = new Dictionary<string, IEnumerable<ChatMessage>>
                    {
                        { "receive_multiple_chat_message", chatMessages }
                    };
                    string output = JsonSerializer.Serialize(response);
                    var byteArray = new ArraySegment<Byte>(Encoding.UTF8.GetBytes(output));
                    await pWebSocket.SendAsync(byteArray, pWebSocketReceiveResult.MessageType, pWebSocketReceiveResult.EndOfMessage, CancellationToken.None);
                }
            }
            if (!String.IsNullOrEmpty(error))
                await SocketMessageHandler.ReplyWithErrorMessage(pWebSocketReceiveResult, pWebSocket, error);
        }
    }
}
