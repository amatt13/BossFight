using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using BossFight.Models;
using System.Threading.Tasks;
using System.Net.WebSockets;
using System.Net;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading;
using System.Text;
using System.Text.Json;

namespace BossFight.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WebSocketController : ControllerBase
    {
        public WebSocketController()
        { 
            Console.WriteLine("Init WebSocketController");
        }

        [HttpGet("/ws")]
        public async Task GetWebsocketMessage()
        {
            Console.WriteLine("GetWebsocketMessage");
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                using WebSocket webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                if (!WebSocketConnections.GetInstance().ConnectionExists(webSocket))
                    WebSocketConnections.GetInstance().AddNewConnection(webSocket);
                await ReadMessage(HttpContext, webSocket);
            }
            else
            {
                HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            }
        }

        private async Task ReadMessage(HttpContext pContext, WebSocket pWebSocket)
        {
            Console.WriteLine($"ReadMessage. From '{pContext.Connection.RemoteIpAddress}'");
            var buffer = new byte[1024 * 4];
            var arraySegment = new ArraySegment<byte>(buffer);
            string jsonString = String.Empty;
            var jsonDictionary = new Dictionary<String, Object>();

            WebSocketReceiveResult result = await pWebSocket.ReceiveAsync(arraySegment, CancellationToken.None);
            while (result.CloseStatus == null)
            {
                arraySegment = new ArraySegment<byte>(buffer, 0, result.Count);
                jsonString = Encoding.UTF8.GetString(arraySegment);
                jsonDictionary = JsonSerializer.Deserialize<Dictionary<String, Object>>(jsonString);
                
                //await new SocketMessageHandler(Db).HandleMessage(jsonDictionary, result, pWebSocket);
                await new SocketMessageHandler().HandleMessage(jsonDictionary, result, pWebSocket);
                try
                {
                    result = await pWebSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);   
                }
                catch (WebSocketException e)
                {
                    Console.WriteLine(e.Message);
                } 
            }
        }


        private async void CloseSocket(WebSocket pWebSocket, WebSocketReceiveResult pResult)
        {
            await pWebSocket.CloseAsync(pResult.CloseStatus.Value, pResult.CloseStatusDescription, CancellationToken.None);
        }
    }
}
