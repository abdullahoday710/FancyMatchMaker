using Common.Messaging;
using DotNetCore.CAP;
using MatchmakingService.Context;
using MatchmakingService.Entities;
using MatchmakingService.Repo;

namespace MatchmakingService.Subscribers
{
    public class MatchConcludedSubscriber : ICapSubscribe
    {
        private readonly IMatchMakingProfileRepo _profileRepo;

        public MatchConcludedSubscriber(IMatchMakingProfileRepo profileRepo)
        {
            _profileRepo = profileRepo;
        }

        [CapSubscribe(TopicNames.MatchConcluded)]
        public async Task HandleMessage(MatchConcludedMessage message)
        {
            foreach (var playerID in message.ParticipatingPlayerIDs)
            {
                var playerProfile = await _profileRepo.GetProfileByUserID(playerID);

                if (playerProfile != null)
                {
                    playerProfile.MatchesPlayed++;

                    // if we have a winner
                    if (message.WinnerPlayerID != null)
                    {
                        if (playerProfile.UserID == message.WinnerPlayerID)
                        {
                            playerProfile.MatchesWon++;
                        }
                        else
                        {
                            playerProfile.MatchesLost++;
                        }
                    }
                    // it's a tie
                    else
                    {
                        playerProfile.MatchesTied++;
                    }

                    await _profileRepo.UpdateAsync(playerProfile);
                }
            }
        }
    }
}
