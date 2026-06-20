// DependencyInjection.cs
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrderService.Application.Common.Interfaces;
using OrderService.Domain.Entities;
using OrderService.Infrastructure.Authentication;
using OrderService.Infrastructure.Identity;
using OrderService.Infrastructure.Persistence;
using OrderService.Infrastructure.Persistence.Interceptors;
using OrderService.Infrastructure.Repositories;

namespace OrderService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services, IConfiguration configuration)
    {
        // 1. Register Interceptors
        services.AddScoped<AuditableEntityInterceptor>();

        // 2. Configure Database Context (PostgreSQL)
        services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            var auditableInterceptor = sp.GetRequiredService<AuditableEntityInterceptor>();
            
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                npgsqlOptions => npgsqlOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName))
                   .AddInterceptors(auditableInterceptor);
        });

        // 3. Configure Identity Core Mappings
        services.AddIdentityCore<ApplicationUser>(options =>
        {
            options.Password.RequiredLength = 8;
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = true;
            
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.AllowedForNewUsers = true;
        })
        .AddRoles<ApplicationRole>()
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

        // 4. Configure Options & Internal Infrastructure Services
        services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddHttpContextAccessor();

        // 5. Register Repositories and Unit of Work
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IAlertRepository, AlertRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // 6. Bind JWT Authentication & Policies Custom Middleware
        services.AddJwtAuthentication(configuration);

        return services;
    }
}
