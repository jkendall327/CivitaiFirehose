using System.Reflection;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Events;

namespace CivitaiFirehose;

public static class ServiceCollectionExtensions
{
    public static void AddLogging(this WebApplicationBuilder builder)
    {
        builder.Host.UseSerilog((context, config) => {
            config
                .ReadFrom.Configuration(context.Configuration)
                .WriteTo.Console()
                .WriteTo.File("logs/civitai-firehose-.txt",
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 7,
                    fileSizeLimitBytes: 5 * 1024 * 1024)
                .MinimumLevel.Override("System.Net.Http", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning);
        });
    }
    
    public static void AddObservability(this WebApplicationBuilder builder)
    {
        var enabled = builder.Configuration.GetValue<bool>("EnableTelemetry");

        if (!enabled) return;
        
        builder.Services.AddSingleton<Meters>();
        
        var assembly = typeof(Program).Assembly;

        var version = assembly.GetCustomAttributes(false)
            .OfType<AssemblyInformationalVersionAttribute>()
            .Single();

        builder.Services.AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .AddService(serviceName: "CivitaiFirehose", serviceVersion: version.InformationalVersion))
            .WithTracing(tracing => tracing
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddConsoleExporter())
            .WithMetrics(metrics => metrics
                .AddMeter(Meters.MeterName)
                .AddConsoleExporter());
    }
}