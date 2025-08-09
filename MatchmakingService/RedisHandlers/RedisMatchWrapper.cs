using MatchmakingService.Entities;
using MatchmakingService.Services;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using StackExchange.Redis;

namespace MatchmakingService.RedisHandlers
{
    public class RedisMatchWrapper
    {
        private readonly IDatabase _redis;
        private readonly string _matchID;
        private readonly string _matchKey;

        public static readonly int PlayersPerMatch = 2;

        // Players have 15 seconds by default to accept a match.
        public static readonly int MatchGracePeriod = 15;

        public RedisMatchWrapper(IDatabase db, string matchID)
        {
            _redis = db;
            _matchID = matchID;

            _matchKey = $"match:{matchID}:status";
        }

        public string GetMatchID() { return _matchID; }

        public async Task DestroyMatch()
        {
            await _redis.KeyDeleteAsync(_matchKey); // delete the hash
            await _redis.SetRemoveAsync("matchmaking:active", _matchID); // remove from active set
        }


        public static async Task<RedisMatchWrapper> GenerateNewMatchEntry(List<MatchMakingProfileEntity> players, IDatabase db)
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

            // Set when the match must expire
            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var expiry = now + MatchGracePeriod;

            var matchExpire = new HashEntry("expiry", expiry);
            entries.Add(matchExpire);

            await db.HashSetAsync($"match:{matchID}:status", entries.ToArray());
            await db.SetAddAsync("matchmaking:active", matchID);

            RedisMatchWrapper result = new RedisMatchWrapper(db, matchID);
            return result;
        }

        public async Task<List<string>> GetPlayerIDsInMatch()
        {
            var matchHashKeys = await _redis.HashKeysAsync(_matchKey);

            var playerIDs = new List<string>();

            foreach (var hashKey in matchHashKeys)
            {
                if (hashKey.ToString() == "expiry")
                {
                    continue;
                }

                playerIDs.Add(hashKey.ToString());
            }

            return playerIDs;
        }

        public async Task<bool> CheckIfEveryoneAccepted()
        {
            var matchHashKeys = await _redis.HashKeysAsync(_matchKey);

            var playersToNotify = new List<string>();

            foreach (var hashKey in matchHashKeys)
            {
                if (hashKey.ToString() == "expiry")
                {
                    continue;
                }

                playersToNotify.Add(hashKey.ToString());
            }

            var matchHashEntries = await _redis.HashGetAllAsync(_matchKey);

            int accepted_players = 0;

            foreach (var hashEntry in matchHashEntries)
            {
                if (hashEntry.Value == "accepted")
                {
                    accepted_players++;
                }
            }

            if (accepted_players == PlayersPerMatch)
            {
                return true;
            }

            return false;
        }

        public async Task PlayerAcceptMatch(long userID, MatchMakerNotifierService notifyService)
        {

            var currentStatus = await _redis.HashGetAsync(_matchKey, userID.ToString());

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
            await _redis.HashSetAsync(_matchKey, userID.ToString(), "accepted");

            

            bool should_match_start = await CheckIfEveryoneAccepted();
            if (should_match_start)
            {
                // notify all players that a match has started
                var playersToNotify = await GetPlayerIDsInMatch();
                await notifyService.NotifyMatchStarted(playersToNotify.ToArray());

                // Clean up the match from our redis queue.
                await DestroyMatch();
            }

        }

        public async Task CancelMatch()
        {
            // TODO : get all the players in the match, Check who accepted, And re-queue them.
        }

        public async Task CheckMatchExpired()
        {
            var matchExpiry = await _redis.HashGetAsync(_matchKey, "expiry");
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
                var matchHashValues = await _redis.HashValuesAsync(_matchKey);

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
                    var matchHashKeys = await _redis.HashKeysAsync(_matchKey);

                    // Re-queue players that stuck around
                    await CancelMatch();
                }


                // Clean up if the match is expired.
                await DestroyMatch();
            }
        }

    }
}
