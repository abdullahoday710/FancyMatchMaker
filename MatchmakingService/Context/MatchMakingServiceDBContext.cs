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

            modelBuilder.Entity<MatchMakingProfileEntity>(matchMakingProfile =>
            {
                // Make the userID a unique index
                matchMakingProfile.HasIndex(u => u.UserID).IsUnique();

                matchMakingProfile.Property(e => e.MatchesPlayed)
                      .HasDefaultValue(0);

                matchMakingProfile.Property(e => e.MatchesWon)
                      .HasDefaultValue(0);

                matchMakingProfile.Property(e => e.MatchesLost)
                      .HasDefaultValue(0);
            });

        }
    }
}
