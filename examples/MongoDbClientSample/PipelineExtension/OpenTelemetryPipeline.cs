using System;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Instrumentation.MongoDbClient;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace MongoDbClientSample.PipelineExtension;

public sealed class OpenTelemetryPipeline : IDisposable
{
    public TracerProvider? TracerProvider { get; private set; }
    public MeterProvider? MeterProvider { get; private set; }
    public ILoggerFactory? LoggerFactory { get; private set; }
    public ActivitySource ActivitySource { get; }

    private bool disposed;

    internal OpenTelemetryPipeline(OpenTelemetryPipelineBuilder builder)
    {
        ActivitySource = new ActivitySource(builder.Options.ServiceName);

        var resource = builder.ResourceBuilder;

        if (builder.Options.EnableTracing)
        {
            var tracingBuilder = Sdk.CreateTracerProviderBuilder()
                .SetResourceBuilder(resource);

            tracingBuilder
                .AddMongoDbClientInstrumentation(options =>
                {
                    builder.MongoDbTracingConfig?.Invoke(options);
                })
                .AddSource(builder.Options.ServiceName);

            if (builder.Sampler != null)
                tracingBuilder.SetSampler(builder.Sampler);

            if (!string.IsNullOrEmpty(builder.Options.OtlpEndpoint))
            {
                tracingBuilder.AddOtlpExporter(opt =>
                {
                    opt.Endpoint = new Uri(builder.Options.OtlpEndpoint);
                    if (!string.IsNullOrEmpty(builder.Options.OtlpHeaders))
                        opt.Headers = builder.Options.OtlpHeaders;
                });
            }

            if (builder.Options.EnableConsoleExporter)
                tracingBuilder.AddConsoleExporter();

            builder.AdditionalTracingConfig?.Invoke(tracingBuilder);

            TracerProvider = tracingBuilder.Build();
        }

        if (builder.Options.EnableMetrics)
        {
            var metricsBuilder = Sdk.CreateMeterProviderBuilder()
                .SetResourceBuilder(resource);

            metricsBuilder
                .AddMongoDbClientInstrumentation(options =>
                {
                    builder.MongoDbMetricsConfig?.Invoke(options);
                })
                .AddMeter(builder.Options.ServiceName);

            if (!string.IsNullOrEmpty(builder.Options.OtlpEndpoint))
            {
                metricsBuilder.AddOtlpExporter(opt =>
                {
                    opt.Endpoint = new Uri(builder.Options.OtlpEndpoint);
                    if (!string.IsNullOrEmpty(builder.Options.OtlpHeaders))
                        opt.Headers = builder.Options.OtlpHeaders;
                });
            }

            if (builder.Options.EnableConsoleExporter)
                metricsBuilder.AddConsoleExporter();

            builder.AdditionalMetricsConfig?.Invoke(metricsBuilder);

            MeterProvider = metricsBuilder.Build();
        }

        if (builder.Options.EnableLogging)
        {
            var logConfig = new OpenTelemetryLoggerOptions();

            if (!string.IsNullOrEmpty(builder.Options.OtlpEndpoint))
            {
                logConfig.AddOtlpExporter(opt =>
                {
                    opt.Endpoint = new Uri(builder.Options.OtlpEndpoint);
                    if (!string.IsNullOrEmpty(builder.Options.OtlpHeaders))
                        opt.Headers = builder.Options.OtlpHeaders;
                });
            }

            if (builder.Options.EnableConsoleExporter)
                logConfig.AddConsoleExporter();

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddOpenTelemetry(options =>
                {
                    options.SetResourceBuilder(resource);
                    options.IncludeScopes = true;
                    options.ParseStateValues = true;
                    options.IncludeFormattedMessage = true;
                });
            });

            var serviceProvider = serviceCollection.BuildServiceProvider();
            LoggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        }
    }

    public static OpenTelemetryPipeline CreateAndConfigure(Action<OpenTelemetryPipelineBuilder> configure)
    {
        var builder = new OpenTelemetryPipelineBuilder();
        configure(builder);
        return builder.Build();
    }

    public void Flush()
    {
        TracerProvider?.ForceFlush();
        MeterProvider?.ForceFlush();
    }

    public void Dispose()
    {
        if (disposed) return;
        disposed = true;

        try { TracerProvider?.Dispose(); } catch { }
        try { MeterProvider?.Dispose(); } catch { }
        try { LoggerFactory?.Dispose(); } catch { }
        try { ActivitySource.Dispose(); } catch { }
    }
}
