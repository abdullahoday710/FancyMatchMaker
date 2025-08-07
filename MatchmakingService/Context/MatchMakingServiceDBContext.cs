using MatchmakingService.Entities;
using Microsoft.EntityFrameworkCore;

namespace MatchmakingService.Context
{
    public class MatchMakingServiceDBContext : DbContext
    {
        public MatchMakingServiceDBContext(DbContextOptions<MatchMakingServiceDBContext> options) : base(options)
        {

        }

        public DbSet<MatchMakingProfileEntity> MatchMakingProfiles { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<MatchMakingProfileEntity>()
            .HasIndex(u => u.UserID)   // Create an index on the UserID column
            .IsUnique();              // Make it a unique index
        }
    }
}
