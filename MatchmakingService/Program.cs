using Common;
using MatchmakingService.Context;
using MatchmakingService.Hubs;
using MatchmakingService.Services;
using MatchmakingService.Subscribers;
using Microsoft.EntityFrameworkCore;

namespace MatchmakingService
{
    public class Program
    {
        public static void RegisterSubscribers(ref WebApplicationBuilder builder)
        {
            // Register all your kafka subscribers here
            builder.Services.AddTransient<UserRegisteredSubscriber>();
        }

        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddDbContext<MatchMakingServiceDBContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("MatchMakingServiceDB")));

            // Add services to the container.
            CommonServiceBuilder.AddAuthServices(ref builder, true);
            CommonServiceBuilder.AddRabbitMQServices<MatchMakingServiceDBContext>(ref builder);

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("LocalhostAllowAll", policy =>
                {
                    policy.SetIsOriginAllowed(origin =>
                    {
                        if (string.IsNullOrEmpty(origin))
                            return false;

                        // Parse origin host from the URL
                        var uri = new Uri(origin);
                        return uri.Host == "localhost" || uri.Host == "127.0.0.1";
                    })
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
                });
            });

            RegisterSubscribers(ref builder);

            // Register matchmaking service as singleton
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

            app.MapHub<MatchmakingHub>("/matchmakingHub").RequireCors("LocalhostAllowAll"); ;

            app.MapControllers();

            app.Run();
        }
    }
}
