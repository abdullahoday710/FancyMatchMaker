using MatchmakingService.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace MatchmakingService.Services
{
    public class MatchMakerNotifierService
    {
        private readonly IHubContext<MatchmakingHub> _matchMakingHub;

        public MatchMakerNotifierService(IHubContext<MatchmakingHub> hub)
        {
            _matchMakingHub = hub;
        }

        public async Task NotifyMatchFound(string[] playerUserIds, string matchId)
        {
            foreach (var userId in playerUserIds)
            {
                if (MatchmakingHub._connections.TryGetValue(userId, out var connectionId))
                {
                    await _matchMakingHub.Clients.Client(connectionId).SendAsync("MatchFound", new
                    {
                        MatchId = matchId
                    });
                }
            }
        }

        public async Task NotifyMatchAccepted(string[] playerUserIds)
        {
            foreach (var userId in playerUserIds)
            {
                if (MatchmakingHub._connections.TryGetValue(userId, out var connectionId))
                {
                    await _matchMakingHub.Clients.Client(connectionId).SendAsync("SomeoneAcceptedMatch");
                }
            }
        }

        public async Task NotifyMatchStarted(string[] playerUserIds)
        {
            foreach (var userId in playerUserIds)
            {
                if (MatchmakingHub._connections.TryGetValue(userId, out var connectionId))
                {
                    await _matchMakingHub.Clients.Client(connectionId).SendAsync("MatchStarted");
                }
            }
        }

    }
}
