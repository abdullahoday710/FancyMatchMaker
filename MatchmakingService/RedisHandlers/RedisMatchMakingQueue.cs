using MatchmakingService.Entities;
using StackExchange.Redis;
using System.Numerics;
using System.Text.Json;

namespace MatchmakingService.RedisHandlers
{
    public class RedisMatchMakingQueue
    {
        private readonly IDatabase _redis;

        private const string QueueListKey = "matchmaking:rankedqueue:list";
        private const string QueueSetKey = "matchmaking:rankedqueue:set";

        public RedisMatchMakingQueue(IDatabase db)
        { 
            _redis = db;
        }

        public static string GetQueueKey() { return QueueListKey; }
        public static string GetQueueSetKey() { return QueueSetKey; }

        public async Task EnqueuePlayerAsync(long playerID, string playerJsonData)
        {
            string playerKey = playerID.ToString();

            // Try to add to set first
            bool added = await _redis.SetAddAsync(QueueSetKey, playerKey);
            if (!added)
                return; // Player already in queue

            await _redis.ListRightPushAsync(QueueListKey, playerJsonData);
        }

        public async Task RemovePlayerFromQueueAsync(long profileID)
        {
            var items = await _redis.ListRangeAsync(QueueListKey);

            foreach (var item in items)
            {
                var player = JsonSerializer.Deserialize<MatchMakingProfileEntity>(item!);
                if (player != null && player.ID == profileID)
                {
                    await _redis.ListRemoveAsync(QueueListKey, item);
                    await _redis.SetRemoveAsync(QueueSetKey, profileID.ToString());
                }
            }
        }

        public async Task<long> QueueSizeAsync()
        {
            return await _redis.ListLengthAsync(QueueListKey);
        }
    }
}
