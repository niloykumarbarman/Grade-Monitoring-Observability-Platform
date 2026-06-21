using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Observability.Logging;
using Observability.Metrics;
using Observability.Tracing;
using OpenTelemetry.Logs;

namespace Observability.Extensions;

public static class ObservabilityExtensions
{
    /// <summary>
    /// Wires up distributed tracing and metrics (OpenTelemetry -> OTEL Collector -> Tempo/Prometheus).
    /// Call this on WebApplicationBuilder.Services.
    /// </summary>
    public static IServiceCollection AddObservability(
        this IServiceCollection services,
        IConfiguration configuration,
        string serviceName)
    {
        var otlpEndpoint = configuration["Observability:OtlpEndpoint"] ?? "http://localhost:4317";

        services.AddOpenTelemetry()
            .WithTracing(tracing => tracing.ConfigureTracing(serviceName, otlpEndpoint))
            .WithMetrics(metrics => metrics.ConfigureMetrics(serviceName, otlpEndpoint));

        return services;
    }

    /// <summary>
    /// Wires up structured logging (Serilog -> OTEL Collector -> Loki).
    /// Call this on the WebApplicationBuilder.Host.
    /// </summary>
    public static IHostBuilder AddObservabilityLogging(
        this IHostBuilder hostBuilder,
        IConfiguration configuration,
        string serviceName)
    {
        var otlpEndpoint = configuration["Observability:OtlpEndpoint"] ?? "http://localhost:4317";
        return hostBuilder.UseObservabilityLogging(serviceName, otlpEndpoint);
    }
}
