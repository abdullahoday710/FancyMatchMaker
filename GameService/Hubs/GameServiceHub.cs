using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Security.Claims;

namespace GameService.Hubs
{
    public class GameServiceHub : Hub
    {
        public static readonly ConcurrentDictionary<long, string> _connections = new();

        public override Task OnConnectedAsync()
        {
            var userId = Context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Context.User.FindFirst("sub")?.Value;

            if (!string.IsNullOrEmpty(userId))
            {
                _connections[long.Parse(userId)] = Context.ConnectionId;
            }
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            var userId = Context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Context.User.FindFirst("sub")?.Value;

            if (!string.IsNullOrEmpty(userId))
            {
                _connections.TryRemove(long.Parse(userId), out _);
            }
            return base.OnDisconnectedAsync(exception);
        }

    }
}
