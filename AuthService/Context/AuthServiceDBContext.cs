using AuthService.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Context
{
    public class AuthServiceDBContext : DbContext
    {
        public AuthServiceDBContext(DbContextOptions<AuthServiceDBContext> options) : base(options)
        {
        }
        public DbSet<UserEntity> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
