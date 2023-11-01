using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading.Tasks;
using BossFight.Models;

namespace BossFight
{
    public class BossFightWebSocket
    {
        public WebSocket WebSocket {get; set;}
        public Player Player {get; set;}
        public int? PlayerId
        {
            get
            {
                return this.Player?.PlayerId;
            }
        }

        public BossFightWebSocket(WebSocket pWebSocket)
        {
            WebSocket = pWebSocket;
            Player = null;
        }

        public BossFightWebSocket(WebSocket pWebSocket, Player pPlayer)
        {
            WebSocket = pWebSocket;
            Player = pPlayer;
        }
    }

    public class WebSocketConnections
    {
        private static WebSocketConnections _instance;
        private readonly List<BossFightWebSocket> _connections;
        private readonly object _lock = new();

        private WebSocketConnections()
        {
            _connections = new List<BossFightWebSocket>();
        }

        public static WebSocketConnections GetInstance()
        {
            _instance ??= new WebSocketConnections();

            return _instance;
        }

        public void AddNewConnection(WebSocket pNewWebSocket)
        {
            lock (_lock)
            {
                _connections.Add(new BossFightWebSocket(pNewWebSocket));
            }
        }

        public void RemoveConnection(WebSocket pWebSocket)
        {
            lock (_lock)
            {
                var bossFightWebSocketToRemove = _connections.FirstOrDefault(elem => elem.WebSocket == pWebSocket);
                if (bossFightWebSocketToRemove != null)
                    _connections.Remove(bossFightWebSocketToRemove);
            }
        }

        public void RemoveConnection(int pPlayerId)
        {
            lock (_lock)
            {
                var bossFightWebSocketToRemove = _connections.FirstOrDefault(elem => elem.PlayerId == pPlayerId);
                if (bossFightWebSocketToRemove != null)
                    _connections.Remove(bossFightWebSocketToRemove);
            }
        }

        public bool ConnectionExists(WebSocket pWebSocket)
        {
            lock (_lock)
            {
                return _connections.Any(elem => elem.WebSocket == pWebSocket);
            }
        }

        public bool ConnectionExists(int pPlayerId)
        {
            lock (_lock)
            {
                return _connections.Any(elem => elem.PlayerId == pPlayerId);
            }
        }

        public BossFightWebSocket GetConnection(WebSocket pWebSocket)
        {
            lock (_lock)
            {
                return _connections.FirstOrDefault(elem => elem.WebSocket == pWebSocket);
            }
        }

        public BossFightWebSocket GetConnection(int pPlayerId)
        {
            lock (_lock)
            {
                return _connections.FirstOrDefault(elem => elem.PlayerId == pPlayerId);
            }
        }

        public BossFightWebSocket GetConnection(Player pPlayer)
        {
            lock (_lock)
            {
                return _connections.FirstOrDefault(elem => elem.PlayerId == pPlayer.PlayerId.Value);
            }
        }

        public List<BossFightWebSocket> GetAllConnections()
        {
            lock (_lock)
            {
                return _connections;
            }
        }

        public List<BossFightWebSocket> GetAllOpenConnections()
        {
            lock (_lock)
            {
                return _connections.Where(c => c.WebSocket.State == WebSocketState.Open).ToList();
            }
        }

        public List<BossFightWebSocket> GetAllOpenConnectionsWithPlayerId()
        {
            lock (_lock)
            {
                return _connections.Where(c => c.WebSocket.State == WebSocketState.Open && c.PlayerId.HasValue).ToList();
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
                return _connections.Where(c => c.WebSocket.State == WebSocketState.Open).Count();
            }
        }

        public async Task SendMessageToEveryOneElseWhoAreLoggedInAsync(WebSocket pWebSocket, ArraySegment<Byte> pMessage)
        {
            var otherConnections = GetAllOpenConnectionsWithPlayerId().Where(con => con.WebSocket != pWebSocket);
            foreach (var ws in otherConnections)
                await ws.WebSocket.SendAsync(pMessage, WebSocketMessageType.Text, true, System.Threading.CancellationToken.None);
        }
    }
}
