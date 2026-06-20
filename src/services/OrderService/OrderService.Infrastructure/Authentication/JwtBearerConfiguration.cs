// Authentication/JwtBearerConfiguration.cs
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using OrderService.Infrastructure.Identity;

namespace OrderService.Infrastructure.Authentication;

public static class JwtBearerConfiguration
{
    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services, IConfiguration config)
    {
        var jwt = config.GetSection("JwtSettings").Get<JwtSettings>()!;

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = true;
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer           = true,
                ValidateAudience         = true,
                ValidateLifetime         = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer              = jwt.Issuer,
                ValidAudience            = jwt.Audience,
                IssuerSigningKey         = new SymmetricSecurityKey(
                                               Encoding.UTF8.GetBytes(jwt.SecretKey)),
                ClockSkew                = TimeSpan.FromSeconds(30)
            };

            // Custom event hooks (logging / SignalR token from query string)
            options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = ctx =>
                {
                    if (ctx.Exception is SecurityTokenExpiredException)
                        ctx.Response.Headers["Token-Expired"] = "true";
                    return Task.CompletedTask;
                },
                OnMessageReceived = ctx =>
                {
                    var accessToken = ctx.Request.Query["access_token"];
                    var path = ctx.HttpContext.Request.Path;
                    if (!string.IsNullOrEmpty(accessToken) &&
                        path.StartsWithSegments("/hubs"))
                        ctx.Token = accessToken;
                    return Task.CompletedTask;
                }
            };
        });

        services.AddAuthorization(options =>
        {
            options.AddPolicy("RequireAdmin", p => p.RequireRole("Admin", "SuperAdmin"));
            options.AddPolicy("RequireOperator", p => p.RequireRole("Operator", "Admin", "SuperAdmin"));
        });

        return services;
    }
}
