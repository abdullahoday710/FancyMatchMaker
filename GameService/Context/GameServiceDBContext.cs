using GameService.Entities;
using Microsoft.EntityFrameworkCore;

namespace GameService.Context
{
    public class GameServiceDBContext : DbContext
    {
        public DbSet<MatchHistoryEntry> MatchHistoryEntries { get; set; }

        public GameServiceDBContext(DbContextOptions<GameServiceDBContext> options) : base(options)
        {

        }


    }
}
