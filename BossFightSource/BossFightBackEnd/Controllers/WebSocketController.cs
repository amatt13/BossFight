using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Net.WebSockets;
using System.Net;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using BossFight.BossFightBackEnd.BossFightLogger;
using Xunit.Sdk;

namespace BossFight.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WebSocketController : ControllerBase
    {
        private static ILogger<WebSocketController> _logger;

        private static void _initLogger()
        {
            ILoggerProvider fileLoggerProvider = new BossFightLoggerProvider("logs/WebSocketController.txt");
            ILoggerFactory _loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
                builder.AddDebug();
                builder.AddProvider(fileLoggerProvider);
                builder.SetMinimumLevel(LogLevel.Trace);
            });
            _logger = _loggerFactory.CreateLogger<WebSocketController>();
        }

        public WebSocketController()
        {
            Console.WriteLine("Init WebSocketController");
            _initLogger();
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

        private static async Task ReadMessage(HttpContext pContext, WebSocket pWebSocket)
        {
            Console.WriteLine($"ReadMessage. From '{pContext.Connection.RemoteIpAddress}'");
            var buffer = new byte[1024 * 4];
            var arraySegment = new ArraySegment<byte>(buffer);
            string jsonString;
            Dictionary<String, Object> jsonDictionary;

            WebSocketReceiveResult result = await pWebSocket.ReceiveAsync(arraySegment, CancellationToken.None);
            while (result.CloseStatus == null)
            {
                arraySegment = new ArraySegment<byte>(buffer, 0, result.Count);
                jsonString = Encoding.UTF8.GetString(arraySegment);
                jsonDictionary = JsonSerializer.Deserialize<Dictionary<String, Object>>(jsonString);

                try
                {
                    await new SocketMessageHandler().HandleMessage(jsonDictionary, result, pWebSocket);
                    result = await pWebSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                }
                catch (WebSocketException e)
                {
                    var errorMessage = e.Message;
                    var StackTrace = e.StackTrace;
                    _logger.LogError("Websocket error: {errorMessage}\n{StackTrace}", errorMessage, StackTrace);
                }
            }
        }


        private async void CloseSocket(WebSocket pWebSocket, WebSocketReceiveResult pResult)
        {
            await pWebSocket.CloseAsync(pResult.CloseStatus.Value, pResult.CloseStatusDescription, CancellationToken.None);
        }
    }
}
