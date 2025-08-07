using AuthService.Context;
using Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using AuthService.Entities;

namespace AuthService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddDbContext<AuthServiceDBContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("AuthServiceDB")));

            CommonServiceBuilder.AddAuthServices(ref builder);
            CommonServiceBuilder.AddRabbitMQServices<AuthServiceDBContext>(ref builder);

            builder.Services.AddDataProtection();

            builder.Services.AddIdentity<UserEntity, IdentityRole<long>>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequiredLength = 6;
                options.Tokens.PasswordResetTokenProvider = TokenOptions.DefaultEmailProvider;
                options.Tokens.EmailConfirmationTokenProvider = TokenOptions.DefaultEmailProvider;
            }).AddEntityFrameworkStores<AuthServiceDBContext>().AddDefaultTokenProviders();

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
