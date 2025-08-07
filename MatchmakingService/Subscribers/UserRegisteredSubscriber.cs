using DotNetCore.CAP.Monitoring;
using DotNetCore.CAP;
using Common.Messaging;
using MatchmakingService.Context;
using MatchmakingService.Entities;

namespace MatchmakingService.Subscribers
{
    public class UserRegisteredSubscriber : ICapSubscribe
    {
        private readonly MatchMakingServiceDBContext _dbContext;

        public UserRegisteredSubscriber(MatchMakingServiceDBContext ctx)
        {
            _dbContext = ctx;

        }

        [CapSubscribe(TopicNames.NewUserRegistered)]
        public void HandleMessage(UserRegisteredMessage message)
        {
            var profile = new MatchMakingProfileEntity { UserID = message.UserID };
            _dbContext.MatchMakingProfiles.Add(profile);
            _dbContext.SaveChanges();
        }
    }
}
