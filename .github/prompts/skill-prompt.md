# OpenTelemetry MongoDbClient Instrumentation Skill

## Context
This project provides auto-instrumentation for MongoDB.Driver.Core using OpenTelemetry .NET. It creates diagnostic activities and metrics for each MongoDB operation.

## Capabilities
1. **Tracing**: Automatically creates spans for MongoDB commands (find, insert, update, delete, etc.)
2. **Metrics**: Records duration and count of MongoDB operations
3. **Filtering**: Users can filter which operations generate telemetry
4. **Enrichment**: Users can add custom attributes to spans
5. **Exception Recording**: Captures exceptions as span events

## Architecture
```
Extension Methods → MongoDbClientInstrumentation (singleton) → DiagnosticListener/EventSubscriber → Activity/Metrics
```

## Code Patterns
- Thread-safe singleton via `Interlocked` for reference counting
- Reflection-based subscription to MongoDB driver events
- `ActivitySource` for spans, `Meter` for metrics
- Options pattern via `IOptionsMonitor<T>`

## Build & Run
- Library: `dotnet build src/`
- Tests: `dotnet test test/`
- Sample: `make run` (starts Docker infra + builds + runs)

## Key Files
- `src/.../MongoDbClientInstrumentation.cs` — core class
- `src/.../TracerProviderBuilderExtensions.cs` — tracing entry point
- `src/.../MeterProviderBuilderExtensions.cs` — metrics entry point
- `src/.../MongoDbClientTraceInstrumentationOptions.cs` — tracing options
- `src/.../MongoDbClientMetricsInstrumentationOptions.cs` — metrics options
- `src/.../Implementation/MongoDbClientActivitySource.cs` — activity creation
- `src/.../Implementation/MongoDbClientMetrics.cs` — metrics recording
- `src/.../Implementation/MongoDbClientEventSource.cs` — logging

## Versioning
MinVer-based: tag `Instrumentation.MongoDbClient-X.Y.Z` to release version X.Y.Z.
