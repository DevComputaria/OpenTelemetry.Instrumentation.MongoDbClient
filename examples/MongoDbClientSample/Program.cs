using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDbClientSample.Configuration;
using MongoDbClientSample.PipelineExtension;
using MongoDbClientSample.Services;

var config = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: true)
    .Build();

var otelSettings = config.GetSection("OpenTelemetry").Get<OpenTelemetrySettings>() ?? new();
var mongoSettings = config.GetSection("MongoDb").Get<MongoDbSettings>() ?? new();

using var pipeline = OpenTelemetryPipeline.CreateAndConfigure(builder =>
{
    builder
        .WithServiceInfo(otelSettings.ServiceName, otelSettings.ServiceVersion, otelSettings.DeploymentEnvironment)
        .ConfigureMongoDbTracing(options =>
        {
            options.RecordException = true;
            options.CaptureCommandText = true;
        });

    if (!string.IsNullOrEmpty(otelSettings.OtlpEndpoint))
        builder.WithOtlpExporter(otelSettings.OtlpEndpoint, otelSettings.OtlpHeaders);

    if (otelSettings.EnableConsoleExporter)
        builder.WithConsoleExporter(true);
});

var loggerFactory = pipeline.LoggerFactory;
var logger = loggerFactory?.CreateLogger<Program>();

logger?.LogInformation("OpenTelemetry pipeline initialized. Service: {Service}, Version: {Version}",
    otelSettings.ServiceName, otelSettings.ServiceVersion);

try
{
    Console.WriteLine("=== MongoDB Client Sample with OpenTelemetry Instrumentation ===\n");

    var demo = new MongoDbDemoService(
        mongoSettings,
        pipeline.ActivitySource,
        loggerFactory?.CreateLogger<MongoDbDemoService>());

    await demo.RunAsync();
}
catch (Exception ex)
{
    logger?.LogError(ex, "MongoDB demo failed");
    Console.WriteLine($"\nMongoDB Error: {ex.Message}");
    Console.WriteLine("Note: If you don't have MongoDB running locally, modify the connection string.");
    Console.WriteLine("You may need to start MongoDB or connect to a different instance.");
}

pipeline.Flush();

Console.WriteLine("\nPress any key to exit...");
Console.ReadKey();
