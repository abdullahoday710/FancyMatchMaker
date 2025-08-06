using AuthService.Context;
using Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using AuthService.Entities;
using Microsoft.OpenApi.Models;

namespace AuthService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddDbContext<AuthServiceDBContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("AuthServiceDB")));

            var publicKeyPath = Environment.GetEnvironmentVariable("ROCK_JWT_PUBLIC_KEY_PATH");

            var rsaPublicKey = RSAKeyUtils.LoadRSAKey(publicKeyPath);

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateLifetime = true,
                    ValidateAudience = false,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = "RockPaperScissorsAuthService",
                    IssuerSigningKey = rsaPublicKey
                };
            });

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
                c.AddSecurityDefinition("Bearer",
            new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Description = "Please enter into field the word 'Bearer' following by space and JWT",
                Name = "Authorization",
                Type = SecuritySchemeType.ApiKey
            });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement()
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                        },
                        Scheme = "oauth2",
                        Name = "Bearer",
                        In = ParameterLocation.Header,

                        },
                        new List<string>()
                    }
                });
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
