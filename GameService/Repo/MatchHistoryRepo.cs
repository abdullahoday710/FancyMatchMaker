using Common.Repo;
using GameService.Context;
using GameService.Entities;
using Microsoft.EntityFrameworkCore;

namespace GameService.Repo
{
    public interface IMatchHistoryRepository : IBaseRepo<MatchHistoryEntry>
    {
        Task<IEnumerable<MatchHistoryEntry>> GetMatchesByPlayerIdAsync(long playerId);
    }

    public class MatchHistoryRepo : BaseRepo<MatchHistoryEntry>, IMatchHistoryRepository
    {
        public MatchHistoryRepo(GameServiceDBContext context) : base(context) { }

        public async Task<IEnumerable<MatchHistoryEntry>> GetMatchesByPlayerIdAsync(long playerId)
        {
            return await _dbSet.Where(e => e.ParticipatingPlayers.Contains(playerId)).ToListAsync();
        }
    }
}
