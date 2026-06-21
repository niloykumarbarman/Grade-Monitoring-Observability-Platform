using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;

namespace Observability.Metrics;

public static class MetricsConfiguration
{
    public static MeterProviderBuilder ConfigureMetrics(
        this MeterProviderBuilder builder,
        string serviceName,
        string otlpEndpoint)
    {
        return builder
            .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName))
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddRuntimeInstrumentation()
            .AddOtlpExporter(options =>
            {
                options.Endpoint = new Uri(otlpEndpoint);
                options.Protocol = OtlpExportProtocol.Grpc;
            });
    }
}
