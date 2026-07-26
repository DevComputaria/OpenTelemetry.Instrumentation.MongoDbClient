namespace MongoDbClientSample.Configuration;

public class OpenTelemetrySettings
{
    public string ServiceName { get; set; } = "MongoDbClientSample";
    public string? ServiceVersion { get; set; } = "1.0.0";
    public string? DeploymentEnvironment { get; set; } = "development";
    public string? OtlpEndpoint { get; set; } = "http://localhost:4317";
    public string? OtlpHeaders { get; set; }
    public bool EnableConsoleExporter { get; set; } = true;
    public bool EnableTracing { get; set; } = true;
    public bool EnableMetrics { get; set; } = true;
    public bool EnableLogging { get; set; }
    public string? Sampler { get; set; } = "always_on";
}
