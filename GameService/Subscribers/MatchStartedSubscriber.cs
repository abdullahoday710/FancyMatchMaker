using Common.Messaging;
using DotNetCore.CAP;
using Microsoft.EntityFrameworkCore;

namespace GameService.Subscribers
{
    public class MatchStartedSubscriber : ICapSubscribe
    {
        [CapSubscribe(TopicNames.NewMatchStarted)]
        public void HandleMessage(NewMatchStartedMessage message)
        {
            Console.WriteLine("New match has started !!!!");
            Console.WriteLine("Match ID : " + message.matchID);

            foreach (var playerID in message.playerIDs)
            {
                Console.WriteLine(playerID);
            }
        }
    }
}
