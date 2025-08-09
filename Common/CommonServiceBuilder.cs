using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Swashbuckle.AspNetCore.SwaggerGen;
using Microsoft.OpenApi.Models;
using Microsoft.EntityFrameworkCore;

namespace Common
{
    // A common class that will add shared services to WebApplicationBuilder, So that our microservices have a unified front.
    // This works by declaring functions that usually take a ref to builders or options and starts appending the necessary services to it.
    public class CommonServiceBuilder
    {
        public static void AddAuthServices(ref WebApplicationBuilder builder, bool useSignalR)
        {
            var publicKeyPath = RSAKeyUtils.GetPublicKeyPath();

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

                if (useSignalR)
                {
                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            // Allow JWT to be passed in SignalR query string for WebSockets
                            var accessToken = context.Request.Query["access_token"];
                            var path = context.HttpContext.Request.Path;
                            if (!string.IsNullOrEmpty(accessToken))
                            {
                                context.Token = accessToken;
                            }
                            return Task.CompletedTask;
                        }
                    };
                }
            });
        }

        public static void AddRabbitMQServices<TContext>(ref WebApplicationBuilder builder) where TContext : DbContext
        {
            builder.Services.AddCap(x =>
            {
                x.UseEntityFramework<TContext>();

                x.UseRabbitMQ(opt =>
                {
                    opt.HostName = "localhost";
                    opt.Port = 5673;
                    opt.UserName = "guest";
                    opt.Password = "guest";
                });   
            });
        }

        public static void AddAuthServicesToSwaggerGen(ref SwaggerGenOptions options)
        {
            options.AddSecurityDefinition("Bearer",
                new OpenApiSecurityScheme
                {
                In = ParameterLocation.Header,
                Description = "Please enter into field the word 'Bearer' following by space and JWT",
                Name = "Authorization",
                Type = SecuritySchemeType.ApiKey
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement()
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
        }
    }
}
