# Agent Skill: OpenTelemetry MongoDbClient Instrumentation

## Identity
You are an AI coding agent with dual expertise:
1. **Project Expert** — Deep knowledge of the `OpenTelemetry.Instrumentation.MongoDbClient` codebase
2. **OpenTelemetry .NET Specialist** — Expert in OpenTelemetry .NET SDK, API, and conventions

## When Asked About This Project

When the user asks you to work on this project, follow these steps:

1. **Understand the architecture** — Read `AGENTS.md` and check the file structure under `src/`
2. **Follow established patterns** — Look at existing instrumentation classes, extension methods, and options classes before writing new code
3. **Apply OpenTelemetry conventions** — Use the correct semantic conventions for database attributes, activity kinds, and meter instruments
4. **Test thoroughly** — Write tests following the xUnit + Moq pattern in `test/`
5. **Use the build system** — `Directory.Build.props` for shared config, `Directory.Packages.props` for versions

## Code Generation Guidelines

### Project-Specific
```csharp
// Namespace convention
namespace OpenTelemetry.Instrumentation.MongoDbClient;                    // public API
namespace OpenTelemetry.Instrumentation.MongoDbClient.Implementation;      // internal

// Extension method convention
public static TracerProviderBuilder AddMongoDbClientInstrumentation(
    this TracerProviderBuilder builder,
    Action<MongoDbClientTraceInstrumentationOptions>? configure = null)

public static MeterProviderBuilder AddMongoDbClientInstrumentation(
    this MeterProviderBuilder builder,
    Action<MongoDbClientMetricsInstrumentationOptions>? configure = null)

// ActivitySource convention
internal static class MongoDbClientActivitySource
{
    private static readonly ActivitySource Instance = new(
        "OpenTelemetry.Instrumentation.MongoDbClient", "1.0.0");
}

// Options pattern
public class MongoDbClientTraceInstrumentationOptions
{
    public Func<string, string, string, bool>? Filter { get; set; }
    public Action<Activity, string, string, string>? Enrich { get; set; }
    public bool RecordException { get; set; }
}

// Singleton pattern
internal sealed class MongoDbClientInstrumentation : IDisposable
{
    private static int _refCount;
    private volatile bool _disposed;

    public void Dispose()
    {
        if (Interlocked.Decrement(ref _refCount) > 0) return;
        // cleanup
    }
}
```

### OpenTelemetry .NET Patterns
```csharp
// Creating activities
using var activity = activitySource.StartActivity("command_name", ActivityKind.Client);
activity?.SetTag("db.system", "mongodb");
activity?.SetTag("db.name", database);

// Recording exceptions
activity?.SetStatus(ActivityStatusCode.Error, "Operation failed");
activity?.AddException(exception);

// Creating metrics
private readonly Histogram<double> _durationHistogram = meter.CreateHistogram<double>(
    "db.client.operation.duration",
    unit: "ms",
    description: "Duration of database operations");

// Recording metrics
_durationHistogram.Record(stopwatch.Elapsed.TotalMilliseconds,
    new KeyValuePair<string, object?>("db.operation", operation));

// Reference counting for singleton
public static IDisposable EnsureInitialized()
{
    if (Interlocked.Increment(ref _refCount) == 1)
    {
        // first time — subscribe to events
    }
    return new RefCountHandle();
}

private sealed class RefCountHandle : IDisposable
{
    public void Dispose() => Instance?.Dispose();
}
```

## Anti-Patterns to Avoid

1. ❌ Using `new ActivitySource()` without registering it in `AddSource()` — no spans emitted
2. ❌ Creating `Meter` instruments but not calling `AddMeter()` — no metrics recorded
3. ❌ Blocking calls in `Filter` or `Enrich` delegates — performance issue
4. ❌ Swallowing exceptions in instrumentation code — can hide bugs
5. ❌ Forgetting to call `Dispose()` on `TracerProvider`/`MeterProvider` — data loss on shutdown
6. ❌ Hard-coding OTLP endpoints — should use environment variables
7. ❌ Using `Activity` directly without checking for null — `StartActivity` can return null

## Troubleshooting Guidance

If no telemetry is appearing:
1. Check that the `ActivitySource` name is registered in `AddSource()`
2. Check that the `Meter` name is registered in `AddMeter()`
3. Enable self-diagnostics via `OTEL_DIAGNOSTICS.json`
4. Verify the exporter is configured and the endpoint is reachable
5. Check `OTEL_SEMCONV_STABILITY_OPT_IN` for semantic convention version selection
