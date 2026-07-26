namespace MongoDbClientSample.PipelineExtension;

public class PipelineOptions
{
    public string ServiceName { get; set; } = "unknown-service";
    public string? ServiceVersion { get; set; }
    public string? DeploymentEnvironment { get; set; }
    public string? OtlpEndpoint { get; set; }
    public string? OtlpHeaders { get; set; }
    public bool EnableConsoleExporter { get; set; } = true;
    public bool EnableTracing { get; set; } = true;
    public bool EnableMetrics { get; set; } = true;
    public bool EnableLogging { get; set; } = false;
}
