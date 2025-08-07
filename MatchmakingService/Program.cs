using Common;
using MatchmakingService.Context;
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
            CommonServiceBuilder.AddAuthServices(ref builder);
            CommonServiceBuilder.AddRabbitMQServices<MatchMakingServiceDBContext>(ref builder);

            RegisterSubscribers(ref builder);

            // Register matchmaking service as singleton
            builder.Services.AddSingleton(new MatchMakerService("localhost:6379"));

            // Register matchmaking background worker
            builder.Services.AddHostedService<MatchmakingWorker>();

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();

            builder.Services.AddSwaggerGen(c =>
            {
                CommonServiceBuilder.AddAuthServicesToSwaggerGen(ref c);
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
