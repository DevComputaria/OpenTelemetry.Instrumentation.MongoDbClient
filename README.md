# OpenTelemetry.Instrumentation.MongoDbClient

This project provides auto-instrumentation for the MongoDB client using OpenTelemetry. It allows developers to easily integrate tracing capabilities into their applications that use MongoDB, enabling better observability and monitoring.

## Features

- Automatic tracing of MongoDB operations.
- Configuration options to enable or disable specific features.
- Integration with OpenTelemetry for seamless observability.
- MongoDB operation metrics collection.
- Support for filtering and enriching telemetry data.

## Getting Started

### Prerequisites

- .NET 8 SDK or later
- MongoDB.Driver NuGet package (version 2.22.0 or later recommended for .NET 8)

### Installation

To install the MongoDB client instrumentation, add the following package references to your project:

```xml
<PackageReference Include="OpenTelemetry.Instrumentation.MongoDbClient" Version="1.0.0" />
<PackageReference Include="MongoDB.Driver" Version="2.22.0" />
```

### Usage

#### Tracing Configuration

Configure the tracer provider to include MongoDB client instrumentation:

```csharp
using OpenTelemetry;
using OpenTelemetry.Trace;

var tracerProvider = Sdk.CreateTracerProviderBuilder()
    .AddMongoDbClientInstrumentation(options =>
    {
        // Configure options here
    })
    .Build();
```

#### Metrics Configuration

Configure the meter provider to collect MongoDB client metrics:

```csharp
using OpenTelemetry;
using OpenTelemetry.Metrics;

var meterProvider = Sdk.CreateMeterProviderBuilder()
    .AddMongoDbClientInstrumentation()
    .Build();
```

#### Advanced Configuration Options

##### Filtering Operations

You can filter which MongoDB operations generate telemetry:

```csharp
.AddMongoDbClientInstrumentation(options =>
{
    options.Filter = (command, database, collection) =>
    {
        // Skip tracing for certain collections
        if (collection == "sensitive_data")
            return false;
            
        // Skip specific commands
        if (command == "isMaster")
            return false;
            
        return true;
    };
})
```

> **Note**: Filtering allows you to reduce the volume of telemetry data by excluding operations you don't need to monitor.

##### Enriching Spans with Custom Attributes

Add custom attributes to your MongoDB operation spans:

```csharp
.AddMongoDbClientInstrumentation(options =>
{
    options.Enrich = (activity, command, database, collection) =>
    {
        // Add custom attributes
        activity.SetTag("tenant.id", GetCurrentTenantId());
        activity.SetTag("environment", "production");
        
        // You can add attributes based on the operation details
        if (collection == "users")
        {
            activity.SetTag("user.operation", true);
        }
    };
})
```

> **Note**: Enrichment enables you to add business context to your telemetry data, making it more valuable for troubleshooting and analysis.

##### Recording Exceptions

Configure whether and how exceptions are recorded:

```csharp
.AddMongoDbClientInstrumentation(options =>
{
    // Enable capturing of exceptions as span events
    options.RecordException = true;
    
    // Optionally configure exception handling
    options.SetExceptionAsErrorStatus = true;
})
```

> **Note**: Recording exceptions provides valuable information for diagnosing failures, but may increase the size of your telemetry data.

##### Complete Example

Here's a complete example combining multiple configuration options:

```csharp
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;

// Create shared resource
var resource = ResourceBuilder.CreateDefault()
    .AddService("MyServiceName", serviceVersion: "1.0.0")
    .AddAttributes(new Dictionary<string, object>
    {
        ["deployment.environment"] = "production"
    });

// Configure tracing
var tracerProvider = Sdk.CreateTracerProviderBuilder()
    .SetResourceBuilder(resource)
    .AddMongoDbClientInstrumentation(options =>
    {
        options.RecordException = true;
        options.SetExceptionAsErrorStatus = true;
        
        options.Filter = (command, database, collection) => 
            collection != "sensitive_data";
            
        options.Enrich = (activity, command, database, collection) =>
        {
            activity.SetTag("db.operation.priority", 
                command == "find" ? "high" : "normal");
        };
    })
    .AddConsoleExporter()
    .Build();

// Configure metrics
var meterProvider = Sdk.CreateMeterProviderBuilder()
    .SetResourceBuilder(resource)
    .AddMongoDbClientInstrumentation()
    .AddConsoleExporter()
    .Build();

// Your application code that uses MongoDB
using (var client = new MongoClient("mongodb://localhost:27017"))
{
    // Operations will be automatically instrumented
    var database = client.GetDatabase("mydb");
    var collection = database.GetCollection<BsonDocument>("mycollection");
    var result = collection.Find(FilterDefinition<BsonDocument>.Empty).ToList();
}

// Clean up providers when done
tracerProvider.Dispose();
meterProvider.Dispose();
```

2. Use the MongoDB client as usual. The instrumentation will automatically create spans for database operations.

### Compatibility

This instrumentation package has been tested with the following MongoDB driver versions:

| MongoDB.Driver Version | .NET Version | Compatibility Status |
|------------------------|--------------|----------------------|
| 2.22.0+                | .NET 8.0+    | Fully supported      |
| 2.19.0 - 2.21.0        | .NET 8.0+    | Supported            |
| 2.15.0 - 2.18.0        | .NET 6.0+    | Limited support*     |

*Limited support means basic functionality works but some advanced features may not be available.

### Contributing

Contributions are welcome! Please follow these steps to contribute:

1. Fork the repository.
2. Create a new branch for your feature or bug fix.
3. Make your changes and commit them.
4. Submit a pull request.

### License

This project is licensed under the MIT License. See the LICENSE file for more details.