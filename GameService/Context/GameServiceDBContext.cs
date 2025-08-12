using Microsoft.EntityFrameworkCore;

namespace GameService.Context
{
    public class GameServiceDBContext : DbContext
    {
        public GameServiceDBContext(DbContextOptions<GameServiceDBContext> options) : base(options)
        {

        }


    }
}
