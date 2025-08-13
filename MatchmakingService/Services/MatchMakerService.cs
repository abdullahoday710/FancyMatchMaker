using Common.Messaging;
using DotNetCore.CAP;
using MatchmakingService.Entities;
using MatchmakingService.RedisHandlers;
using StackExchange.Redis;
using System.Text.Json;

namespace MatchmakingService.Services
{

    public class MatchMakerService
    {
        private readonly IDatabase _redis;
        private RedisMatchMakingQueue RedisMatchMakingQueue;
        private readonly MatchMakerNotifierService _notifyService;
        private readonly ICapPublisher _capPublisher;

        public MatchMakerService(MatchMakerNotifierService notifyService, ICapPublisher capPublisher, IConnectionMultiplexer redisConnection)
        {
            _redis = redisConnection.GetDatabase();
            _notifyService = notifyService;

            RedisMatchMakingQueue = new RedisMatchMakingQueue(_redis);

            _capPublisher = capPublisher;
        }

        public async Task<bool> EnqueuePlayerAsync(MatchMakingProfileEntity player)
        {
            string json = JsonSerializer.Serialize(player);
            await RedisMatchMakingQueue.EnqueuePlayerAsync(player.ID, json);

            return true;
        }

        public async Task<bool> RemovePlayerFromQueue(long profileID)
        {
            await RedisMatchMakingQueue.RemovePlayerFromQueueAsync(profileID);

            return true;
        }

        // This is just a placeholder matchmaking function that will find a match once the queue has the required amount of players in the queue, It doesn't currently have any consideration to elo ratings or whatever.
        public async Task<List<MatchMakingProfileEntity>> TryMatchPlayersAsync()
        {
            var result = new List<MatchMakingProfileEntity>();

            var length = await _redis.ListLengthAsync(RedisMatchMakingQueue.GetQueueKey());
            if (length < RedisMatchWrapper.PlayersPerMatch)
                return result;

            for (int i = 0; i < RedisMatchWrapper.PlayersPerMatch; i++)
            {
                var value = await _redis.ListLeftPopAsync(RedisMatchMakingQueue.GetQueueKey());

                if (value.IsNullOrEmpty) return result;

                var player = JsonSerializer.Deserialize<MatchMakingProfileEntity>(value);
                if (player == null) continue;

                await _redis.SetRemoveAsync(RedisMatchMakingQueue.GetQueueSetKey(), player.ID.ToString());
                result.Add(player);
            }

            return result;
        }

        public async Task<string> GenerateNewMatchEntry(List<MatchMakingProfileEntity> players)
        {
            var result = await RedisMatchWrapper.GenerateNewMatchEntry(players, _redis);
            var playerIDs = new List<string>();

            foreach (var player in players)
            {
                playerIDs.Add(player.UserID.ToString());
            }

            await _notifyService.NotifyMatchFound(playerIDs.ToArray(), result.GetMatchID());

            return result.GetMatchID();
        }

        public async Task PlayerAcceptMatch(long userID, string matchID)
        {
            RedisMatchWrapper match = new RedisMatchWrapper(_redis, matchID);

            // In this case, The PlayerAcceptMatch function must notify the players that a match has started because it will destroy the match entry within redis once the function returns.
            await match.PlayerAcceptMatch(userID, _notifyService, _capPublisher);
        }

        public async Task CheckMatchTimeoutsAsync()
        {
            var matchIDs = await _redis.SetMembersAsync("matchmaking:active");

            foreach (var matchIDRedisValue in matchIDs)
            {
                string matchID = matchIDRedisValue.ToString();
                RedisMatchWrapper match = new RedisMatchWrapper(_redis, matchID);

                await match.CheckMatchExpired();
            }
        }
    }
}
