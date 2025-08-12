using GameService.Hubs;
using GameService.MatchState;
using Microsoft.AspNetCore.SignalR;

namespace GameService.Services
{
    public class GameNotifierService
    {
        private readonly IHubContext<GameServiceHub> _gameHub;

        public GameNotifierService(IHubContext<GameServiceHub> hub) { _gameHub = hub; }

        public async Task NotifyStancePlayed(List<PlayerState> players, long stanceIssuer)
        {
            foreach (var player in players)
            {
                if (GameServiceHub._connections.TryGetValue(player.PlayerID, out var connectionId))
                {
                    await _gameHub.Clients.Client(connectionId).SendAsync("StancePlayed", new { player = stanceIssuer});
                }
            }
        }

        public async Task NotifyGameConcluded(List<PlayerState> players, long? winnerPlayer)
        {
            foreach (var player in players)
            {
                if (GameServiceHub._connections.TryGetValue(player.PlayerID, out var connectionId))
                {
                    await _gameHub.Clients.Client(connectionId).SendAsync("GameConcluded", new { winner = winnerPlayer, playerStates = players });
                }
            }
        }

    }
}
