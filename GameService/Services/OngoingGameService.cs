using Common.Messaging;
using GameService.MatchState;
using GameService.RedisHandlers;
using StackExchange.Redis;

namespace GameService.Services
{
    public class OngoingGameService
    {
        private readonly GameNotifierService _notifierService;
        private readonly IDatabase _redis;

        public OngoingGameService(GameNotifierService notifierService)
        {
            var connection = ConnectionMultiplexer.Connect("localhost:6379");

            _redis = connection.GetDatabase();

            _notifierService = notifierService;
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

                    // TODO : notify all players that this player has made a play.
                }
            }

        }

        public async Task GenerateNewGameEntry(NewMatchStartedMessage message)
        {
            await RedisOnGoingMatchWrapper.GenerateNewOngoingMatchEntry(message, _redis);
        }
    }
}
