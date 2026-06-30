using BuildingBlocks.EventBus.Abstractions;
using BuildingBlocks.EventBus.IntegrationEvents;
using BuildingBlocks.EventBus.RabbitMQ;
using GradeService.Application;
using GradeService.Infrastructure.EventHandlers;
using GradeService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

// Database
builder.Services.AddDbContext<GradeDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("GradeDb")));

// Repository
builder.Services.AddScoped<IGradeRepository, GradeRepository>();

// RabbitMQ Connection
builder.Services.AddSingleton<IConnection>(sp =>
{
    var factory = new ConnectionFactory
    {
        HostName = builder.Configuration["RabbitMQ:HostName"] ?? "localhost",
        Port = int.Parse(builder.Configuration["RabbitMQ:Port"] ?? "5672"),
        UserName = builder.Configuration["RabbitMQ:UserName"] ?? "guest",
        Password = builder.Configuration["RabbitMQ:Password"] ?? "guest",
        VirtualHost = builder.Configuration["RabbitMQ:VirtualHost"] ?? "/",
        Ssl = new SslOption
        {
            Enabled = bool.Parse(builder.Configuration["RabbitMQ:SslEnabled"] ?? "false"),
            ServerName = builder.Configuration["RabbitMQ:HostName"] ?? "localhost"
        }
    };
    return factory.CreateConnectionAsync().GetAwaiter().GetResult();
});

// EventBus
builder.Services.AddSingleton<IEventBus, RabbitMQEventBus>();

// Event Handlers
builder.Services.AddTransient<OrderConfirmedEventHandler>();

var app = builder.Build();

// Auto-create DB on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<GradeDbContext>();
    db.Database.Migrate();
}

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.UseHttpsRedirection();

// Subscribe to events
var eventBus = app.Services.GetRequiredService<IEventBus>();
eventBus.Subscribe<OrderConfirmedIntegrationEvent, OrderConfirmedEventHandler>();

// Health check
app.MapGet("/health", () => Results.Ok(new { status = "GradeService is running" }))
   .WithName("HealthCheck");

// Grades endpoint
app.MapGet("/api/grades/{studentId:guid}", async (Guid studentId, GradeDbContext db) =>
{
    var grades = await db.Grades
        .Where(g => g.StudentId == studentId)
        .OrderByDescending(g => g.UpdatedAt)
        .ToListAsync();
    return Results.Ok(grades);
}).WithName("GetGradesByStudent");

app.Run();
