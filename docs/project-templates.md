# Project Templates for OpenTelemetry Instrumentation Libraries

This document provides templates and guidelines for structuring OpenTelemetry instrumentation libraries, ensuring consistency across different instrumentations while following semantic conventions and architectural best practices.

## Project Structure

A typical OpenTelemetry instrumentation library should follow this structure:

```
OpenTelemetry.Instrumentation.{LibraryName}/
├── src/
│   ├── OpenTelemetry.Instrumentation.{LibraryName}.csproj
│   ├── AssemblyInfo.cs
│   ├── {LibraryName}InstrumentationOptions.cs
│   ├── {LibraryName}Instrumentation.cs
│   ├── TracerProviderBuilderExtensions.cs
│   ├── MeterProviderBuilderExtensions.cs (if metrics are supported)
│   ├── Implementation/
│   │   ├── {LibraryName}DiagnosticListener.cs
│   │   ├── {LibraryName}ActivitySource.cs
│   │   └── {LibraryName}InstrumentationEventSource.cs
│   └── SemanticConventions.cs
├── test/
│   ├── OpenTelemetry.Instrumentation.{LibraryName}.Tests.csproj
│   ├── {LibraryName}Tests.cs
│   └── Implementation/
│       └── {LibraryName}DiagnosticListenerTests.cs
└── README.md
```

## Project File Templates

### Project File (.csproj)

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFrameworks>net6.0;net7.0;net8.0;netstandard2.0</TargetFrameworks>
    <Description>OpenTelemetry instrumentation for {LibraryName}</Description>
    <PackageTags>opentelemetry;distributed-tracing;tracing;metrics;{LibraryName}</PackageTags>
    <MinVerTagPrefix>v</MinVerTagPrefix>
    <RootNamespace>OpenTelemetry.Instrumentation.{LibraryName}</RootNamespace>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="OpenTelemetry" Version="1.6.0" />
    <PackageReference Include="OpenTelemetry.Api" Version="1.6.0" />
    <PackageReference Include="{ActualLibraryReference}" Version="{MinSupportedVersion}" />
  </ItemGroup>
</Project>
```

### AssemblyInfo.cs

```csharp
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("OpenTelemetry.Instrumentation.{LibraryName}.Tests")]
```

## Code Templates

### Options Class

```csharp
using System;
using System.Diagnostics;

namespace OpenTelemetry.Instrumentation.{LibraryName}
{
    /// <summary>
    /// Options for {LibraryName} instrumentation.
    /// </summary>
    public class {LibraryName}InstrumentationOptions
    {
        /// <summary>
        /// Gets or sets a value indicating whether the exception will be recorded as ActivityEvent or not.
        /// </summary>
        /// <remarks>
        /// Default value: <see langword="false"/>.
        /// </remarks>
        public bool RecordException { get; set; }

        /// <summary>
        /// Gets or sets an optional filter function that determines if an activity should be created for each operation.
        /// </summary>
        /// <remarks>
        /// The filter function receives the operation information and returns a boolean indicating whether
        /// an activity should be created or not.
        /// </remarks>
        public Func<{OperationContextType}, bool>? Filter { get; set; }

        /// <summary>
        /// Gets or sets an action to enrich an Activity.
        /// </summary>
        /// <remarks>
        /// The enrichment action receives the activity object and the operation context.
        /// </remarks>
        public Action<Activity, {OperationContextType}>? Enrich { get; set; }
    }
}
```

### Instrumentation Class

```csharp
using System;
using System.Diagnostics;
using System.Threading;
using OpenTelemetry.Instrumentation.{LibraryName}.Implementation;

namespace OpenTelemetry.Instrumentation.{LibraryName}
{
    /// <summary>
    /// {LibraryName} instrumentation.
    /// </summary>
    internal sealed class {LibraryName}Instrumentation : IDisposable
    {
        private static readonly {LibraryName}InstrumentationEventSource Log = {LibraryName}InstrumentationEventSource.Log;
        private static {LibraryName}Instrumentation instance;
        private static readonly object lockObj = new();
        private readonly {LibraryName}DiagnosticListener diagnosticListener;
        private int refCount;

        internal static {LibraryName}InstrumentationOptions TracingOptions { get; set; } = new();

        private {LibraryName}Instrumentation()
        {
            try
            {
                this.diagnosticListener = new {LibraryName}DiagnosticListener(TracingOptions);
                this.diagnosticListener.Subscribe();
            }
            catch (Exception ex)
            {
                Log.Error($"Error initializing {LibraryName} instrumentation. {ex}");
                throw;
            }
        }

        /// <summary>
        /// Gets or creates the singleton instance of <see cref="{LibraryName}Instrumentation"/>.
        /// </summary>
        /// <returns>The singleton instance.</returns>
        public static {LibraryName}Instrumentation GetInstance()
        {
            if (instance == null)
            {
                lock (lockObj)
                {
                    instance ??= new {LibraryName}Instrumentation();
                }
            }

            return instance;
        }

