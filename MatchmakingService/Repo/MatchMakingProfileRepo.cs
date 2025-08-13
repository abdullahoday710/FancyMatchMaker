using Common.Repo;
using MatchmakingService.Context;
using MatchmakingService.Entities;
using Microsoft.EntityFrameworkCore;

namespace MatchmakingService.Repo
{
    public interface IMatchMakingProfileRepo : IBaseRepo<MatchMakingProfileEntity>
    {
        Task<MatchMakingProfileEntity?> GetProfileByUserID(long userID);
    }

    public class MatchMakingProfileRepo : BaseRepo<MatchMakingProfileEntity>, IMatchMakingProfileRepo
    {
        public MatchMakingProfileRepo(MatchMakingServiceDBContext context) : base(context) { }

        public async Task<MatchMakingProfileEntity?> GetProfileByUserID(long userID)
        {
            return await _dbSet.Where(profile => profile.UserID == userID).FirstOrDefaultAsync();
        }
    }
}
