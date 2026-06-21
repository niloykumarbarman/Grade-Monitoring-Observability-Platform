using Observability.Extensions;
using OrderService.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

const string ServiceName = "OrderService";

builder.Host.AddObservabilityLogging(builder.Configuration, ServiceName);
builder.Services.AddObservability(builder.Configuration, ServiceName);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddInfrastructureServices(builder.Configuration);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
        policy.WithOrigins("http://localhost:3000", "https://grade-monitoring-frontend.onrender.com")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
