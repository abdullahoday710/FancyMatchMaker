using MatchmakingService.Entities;
using MatchmakingService.Hubs;
using Microsoft.AspNetCore.SignalR;
using StackExchange.Redis;
using System.Collections.Concurrent;
using System.Text.Json;

namespace MatchmakingService.Services
{

    public class MatchMakerService
    {
        private readonly IDatabase _redis;
        private const string QueueListKey = "matchmaking:rankedqueue:list";
        private const string QueueSetKey = "matchmaking:rankedqueue:set";

        private readonly MatchMakerNotifierService _notifyService;
        public MatchMakerService(MatchMakerNotifierService notifyService)
        {
            var connection = ConnectionMultiplexer.Connect("localhost:6379");
            _redis = connection.GetDatabase();
            _notifyService = notifyService;
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

        // This is just a placeholder matchmaking function that matches players 1v1 classic way without any consideration to skill or elo ratings.
        public async Task<List<MatchMakingProfileEntity>> TryMatchPlayersAsync(int playersPerMatch = 2)
        {
            var result = new List<MatchMakingProfileEntity>();

            var length = await _redis.ListLengthAsync(QueueListKey);
            if (length < playersPerMatch)
                return result;

            for (int i = 0; i < playersPerMatch; i++)
            {
                var value = await _redis.ListLeftPopAsync(QueueListKey);

                if (value.IsNullOrEmpty) return result;

                var player = JsonSerializer.Deserialize<MatchMakingProfileEntity>(value);
                if (player == null) continue;

                await _redis.SetRemoveAsync(QueueSetKey, player.ID.ToString());
                result.Add(player);
            }

            return result;
        }

        public async Task<long> QueueSizeAsync()
        {
            return await _redis.ListLengthAsync(QueueListKey);
        }

        public async Task<string> GenerateNewMatchEntry(List<MatchMakingProfileEntity> players)
        {
            string matchID = Guid.NewGuid().ToString();

            List<HashEntry> entries = new List<HashEntry>();

            var playerIDs = new List<string>();

            foreach (var player in players)
            {
                var entry = new HashEntry(player.UserID, "pending");
                entries.Add(entry);

                playerIDs.Add(player.UserID.ToString());
            }


            // Players have 30 seconds to accept a match.
            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var expiry = now + 30; // Expires in 30 seconds
            var matchExpire = new HashEntry("expiry", expiry);
            entries.Add(matchExpire);

            await _redis.HashSetAsync($"match:{matchID}:status", entries.ToArray());
            await _redis.SetAddAsync("matchmaking:active", matchID);

            Console.WriteLine(matchID);

            await _notifyService.NotifyMatchFound(playerIDs.ToArray(), matchID);
            return matchID;
        }

        public async Task CheckIfEveryoneAcceptedThenStartMatch(string matchID)
        {
            string key = $"match:{matchID}:status";

            var matchHashKeys = await _redis.HashKeysAsync(key);

            var playersToNotify = new List<string>();

            foreach (var hashKey in matchHashKeys)
            {
                if (hashKey.ToString() == "expiry")
                {
                    continue;
                }

                playersToNotify.Add(hashKey.ToString());
            }

            var matchHashEntries = await _redis.HashGetAllAsync(key);

            int accepted_players = 0;

            foreach (var hashEntry in matchHashEntries)
            {
                if (hashEntry.Name == "expiry")
                {
                    continue;
                }

                if (hashEntry.Value == "accepted")
                {
                    accepted_players++;
                }
            }

            if (accepted_players == 2)
            {
                await _notifyService.NotifyMatchStarted(playersToNotify.ToArray());

                // Clean up
                await _redis.KeyDeleteAsync(key); // delete the hash
                await _redis.SetRemoveAsync("matchmaking:active", matchID); // remove from active set
            }
        }

        public async Task PlayerAcceptMatch(long userID, string matchID)
        {
            string key = $"match:{matchID}:status";

            var currentStatus = await _redis.HashGetAsync(key, userID.ToString());
            var matchHashKeys = await _redis.HashKeysAsync(key);

            var playersToNotify = new List<string>();

            foreach (var hashKey in matchHashKeys)
            {
                if (hashKey.ToString() == "expiry")
                {
                    continue;
                }

                playersToNotify.Add(hashKey.ToString());
            }

            if (currentStatus.IsNull)
            {
                // Player not found in this match
                return;
            }

            if (currentStatus.ToString() != "pending")
            {
                // Only update if they're pending
                return;
            }

            // Update their status to accepted
            await _redis.HashSetAsync(key, userID.ToString(), "accepted");

            await _notifyService.NotifyMatchAccepted(playersToNotify.ToArray());

            await CheckIfEveryoneAcceptedThenStartMatch(matchID);
        }

        public void CancelMatch(string matchID, RedisValue[] userIDs)
        {
            foreach (var userIDVal in userIDs)
            {
                // Retard way of doing things but who cares. If you are a tech lead that is scanning through my github to see if I am good or not,
                // Well, Congrats, No one gave a shit and dug this deep, So you would be a first. When you call me mention this, It would be funny.
                if (userIDVal.ToString() == "expiry")
                {
                    continue;
                }

                // TODO : re-queue players that didn't decline the match.

            }
        }

        public async Task CheckMatchTimeoutsAsync()
        {
            var matchIDs = await _redis.SetMembersAsync("matchmaking:active");

            foreach (var matchIDRedisValue in matchIDs)
            {
                string matchID = matchIDRedisValue.ToString();
                string key = $"match:{matchID}:status";

                var matchExpiry = await _redis.HashGetAsync(key, "expiry");
                var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

                bool isMatchExpired = false;

                if (long.TryParse(matchExpiry, out long expiryTimestamp))
                {
                    if (now >= expiryTimestamp)
                    {
                        isMatchExpired = true;
                    }
                }

                if (isMatchExpired)
                {
                    var matchHashValues = await _redis.HashValuesAsync(key);

                    // True when some asshole didn't press accept and the match time is expired.
                    bool isMatchDeclinedByAsshole = false;

                    foreach (var matchValue in matchHashValues)
                    {
                        if (matchValue.ToString() == "pending")
                        {
                            isMatchDeclinedByAsshole = true;
                            break;
                        }
                    }

                    if (isMatchDeclinedByAsshole)
                    {
                        var matchHashKeys = await _redis.HashKeysAsync(key);
                        // Re-queue players that stuck around
                        CancelMatch(matchID, matchHashKeys);
                    }
                    


                    // Clean up
                    await _redis.KeyDeleteAsync(key); // delete the hash
                    await _redis.SetRemoveAsync("matchmaking:active", matchID); // remove from active set
                }

            }
        }
    }
}
