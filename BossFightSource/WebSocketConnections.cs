using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace BossFight 
{
    public class WebSocketConnections
    {
        private static WebSocketConnections _instance;
        private List<WebSocket> _connections;
        private readonly object _lock = new object();

        private WebSocketConnections()
        { 
            _connections = new List<WebSocket>();
        }

        public static WebSocketConnections GetInstance()
        {
            if (_instance == null)
                _instance = new WebSocketConnections();
            
            return _instance;
        }

        public void AddNewConnection(WebSocket pNewWebSocket)
        {
            lock (_lock)
            {
                _connections.Add(pNewWebSocket);
            }
        }

        public void RemoveConnection(WebSocket pNewWebSocket)
        {
            lock (_lock)
            {
                _connections.Remove(pNewWebSocket);                 
            }
        }

        public bool ConnectionExists(WebSocket pebSocket)
        {
            lock (_lock)
            {
                return _connections.Contains(pebSocket);                 
            }
        }

        public List<WebSocket> GetAllConnections()
        {
            lock (_lock)
            {
                return _connections;
            }
        }

        public List<WebSocket> GetAllOpenConnections()
        {
            lock (_lock)
            {
                return _connections.Where(c => c.State == WebSocketState.Open).ToList();
            }
        }

        public int GetConnectionsCount()
        {
            lock (_lock)
            {
                return _connections.Count;
            }
        }

        public int GetOpenConnectionsCount()
        {
            lock (_lock)
            {
                return _connections.Where(c => c.State == WebSocketState.Open).Count();
            }
        }

        public async Task SendMessageToEveryOneElseAsync(WebSocket pWebSocket, ArraySegment<Byte> pMessage)
        {
            var otherConnections = GetAllOpenConnections().Where(con => con != pWebSocket);
            foreach (var ws in otherConnections)
                await ws.SendAsync(pMessage, WebSocketMessageType.Text, true, System.Threading.CancellationToken.None);
        }
    }
}