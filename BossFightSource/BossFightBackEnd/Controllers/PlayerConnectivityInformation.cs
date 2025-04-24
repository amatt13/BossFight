using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using BossFight.Models;
using Microsoft.Extensions.Logging;
using Xunit;

namespace BossFight
{
    public class PlayerConnectivityInformation
    {
        private readonly record struct PlayerInformation(string Name, int Level, int PlayerId, string PlayerClassName, int CurrentHp, int MaxHp, int CurrentMana, int MaxMana, BodyType PrefferedBodyType);

        private readonly ILogger<PlayerConnectivityInformation> _logger;

        private readonly int _interval = 3_000;

        private List<BossFightWebSocket> _bossFightWebSocketList {get; set;}

        public PlayerConnectivityInformation(ILogger<PlayerConnectivityInformation> logger)
        {
            _logger = logger;
            _logger.LogInformation("Started Player Connectivity Information loop");
        }

        public void Run()
        {
            Task.Run(ConnectionInfoLoop);
        }

        async private Task ConnectionInfoLoop()
        {
            var startTime = DateTime.Now;

            await Work();

            var currentTime = DateTime.Now;
            var durationInMilliseconds = (currentTime - startTime).Milliseconds;
            if (durationInMilliseconds > _interval )
            {
                // should never happend...
                var durationIsSeconds = durationInMilliseconds / 1000;
                _logger.LogInformation("Regen :  We are behind schedule!! It took {durationIsSeconds}", durationIsSeconds);
            }
            else
                Thread.Sleep(_interval  - durationInMilliseconds);

            _ = Task.Run(ConnectionInfoLoop);
        }

        async private Task Work()
        {
            var webSocketConnections = WebSocketConnections.GetInstance();
            _bossFightWebSocketList = webSocketConnections.GetAllOpenConnectionsWithPlayerId();

            var playerInformationList = _bossFightWebSocketList.Select(
                bfws => new PlayerInformation(
                    bfws.Player.Name,
                    bfws.Player.Stats.GetLevel(),
                    bfws.PlayerId.Value,
                    bfws.Player.PlayerPlayerClass.PlayerClass.Name,
                    bfws.Player.Hp,
                    bfws.Player.Stats.GetMaxHp(),
                    bfws.Player.Mana,
                    bfws.Player.Stats.GetMaxMana(),
                    bfws.Player.PrefferedBodyType
                    )
                );

            foreach (var connection in _bossFightWebSocketList)
            {
                var otherPlayerInfoList = playerInformationList.Where(pi => pi.PlayerId != connection.PlayerId.Value);
                if (otherPlayerInfoList.Any())
                {
                    var playerInfoUpdates = new Dictionary<string, IEnumerable<PlayerInformation>>
                    {
                        { "other_players_info_updates",  otherPlayerInfoList}
                    };
                    string json = JsonSerializer.Serialize(playerInfoUpdates);
                    var playerInfoUpdatesByteArray = new ArraySegment<Byte>(Encoding.UTF8.GetBytes(json));
                    await connection.WebSocket.SendAsync(playerInfoUpdatesByteArray, System.Net.WebSockets.WebSocketMessageType.Text, true, new CancellationToken());
                }
            }
        }
    }
}
