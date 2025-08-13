using Common;
using Common.Repo;
using MatchmakingService.Context;
using MatchmakingService.Hubs;
using MatchmakingService.Repo;
using MatchmakingService.Services;
using MatchmakingService.Subscribers;
using Microsoft.EntityFrameworkCore;

namespace MatchmakingService
{
    public class Program
    {
        public static void RegisterSubscribers(ref WebApplicationBuilder builder)
        {
            builder.Services.AddTransient<UserRegisteredSubscriber>();
            builder.Services.AddTransient<MatchConcludedSubscriber>();

        }

        public static void RegisterRepos(ref WebApplicationBuilder builder)
        {
            builder.Services.AddScoped(typeof(IBaseRepo<>), typeof(BaseRepo<>));

            builder.Services.AddScoped<IMatchMakingProfileRepo, MatchMakingProfileRepo>();
        }

        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddDbContext<MatchMakingServiceDBContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("MatchMakingServiceDB")));

            // Add services to the container.
            CommonServiceBuilder.AddAuthServices(ref builder, true);
            CommonServiceBuilder.AddRabbitMQServices<MatchMakingServiceDBContext>(ref builder);
            CommonServiceBuilder.CreateCommonCorsPolicies(ref builder);

            RegisterRepos(ref builder);
            RegisterSubscribers(ref builder);

            // Register matchmaking services as singleton
            builder.Services.AddSingleton<MatchMakerNotifierService>();
            builder.Services.AddSingleton<MatchMakerService>();

            // Register matchmaking background worker
            builder.Services.AddHostedService<MatchmakingWorker>();

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();

            builder.Services.AddSwaggerGen(c =>
            {
                CommonServiceBuilder.AddAuthServicesToSwaggerGen(ref c);
            });

            builder.Services.AddSignalR();

            var app = builder.Build();
            app.UseRouting();
            app.UseCors("LocalhostAllowAll");

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapHub<MatchmakingHub>("/matchmakingHub").RequireCors("LocalhostAllowAll");

            app.MapControllers();

            app.Run();
        }
    }
}
