using MatchmakingService.Entities;
using StackExchange.Redis;
using System.Text.Json;

namespace MatchmakingService.Services
{

    public class MatchMakerService
    {
        private readonly IDatabase _redis;
        private const string QueueListKey = "matchmaking:rankedqueue:list";
        private const string QueueSetKey = "matchmaking:rankedqueue:set";

        public MatchMakerService(string redisConnection)
        {
            var connection = ConnectionMultiplexer.Connect(redisConnection);
            _redis = connection.GetDatabase();
        }

        public async Task<bool> EnqueuePlayerAsync(MatchMakingProfileEntity player)
        {
            string playerKey = player.ID.ToString();

            // Try to add to set first
            bool added = await _redis.SetAddAsync(QueueSetKey, playerKey);
            if (!added)
                return false; // Player already in queue

            string json = JsonSerializer.Serialize(player);
            await _redis.ListRightPushAsync(QueueListKey, json);
            return true;
        }

        public async Task<bool> RemovePlayerFromQueue(long profileID)
        {
            var items = await _redis.ListRangeAsync(QueueListKey);

            foreach (var item in items)
            {
                var player = JsonSerializer.Deserialize<MatchMakingProfileEntity>(item!);
                if (player != null && player.ID == profileID)
                {
                    await _redis.ListRemoveAsync(QueueListKey, item);
                    await _redis.SetRemoveAsync(QueueSetKey, profileID.ToString());
                    return true;
                }
            }

            return false; // Not found
        }

        public async Task<MatchMakingProfileEntity[]> TryMatchPlayersAsync(int playersPerMatch = 2)
        {
            var length = await _redis.ListLengthAsync(QueueListKey);
            if (length < playersPerMatch)
                return Array.Empty<MatchMakingProfileEntity>();

            MatchMakingProfileEntity[] players = new MatchMakingProfileEntity[playersPerMatch];

            for (int i = 0; i < playersPerMatch; i++)
            {
                var value = await _redis.ListLeftPopAsync(QueueListKey);
                if (value.IsNullOrEmpty) return Array.Empty<MatchMakingProfileEntity>();

                var player = JsonSerializer.Deserialize<MatchMakingProfileEntity>(value);
                if (player == null) continue;

                await _redis.SetRemoveAsync(QueueSetKey, player.ID.ToString());
                players[i] = player;
            }

            return players;
        }

        public async Task<long> QueueSizeAsync()
        {
            return await _redis.ListLengthAsync(QueueListKey);
        }
    }
}
