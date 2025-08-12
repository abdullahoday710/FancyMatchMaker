using Common.Messaging;
using GameService.MatchState;
using StackExchange.Redis;
using System.Text.Json;

namespace GameService.RedisHandlers
{
    // Represents a data structure for an ongoing match inside redis.
    public class RedisOnGoingMatchWrapper
    {
        private readonly IDatabase _redis;
        private readonly string _matchID;
        private readonly string _matchKey;
        private const int maxMatchLifeTime = 3; // in minutes

        public RedisOnGoingMatchWrapper(IDatabase db, string matchID)
        {
            _redis = db;
            _matchID = matchID;

            _matchKey = $"ongointmatches:{matchID}:status";
        }

        public async Task<bool> IsValidMatch()
        {
            return await _redis.KeyExistsAsync(_matchKey);
        }

        public static async Task<RedisOnGoingMatchWrapper> GenerateNewOngoingMatchEntry(NewMatchStartedMessage matchStartedMessage, IDatabase db)
        {
            List<HashEntry> match_hash_entries = new List<HashEntry>();

            foreach (var playerID in matchStartedMessage.playerIDs)
            {
                var initialPlayerState = new PlayerState
                {
                    PlayerID = playerID,
                    ChosenStance = GameStances.None,
                    ChoiceMade = false,
                    RoundsWon = 0,
                };

                var entry = new HashEntry(playerID, JsonSerializer.Serialize(initialPlayerState));
                match_hash_entries.Add(entry);

                // Create reverse lookup: player -> matchID
                await db.StringSetAsync($"player:{playerID}:matchID", matchStartedMessage.matchID, TimeSpan.FromMinutes(maxMatchLifeTime));
            }

            string matchKey = $"ongointmatches:{matchStartedMessage.matchID}:status";

            await db.HashSetAsync(matchKey, match_hash_entries.ToArray());
            await db.KeyExpireAsync(matchKey, TimeSpan.FromMinutes(maxMatchLifeTime));

            return new RedisOnGoingMatchWrapper(db, matchStartedMessage.matchID);
        }


        public string GetMatchID() { return _matchID; }

        public static async Task<RedisOnGoingMatchWrapper?> GetMatchForPlayer(long playerID, IDatabase db)
        {
            // Look up the matchID for this player
            string? matchID = await db.StringGetAsync($"player:{playerID}:matchID");
            if (string.IsNullOrEmpty(matchID))
                return null;

            var wrapper = new RedisOnGoingMatchWrapper(db, matchID);
            if (!await wrapper.IsValidMatch())
                return null;

            return wrapper;
        }

        public async Task UpdateStateForPlayer(long playerID, PlayerState newState)
        {
            bool exists = await _redis.HashExistsAsync(_matchKey, playerID);
            if (!exists)
                throw new InvalidOperationException($"Player {playerID} is not in match {_matchID}");

            string json = JsonSerializer.Serialize(newState);
            await _redis.HashSetAsync(_matchKey, playerID, json);
        }

        public async Task<PlayerState?> GetPlayerState(long playerID)
        {
            // Ensure the player exists in this match
            if (!await _redis.HashExistsAsync(_matchKey, playerID))
                return null;

            var json = await _redis.HashGetAsync(_matchKey, playerID);
            if (json.IsNullOrEmpty)
                return null;

            return JsonSerializer.Deserialize<PlayerState>(json!);
        }

        public async Task<List<PlayerState>> GetAllPlayerStates()
        {
            var entries = await _redis.HashGetAllAsync(_matchKey);
            if (entries.Length == 0)
                return new List<PlayerState>();

            var result = new List<PlayerState>(entries.Length);
            foreach (var entry in entries)
            {
                if (!entry.Value.IsNullOrEmpty)
                {
                    var state = JsonSerializer.Deserialize<PlayerState>(entry.Value!);
                    if (state != null)
                        result.Add(state);
                }
            }

            return result;
        }

        public async Task DestroyMatch()
        {
            var players = await _redis.HashKeysAsync(_matchKey);

            // Remove player -> matchID reverse lookups
            foreach (var player in players)
            {
                await _redis.KeyDeleteAsync($"player:{player}:matchID");
            }

            await _redis.KeyDeleteAsync(_matchKey);
        }

        public async Task<long?> DetermineMatchWinner()
        {
            var players = await GetAllPlayerStates();

            // Only consider players who made a choice
            var chosenPlayers = players.Where(p => p.ChoiceMade).ToList();

            if (chosenPlayers.Count < 2)
            {
                throw new InvalidOperationException("Trying to determine a winner but not all players have made a play.");
            }

            var p1 = chosenPlayers[0];
            var p2 = chosenPlayers[1];

            if (p1.ChosenStance == p2.ChosenStance)
                return null; // Tie

            // Rock beats Scissors
            if (p1.ChosenStance == GameStances.Rock && p2.ChosenStance == GameStances.Scissors)
                return p1.PlayerID;
            if (p2.ChosenStance == GameStances.Rock && p1.ChosenStance == GameStances.Scissors)
                return p2.PlayerID;

            // Scissors beats Paper
            if (p1.ChosenStance == GameStances.Scissors && p2.ChosenStance == GameStances.Paper)
                return p1.PlayerID;
            if (p2.ChosenStance == GameStances.Scissors && p1.ChosenStance == GameStances.Paper)
                return p2.PlayerID;

            // Paper beats Rock
            if (p1.ChosenStance == GameStances.Paper && p2.ChosenStance == GameStances.Rock)
                return p1.PlayerID;
            if (p2.ChosenStance == GameStances.Paper && p1.ChosenStance == GameStances.Rock)
                return p2.PlayerID;

            return null; // Shouldn't reach here if all stances are valid
        }
    }
}
