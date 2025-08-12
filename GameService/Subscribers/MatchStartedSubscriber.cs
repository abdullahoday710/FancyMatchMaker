using Common.Messaging;
using DotNetCore.CAP;
using GameService.Services;

namespace GameService.Subscribers
{
    public class MatchStartedSubscriber : ICapSubscribe
    {
        private readonly OngoingGameService _gameService;

        public MatchStartedSubscriber(OngoingGameService gameService)
        {
            _gameService = gameService;
        }

        [CapSubscribe(TopicNames.NewMatchStarted)]
        public async Task HandleMessage(NewMatchStartedMessage message)
        {
            await _gameService.GenerateNewGameEntry(message);
        }
    }
}
