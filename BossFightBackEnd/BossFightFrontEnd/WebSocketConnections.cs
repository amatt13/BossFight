using System.Collections.Generic;
using System.Net.WebSockets;

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

        public int GetConnectionsCount()
        {
            lock (_lock)
            {
                return _connections.Count;
            }
        }
    }
}