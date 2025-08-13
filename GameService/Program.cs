using Common;
using Common.Repo;
using GameService.Context;
using GameService.Hubs;
using GameService.Repo;
using GameService.Services;
using GameService.Subscribers;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace GameService
{
    public class Program
    {
        public static void RegisterSubscribers(ref WebApplicationBuilder builder)
        {
            builder.Services.AddTransient<MatchStartedSubscriber>();
        }

        public static void RegisterRepos(ref WebApplicationBuilder builder)
        {
            builder.Services.AddScoped(typeof(IBaseRepo<>), typeof(BaseRepo<>));

            builder.Services.AddScoped<IMatchHistoryRepository, MatchHistoryRepo>();
        }

        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddDbContext<GameServiceDBContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("GameServiceDB")));

            // Common setup
            CommonServiceBuilder.AddAuthServices(ref builder, true);
            CommonServiceBuilder.AddRabbitMQServices<GameServiceDBContext>(ref builder);
            CommonServiceBuilder.AddRedisServices(ref builder);
            CommonServiceBuilder.CreateCommonCorsPolicies(ref builder);

            // Repositories
            RegisterRepos(ref builder);

            // Services
            builder.Services.AddScoped<MatchHistoryService>();
            builder.Services.AddScoped<OngoingGameService>();
            builder.Services.AddSingleton<GameNotifierService>();

            // Subscribers
            RegisterSubscribers(ref builder);

            // Controllers & Swagger
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                CommonServiceBuilder.AddAuthServicesToSwaggerGen(ref c);
            });

            // SignalR
            builder.Services.AddSignalR();

            var app = builder.Build();

            app.UseRouting();
            app.UseCors("LocalhostAllowAll");

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapHub<GameServiceHub>("/gameServiceHub").RequireCors("LocalhostAllowAll");
            app.MapControllers();

            app.Run();
        }
    }
}
