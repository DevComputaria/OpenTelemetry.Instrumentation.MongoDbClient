# Architecture Guidelines for Instrumentation Libraries

This document outlines the architectural patterns and best practices for developing OpenTelemetry instrumentation libraries, based on the SqlClient instrumentation approach.

## Overall Architecture

OpenTelemetry instrumentation libraries typically follow a layered architecture:

1. **Public API Layer** - Extension methods and configuration options
2. **Instrumentation Layer** - Core instrumentation logic
3. **Implementation Layer** - Technology-specific implementation details

## Key Components

### Extension Methods

Extension methods are the primary entry point for users to add instrumentation to their applications. Typically, they extend:

- `TracerProviderBuilder` for tracing
- `MeterProviderBuilder` for metrics

These should follow a consistent pattern across instrumentation libraries:

```csharp
public static TracerProviderBuilder AddMongoDbClientInstrumentation(
    this TracerProviderBuilder builder,
    Action<MongoDbClientInstrumentationOptions>? configure = null)
{
    // Implementation
}
```

### Options Classes

Each instrumentation library should have an options class that allows configuration:

- `{LibraryName}TraceInstrumentationOptions` - For tracing configuration
- `{LibraryName}MeterInstrumentationOptions` - For metrics configuration (if needed)

Common options include:
- `Filter` - To filter what gets instrumented
- `Enrich` - To add custom attributes to spans/metrics
- Configuration for specific recording behaviors

### Instrumentation Class

A central instrumentation class manages the lifecycle of the instrumentation:

- Singleton instance pattern
- Implements `IDisposable` for cleanup
- Handles instrumentation subscription and unsubscription
- Manages reference counting for handles

### ActivitySource and Metric Instruments

- Define a central `ActivitySource` for the library
- Define metrics in a consistent location
- Use semantic conventions for naming

### Diagnostic Listeners and Event Handlers

For libraries that emit diagnostic events:

- Create listeners for DiagnosticSource events
- Use event source listeners for EventSource-based events
- Handle activity creation and enrichment

## Cross-Cutting Concerns

### Logging

Use an `EventSource`-derived class for internal logging:

```csharp
[EventSource(Name = "OpenTelemetry-Instrumentation-MongoDbClient")]
internal sealed class MongoDbClientInstrumentationEventSource : EventSource
{
    // Implementation
}
```

### Versioning and Compatibility

- Support multiple versions of the target library when possible
- Consider backward compatibility
- Document version requirements clearly

### Resource Management

- Properly dispose of resources
- Use reference counting for shared resources
- Consider thread safety for shared state

## Processing Pipeline

A typical instrumentation processing pipeline includes:

1. **Interception** - Hook into API calls via DiagnosticSource, EventSource, or other mechanisms
2. **Filtering** - Apply user-provided filters
3. **Activity Creation** - Create and populate activities with attributes
4. **Enrichment** - Apply user-provided enrichment
5. **Processing** - Record additional metrics or logs
6. **Completion** - End activities and record results

## Integration Patterns

### Configuration Integration

- Support named options for container scenarios
- Support environment variable configuration
- Follow consistent patterns for configuration

### Runtime Detection

- Detect library versions at runtime when needed
- Adapt behavior based on detected features
- Gracefully handle missing dependencies

## Testing Approach

- Unit test individual components
- Integration test with the actual library
- Test with different library versions
- Test configuration options thoroughly

## Example Architecture

For a MongoDB client instrumentation library, consider the following structure:

