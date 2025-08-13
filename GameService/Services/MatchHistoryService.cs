using Common.Repo;
using GameService.Entities;
using GameService.Repo;

namespace GameService.Services
{
    public class MatchHistoryService
    {
        private readonly IMatchHistoryRepository _repo;

        public MatchHistoryService(IMatchHistoryRepository matchRepo)
        {
            _repo = matchRepo;
        }

        public async Task<IEnumerable<MatchHistoryEntry>> GetPlayerMatches(long playerId)
        {
            var allMatches = await _repo.GetAllAsync();
            return allMatches.Where(m => m.ParticipatingPlayers.Contains(playerId));
        }

        public async Task AddMatch(MatchHistoryEntry match)
        {
            await _repo.AddAsync(match);
        }
    }
}
