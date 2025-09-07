# Best Practices for OpenTelemetry Instrumentation

This document outlines best practices for implementing OpenTelemetry instrumentation libraries, based on the SqlClient instrumentation pattern.

## General Principles

1. **Follow OpenTelemetry Semantic Conventions**
   - Adhere to [OpenTelemetry Semantic Conventions](https://github.com/open-telemetry/semantic-conventions) for consistency
   - Use standard attribute names for common concepts
   - Follow naming patterns for spans and metrics

2. **Minimize Performance Impact**
   - Use conditional compilation for platform-specific code
   - Avoid allocations in hot paths
   - Implement efficient sampling strategies
   - Use caching where appropriate
   - Check if listeners are enabled before performing heavy operations

3. **Ensure Backward Compatibility**
   - Support multiple versions of the target library
   - Provide graceful fallbacks for missing features
   - Document breaking changes clearly

4. **Provide Clear Configuration Options**
   - Offer sensible defaults
   - Allow customization of key behaviors
   - Document all configuration options thoroughly

## Implementation Practices

### Activity (Span) Creation

1. **Naming Activities**
   - Use consistent naming patterns: `{db.operation}` or `{library}.{operation}`
   - Follow OpenTelemetry semantic conventions

2. **Setting Attributes**
   - Set common attributes first (e.g., db.system, db.name)
   - Add operation-specific attributes
   - Sanitize sensitive information (e.g., SQL statements)
   - Use attribute limits to prevent excessive data collection

3. **Handling Context**
   - Propagate context correctly across async boundaries
   - Respect parent-child relationships
   - Use appropriate ActivityKind (Client, Server, etc.)

### Error Handling

1. **Capturing Exceptions**
   - Record exceptions on activities
   - Set appropriate status codes
   - Don't let instrumentation exceptions affect application behavior

2. **Logging Issues**
   - Log instrumentation problems through EventSource
   - Use appropriate verbosity levels
   - Include diagnostic information

### Resource Management

1. **Activity Lifecycle**
   - Always stop activities that are started
   - Use try/finally blocks to ensure proper cleanup
   - Implement IDisposable when appropriate

2. **Reference Counting**
   - Use atomic operations for thread safety
   - Balance increment/decrement operations
   - Check for zero references before cleanup

### Customization Support

1. **Enrichment**
   - Allow users to add custom attributes to activities
   - Provide context in enrichment callbacks
   - Handle exceptions in user-provided code

2. **Filtering**
   - Support filtering based on operation properties
   - Allow users to skip specific operations
   - Implement efficient filter evaluation

## Examples from SqlClient Instrumentation

### Effective Extension Method

```csharp
public static TracerProviderBuilder AddMongoDbClientInstrumentation(
    this TracerProviderBuilder builder,
    Action<MongoDbClientTraceInstrumentationOptions>? configure = null)
{
    Guard.ThrowIfNull(builder);

    if (configure != null)
    {
        builder.ConfigureServices(services => 
            services.Configure(configure));
    }

    builder.AddInstrumentation(sp =>
    {
        var options = sp.GetRequiredService<IOptionsMonitor<MongoDbClientTraceInstrumentationOptions>>().CurrentValue;
        MongoDbClientInstrumentation.TracingOptions = options;
        return MongoDbClientInstrumentation.AddTracingHandle();
    });

    builder.AddSource("OpenTelemetry.Instrumentation.MongoDbClient");

    return builder;
}
```

### Effective Activity Creation

```csharp
public static Activity? StartMongoActivity(string command, string database, string collection)
{
    var activityName = "mongodb.query";
    
    var activity = ActivitySource.StartActivity(
        activityName,
        ActivityKind.Client);
    
    if (activity == null || !activity.IsAllDataRequested)
    {
        return activity;
    }

    // Add standard attributes
    activity.SetTag(SemanticConventions.AttributeDbSystem, "mongodb");
    activity.SetTag(SemanticConventions.AttributeDbName, database);
    activity.SetTag(SemanticConventions.AttributeDbMongoDbCollection, collection);
    activity.SetTag(SemanticConventions.AttributeDbOperation, command);
    
    return activity;
}
```

### Effective Error Handling

```csharp
try
{
    // Perform operation with instrumentation
}
catch (Exception ex)
{
    if (activity?.IsAllDataRequested == true)
    {
        activity.SetStatus(ActivityStatusCode.Error, ex.Message);
        
        if (options.RecordException)
        {
            activity.RecordException(ex);
        }
    }
    
    throw;
}
finally
{
    activity?.Stop();
}
```

## Checklist for Implementing New Instrumentation

- [ ] Create appropriate extension methods
- [ ] Define instrumentation options class
- [ ] Create ActivitySource
- [ ] Implement diagnostic listeners
- [ ] Add proper error handling
- [ ] Include EventSource for logging
- [ ] Support configuration options
- [ ] Document public API
- [ ] Write unit and integration tests
- [ ] Create README with usage examples
- [ ] Follow semantic conventions

By following these best practices, you'll create instrumentation that is consistent with the OpenTelemetry ecosystem, provides valuable telemetry data, and maintains high performance.
