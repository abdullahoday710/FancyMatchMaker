using MatchmakingService.Hubs;
using Microsoft.AspNetCore.SignalR;
using StackExchange.Redis;

namespace MatchmakingService.Services
{
    public class MatchmakingWorker : BackgroundService
    {
        private readonly MatchMakerService _matchmakingService;
        private readonly ILogger<MatchmakingWorker> _logger;

        public MatchmakingWorker(MatchMakerService matchmakingService, ILogger<MatchmakingWorker> logger)
        {
            _matchmakingService = matchmakingService;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Matchmaking worker started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var players = await _matchmakingService.TryMatchPlayersAsync();

                    if (players.Count > 0)
                    {
                        await _matchmakingService.GenerateNewMatchEntry(players);
                    }

                    // Check for matches that weren't accepted in time.
                    await _matchmakingService.CheckMatchTimeoutsAsync();

                    // Wait 1 second before checking again
                    await Task.Delay(1000, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while trying to match players.");
                    await Task.Delay(2000, stoppingToken); // Delay longer on error
                }
            }

            _logger.LogInformation("Matchmaking worker stopping.");
        }
    }
}