        /// <summary>
        /// Adds a reference to the instrumentation. This tracks the number of active users.
        /// </summary>
        /// <returns>A handle that decrements the reference count when disposed.</returns>
        public static IDisposable AddTracingHandle()
        {
            var instrumentation = GetInstance();
            Interlocked.Increment(ref instrumentation.refCount);
            return new ReferenceCountedDisposable(instrumentation);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            lock (lockObj)
            {
                if (Interlocked.Decrement(ref this.refCount) <= 0 && instance != null)
                {
                    this.diagnosticListener?.Dispose();
                    instance = null;
                    TracingOptions = new();
                }
            }
        }

        private sealed class ReferenceCountedDisposable : IDisposable
        {
            private readonly {LibraryName}Instrumentation instrumentation;

            public ReferenceCountedDisposable({LibraryName}Instrumentation instrumentation)
            {
                this.instrumentation = instrumentation;
            }

            public void Dispose()
            {
                this.instrumentation.Dispose();
            }
        }
    }
}
```

### Extension Methods

```csharp
using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OpenTelemetry.Instrumentation.{LibraryName};

namespace OpenTelemetry.Trace
{
    /// <summary>
    /// Extension methods to simplify registering {LibraryName} instrumentation.
    /// </summary>
    public static class TracerProviderBuilderExtensions
    {
        /// <summary>
        /// Adds {LibraryName} instrumentation to the TracerProviderBuilder.
        /// </summary>
        /// <param name="builder">The <see cref="TracerProviderBuilder"/> to add instrumentation to.</param>
        /// <param name="configure">Optional configuration options for the instrumentation.</param>
        /// <returns>The <see cref="TracerProviderBuilder"/> for chaining.</returns>
        public static TracerProviderBuilder Add{LibraryName}Instrumentation(
            this TracerProviderBuilder builder,
            Action<{LibraryName}InstrumentationOptions>? configure = null)
        {
            Guard.ThrowIfNull(builder);

            if (configure != null)
            {
                builder.ConfigureServices(services => 
                    services.Configure(configure));
            }

            builder.AddInstrumentation(sp =>
            {
                var options = sp.GetRequiredService<IOptionsMonitor<{LibraryName}InstrumentationOptions>>().CurrentValue;
                {LibraryName}Instrumentation.TracingOptions = options;
                return {LibraryName}Instrumentation.AddTracingHandle();
            });

            builder.AddSource("OpenTelemetry.Instrumentation.{LibraryName}");

            return builder;
        }
    }
}
```

### Activity Source

```csharp
using System.Diagnostics;

namespace OpenTelemetry.Instrumentation.{LibraryName}.Implementation
{
    /// <summary>
    /// Activity source for {LibraryName} instrumentation.
    /// </summary>
    internal static class {LibraryName}ActivitySource
    {
        private static readonly ActivitySource ActivitySource = new ActivitySource("OpenTelemetry.Instrumentation.{LibraryName}");

        /// <summary>
        /// Starts a new activity for {LibraryName} operations.
        /// </summary>
        /// <param name="name">The name of the activity.</param>
        /// <param name="kind">The kind of activity to create.</param>
        /// <returns>A new <see cref="Activity"/> or null if instrumentation is disabled.</returns>
        public static Activity? StartActivity(string name, ActivityKind kind = ActivityKind.Client)
        {
            return ActivitySource.StartActivity(name, kind);
        }
    }
}
```

### Event Source

```csharp
using System;
using System.Diagnostics.Tracing;

namespace OpenTelemetry.Instrumentation.{LibraryName}.Implementation
{
    [EventSource(Name = "OpenTelemetry-Instrumentation-{LibraryName}")]
    internal sealed class {LibraryName}InstrumentationEventSource : EventSource
    {
        public static {LibraryName}InstrumentationEventSource Log = new();

        [Event(1, Message = "Error occurred: {0}", Level = EventLevel.Error)]
        public void Error(string error)
        {
            this.WriteEvent(1, error);
        }

        [Event(2, Message = "Warning: {0}", Level = EventLevel.Warning)]
        public void Warning(string warning)
        {
            this.WriteEvent(2, warning);
        }

        [Event(3, Message = "Information: {0}", Level = EventLevel.Informational)]
        public void Information(string message)
        {
            this.WriteEvent(3, message);
        }
    }
}
```

### MongoDB-Specific Event Source Example

```csharp
using System;
using System.Diagnostics.Tracing;

namespace OpenTelemetry.Instrumentation.MongoDbClient.Implementation
{
    /// <summary>
    /// EventSource events for the MongoDb Client instrumentation.
    /// </summary>
    [EventSource(Name = "OpenTelemetry-Instrumentation-MongoDbClient")]
    internal sealed class MongoDbClientInstrumentationEventSource : EventSource
    {
        public static readonly MongoDbClientInstrumentationEventSource Log = new();

