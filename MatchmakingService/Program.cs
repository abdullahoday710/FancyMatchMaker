using Common;
using MatchmakingService.Context;
using Microsoft.EntityFrameworkCore;

namespace MatchmakingService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddDbContext<MatchMakingServiceDBContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("MatchMakingServiceDB")));

            // Add services to the container.
            CommonServiceBuilder.AddAuthServices(ref builder);


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
