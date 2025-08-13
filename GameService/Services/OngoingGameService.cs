using Common.Messaging;
using GameService.Entities;
using GameService.MatchState;
using GameService.RedisHandlers;
using StackExchange.Redis;

namespace GameService.Services
{
    public class OngoingGameService
    {
        private readonly GameNotifierService _notifierService;
        private readonly MatchHistoryService _matchHistoryService;
        private readonly IDatabase _redis;

        public OngoingGameService(GameNotifierService notifierService, MatchHistoryService matchHistoryService)
        {
            var connection = ConnectionMultiplexer.Connect("localhost:6379");

            _redis = connection.GetDatabase();

            _notifierService = notifierService;
            _matchHistoryService = matchHistoryService;
        }

        public async Task ConcludeMatch(RedisOnGoingMatchWrapper match)
        {
            long? winner = await match.DetermineMatchWinner();

            var players = await match.GetAllPlayerStates();

            await _notifierService.NotifyGameConcluded(players, winner);

            List<long> playerIDs = new List<long>();
            foreach (var player in players)
            {
                playerIDs.Add(player.PlayerID);
            }

            MatchHistoryEntry matchEntry = new MatchHistoryEntry { MatchUUID = match.GetMatchID(), WinnerID = winner, ParticipatingPlayers = playerIDs };
            await _matchHistoryService.AddMatch(matchEntry);

            await match.DestroyMatch();
        }

        public async Task SetPlayerStance(GameStances stance, long playerID)
        {
            RedisOnGoingMatchWrapper? match = await RedisOnGoingMatchWrapper.GetMatchForPlayer(playerID, _redis);

            if (match != null)
            {
                PlayerState? playerState = await match.GetPlayerState(playerID);

                if (playerState != null)
                {
                    playerState.ChosenStance = stance;
                    playerState.ChoiceMade = true;

                    await match.UpdateStateForPlayer(playerID, playerState);

                    var matchPlayers = await match.GetAllPlayerStates();
                    await _notifierService.NotifyStancePlayed(matchPlayers, playerID);

                    int finished_players = 0;

                    foreach (var player in matchPlayers)
                    { 
                        if (player.ChoiceMade == true)
                        {
                            finished_players++;
                        }
                    }

                    if (finished_players == 2)
                    {
                        await ConcludeMatch(match);
                    }
                }
            }

        }

        public async Task GenerateNewGameEntry(NewMatchStartedMessage message)
        {
            await RedisOnGoingMatchWrapper.GenerateNewOngoingMatchEntry(message, _redis);
        }
    }
}