        [Event(1, Message = "MongoDbClient instrumentation error: {0}", Level = EventLevel.Error)]
        public void MongoDbError(string error)
        {
            this.WriteEvent(1, error);
        }

        [Event(2, Message = "MongoDbClient instrumentation warning: {0}", Level = EventLevel.Warning)]
        public void MongoDbWarning(string warning)
        {
            this.WriteEvent(2, warning);
        }

        [Event(3, Message = "MongoDbClient instrumentation information: {0}", Level = EventLevel.Informational)]
        public void MongoDbInformation(string message)
        {
            this.WriteEvent(3, message);
        }

        [Event(4, Message = "Failed to instrument MongoDB operation. Database: {0}, Collection: {1}, Error: {2}", Level = EventLevel.Warning)]
        public void MongoDbOperationFailed(string database, string collection, string error)
        {
            this.WriteEvent(4, database, collection, error);
        }

        [Event(5, Message = "MongoDB command exceeded duration threshold. Command: {0}, Duration: {1}ms", Level = EventLevel.Warning)]
        public void MongoDbCommandDurationExceeded(string command, long durationMilliseconds)
        {
            this.WriteEvent(5, command, durationMilliseconds);
        }
        
        [Event(6, Message = "MongoDB instrumentation is disabled for diagnostics source: {0}", Level = EventLevel.Verbose)]
        public void MongoDbInstrumentationDisabled(string diagnosticsSource)
        {
            this.WriteEvent(6, diagnosticsSource);
        }
        
        [Event(7, Message = "MongoDB instrumentation initialized successfully", Level = EventLevel.Informational)]
        public void MongoDbInstrumentationInitialized()
        {
            this.WriteEvent(7);
        }
    }
}
```

### Semantic Conventions

```csharp
namespace OpenTelemetry.Instrumentation.{LibraryName}
{
    /// <summary>
    /// Constants for semantic attribute names used by the {LibraryName} instrumentation.
    /// </summary>
    internal static class SemanticConventions
    {
        // Follow the OpenTelemetry semantic conventions
        // https://github.com/open-telemetry/semantic-conventions

        // Standard attributes
        public const string AttributeNetPeerName = "net.peer.name";
        public const string AttributeNetPeerPort = "net.peer.port";
        
        // {LibraryName}-specific attributes
        public const string AttributeLibrarySpecificName = "{library.specific.attribute}";
    }
}
```

## Implementation Guidelines

1. **Follow Semantic Conventions**: Always follow the [OpenTelemetry Semantic Conventions](https://github.com/open-telemetry/opentelemetry-specification/tree/main/specification/trace/semantic_conventions) for attribute naming and values.

2. **Version Compatibility**: Support multiple target frameworks (e.g., netstandard2.0, net6.0+) to ensure broad compatibility.

3. **Performance Considerations**:
   - Use lazy initialization where appropriate
   - Implement efficient filtering early in the pipeline
   - Avoid excessive allocations in hot paths

4. **Error Handling and Robustness**:
   - Catch and log exceptions
   - Implement proper cleanup in Dispose methods
   - Use reference counting for shared resources

5. **Testing Requirements**:
   - Unit tests for all public APIs
   - Integration tests with the actual library
   - Performance benchmarks for critical paths

## Naming Guidelines

1. **Namespaces**:
   - Use `OpenTelemetry.Instrumentation.{LibraryName}` for the main namespace
   - Use `OpenTelemetry.Trace` for extensions to TracerProviderBuilder
   - Use `OpenTelemetry.Metrics` for extensions to MeterProviderBuilder

2. **Class Names**:
   - Suffix options classes with `InstrumentationOptions`
   - Suffix event sources with `InstrumentationEventSource`
   - Use clear, descriptive names for all public APIs

## Example README Structure

```markdown
# OpenTelemetry .NET {LibraryName} Instrumentation

This library provides OpenTelemetry instrumentation for {LibraryName}.

## Installation

```shell
dotnet add package OpenTelemetry.Instrumentation.{LibraryName}
```

## Usage

```csharp
using OpenTelemetry;
using OpenTelemetry.Trace;

var tracerProvider = Sdk.CreateTracerProviderBuilder()
    .Add{LibraryName}Instrumentation(options =>
    {
        options.RecordException = true;
        options.Filter = context => context.SomeProperty == "value";
        options.Enrich = (activity, context) =>
        {
            activity.SetTag("custom.tag", "value");
        };
    })
    .Build();
```

## Configuration Options

- `RecordException` - When true, exceptions will be recorded as Activity events.
- `Filter` - Optional predicate to determine whether to instrument specific operations.
- `Enrich` - Optional action to add additional tags or context to activities.

## Supported Versions

- {LibraryName} {SupportedVersionRange}
- .NET (Core) {SupportedFrameworks}
```

By following these templates and guidelines, you'll create OpenTelemetry instrumentation libraries that are consistent, maintainable, and follow established patterns.
