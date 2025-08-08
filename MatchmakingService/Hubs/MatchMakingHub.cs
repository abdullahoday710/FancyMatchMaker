using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Security.Claims;

namespace MatchmakingService.Hubs
{
    public class MatchmakingHub : Hub
    {
        public static readonly ConcurrentDictionary<string, string> _connections = new();

        public override Task OnConnectedAsync()
        {
            var userId = Context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Context.User.FindFirst("sub")?.Value;

            if (!string.IsNullOrEmpty(userId))
            {
                _connections[userId] = Context.ConnectionId;
            }
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            var userId = Context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Context.User.FindFirst("sub")?.Value;

            if (!string.IsNullOrEmpty(userId))
            {
                _connections.TryRemove(userId, out _);
            }
            return base.OnDisconnectedAsync(exception);
        }

    }
}
