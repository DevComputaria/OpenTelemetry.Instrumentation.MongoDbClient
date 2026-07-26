using System;
using System.Collections.Generic;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace MongoDbClientSample.PipelineExtension;

public class OpenTelemetryPipelineBuilder
{
    internal PipelineOptions Options { get; } = new();
    internal Action<OpenTelemetry.Trace.TracerProviderBuilder>? AdditionalTracingConfig { get; private set; }
    internal Action<OpenTelemetry.Metrics.MeterProviderBuilder>? AdditionalMetricsConfig { get; private set; }
    internal Action<OpenTelemetry.Instrumentation.MongoDbClient.MongoDbClientTraceInstrumentationOptions>? MongoDbTracingConfig { get; private set; }
    internal Action<OpenTelemetry.Instrumentation.MongoDbClient.MongoDbClientMetricsInstrumentationOptions>? MongoDbMetricsConfig { get; private set; }
    internal Sampler? Sampler { get; private set; }

    internal ResourceBuilder ResourceBuilder { get; private set; } = ResourceBuilder.CreateDefault();

    public OpenTelemetryPipelineBuilder WithServiceInfo(string name, string? version = null, string? environment = null)
    {
        Options.ServiceName = name;
        Options.ServiceVersion = version;
        Options.DeploymentEnvironment = environment;
        return this;
    }

    public OpenTelemetryPipelineBuilder WithOtlpExporter(string endpoint, string? headers = null)
    {
        Options.OtlpEndpoint = endpoint;
        Options.OtlpHeaders = headers;
        return this;
    }

    public OpenTelemetryPipelineBuilder WithConsoleExporter(bool enable = true)
    {
        Options.EnableConsoleExporter = enable;
        return this;
    }

    public OpenTelemetryPipelineBuilder WithSampler(Sampler sampler)
    {
        Sampler = sampler;
        return this;
    }

    public OpenTelemetryPipelineBuilder ConfigureMongoDbTracing(
        Action<OpenTelemetry.Instrumentation.MongoDbClient.MongoDbClientTraceInstrumentationOptions>? configure)
    {
        MongoDbTracingConfig = configure;
        return this;
    }

    public OpenTelemetryPipelineBuilder ConfigureMongoDbMetrics(
        Action<OpenTelemetry.Instrumentation.MongoDbClient.MongoDbClientMetricsInstrumentationOptions>? configure)
    {
        MongoDbMetricsConfig = configure;
        return this;
    }

    public OpenTelemetryPipelineBuilder WithAdditionalTracing(Action<OpenTelemetry.Trace.TracerProviderBuilder> configure)
    {
        AdditionalTracingConfig = configure;
        return this;
    }

    public OpenTelemetryPipelineBuilder WithAdditionalMetrics(Action<OpenTelemetry.Metrics.MeterProviderBuilder> configure)
    {
        AdditionalMetricsConfig = configure;
        return this;
    }

    public OpenTelemetryPipeline Build()
    {
        var resourceAttrs = new Dictionary<string, object>();
        if (!string.IsNullOrEmpty(Options.DeploymentEnvironment))
            resourceAttrs["deployment.environment"] = Options.DeploymentEnvironment;

        ResourceBuilder = ResourceBuilder.CreateDefault()
            .AddService(Options.ServiceName, Options.ServiceVersion)
            .AddAttributes(resourceAttrs);

        return new OpenTelemetryPipeline(this);
    }
}
