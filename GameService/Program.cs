
using Common;
using GameService.Context;
using GameService.Hubs;
using GameService.Services;
using GameService.Subscribers;
using Microsoft.EntityFrameworkCore;

namespace GameService
{
    public class Program
    {
        public static void RegisterSubscribers(ref WebApplicationBuilder builder)
        {
            builder.Services.AddTransient<MatchStartedSubscriber>();
        }

        public static void Main(string[] args)
        {

            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddDbContext<GameServiceDBContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("GameServiceDB")));

            CommonServiceBuilder.AddAuthServices(ref builder, true);
            CommonServiceBuilder.AddRabbitMQServices<GameServiceDBContext>(ref builder);
            CommonServiceBuilder.CreateCommonCorsPolicies(ref builder);

            builder.Services.AddSingleton<GameNotifierService>();
            builder.Services.AddSingleton<OngoingGameService>();

            RegisterSubscribers(ref builder);

            // Add services to the container.

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

            app.MapHub<GameServiceHub>("/gameServiceHub").RequireCors("LocalhostAllowAll");

            app.MapControllers();

            app.Run();
        }
    }
}
