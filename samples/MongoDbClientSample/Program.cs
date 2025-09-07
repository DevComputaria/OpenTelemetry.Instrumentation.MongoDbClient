// See https://aka.ms/new-console-template for more information
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
using OpenTelemetry.Instrumentation.MongoDbClient;
using System.Diagnostics;

// Configuration values (in a real application, these would come from appsettings.json or environment variables)
var configuration = new Dictionary<string, string>
{
    ["ServiceName"] = "MongoDbClientSample",
    ["ServiceVersion"] = "1.0.0",
    ["OTLPEndpoint"] = "http://localhost:4317", // Change to your actual OTLP endpoint
    ["OTLPToken"] = "your-auth-token"           // Change to your actual auth token if needed
};

// Create a resource that identifies your service
var resource = ResourceBuilder.CreateDefault()
    .AddService(configuration["ServiceName"], configuration["ServiceVersion"])
    .AddAttributes(new Dictionary<string, object>
    {
        ["deployment.environment"] = "development"
    });

// Create a custom ActivitySource for the application
var activitySource = new ActivitySource("MongoDbClientSample.App");

// Configure OpenTelemetry tracing
using var tracerProvider = Sdk.CreateTracerProviderBuilder()
    .SetResourceBuilder(resource)
    // Add MongoDB client instrumentation
    .AddMongoDbClientInstrumentation(options =>
    {
        // Enable exception recording
        options.RecordException = true;
        
        // Filter to exclude certain operations if needed
        options.Filter = (command, database, collection) => 
            // In this example, we trace everything
            true;
            
        // Enrich spans with custom attributes
        options.EnrichActivity = (activity, command, database, collection) =>
        {
            activity.SetTag("sample.is_demo", true);
            
            // You can add different tags based on the operation
            if (command == "find")
            {
                activity.SetTag("sample.operation_type", "read");
            }
            else if (command == "insert")
            {
                activity.SetTag("sample.operation_type", "write");
            }
        };
    })
    // Add other common instrumentations
    .AddHttpClientInstrumentation()
    .AddSqlClientInstrumentation()
    // Add source for the application's own activities
    .AddSource("MongoDbClientSample.App")
    // Set sampling strategy
    .SetSampler(new AlwaysOnSampler())
    // Add exporters
    .AddConsoleExporter()
    .AddOtlpExporter(opt =>
    {
        opt.Endpoint = new Uri(configuration["OTLPEndpoint"]);
        opt.Headers = $"Authorization=Bearer {configuration["OTLPToken"]}";
    })
    .Build();

// Configure OpenTelemetry metrics
using var meterProvider = Sdk.CreateMeterProviderBuilder()
    .SetResourceBuilder(resource)
    // Add MongoDB client metrics instrumentation
    .AddMongoDbClientInstrumentation()
    // Add runtime metrics 
    .AddRuntimeInstrumentation()
    // Add metrics from other libraries
    .AddMeter("Microsoft.AspNetCore.Hosting")
    .AddMeter("Microsoft.AspNetCore.Server.Kestrel")
    // Add exporters
    .AddConsoleExporter()
    .AddOtlpExporter(opt =>
    {
        opt.Endpoint = new Uri(configuration["OTLPEndpoint"]);
        opt.Headers = $"Authorization=Bearer {configuration["OTLPToken"]}";
    })
    .Build();

// Configure OpenTelemetry logging (note: console app logging requires additional setup)
// For demonstration purposes, this shows how you would configure it
/*
using var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddOpenTelemetry(options =>
    {
        options.SetResourceBuilder(resource);
        options.AddConsoleExporter();
        options.AddOtlpExporter(opt =>
        {
            opt.Endpoint = new Uri(configuration["OTLPEndpoint"]);
            opt.Headers = $"Authorization=Bearer {configuration["OTLPToken"]}";
        });
    });
});
var logger = loggerFactory.CreateLogger<Program>();
*/

try
{
    Console.WriteLine("=== MongoDB Client Sample with OpenTelemetry Instrumentation ===");
    
    // Connection string - change this to your MongoDB instance if available
    string connectionString = "mongodb://localhost:27017";
    Console.WriteLine($"Connecting to MongoDB at: {connectionString}");
    
    // Create a MongoDB client - this will be automatically instrumented
    var client = new MongoClient(connectionString);
    
    // Create and start a custom activity to wrap multiple operations
    using var activity = activitySource.StartActivity("MongoDbSampleOperations", ActivityKind.Internal);
    activity?.SetTag("sample.operation_group", "full_demo");
    
    // Get a database reference
    string databaseName = "sample_db";
    var database = client.GetDatabase(databaseName);
    Console.WriteLine($"Using database: {databaseName}");
    
    // Get a collection reference
    string collectionName = "sample_collection";
    var collection = database.GetCollection<BsonDocument>(collectionName);
    Console.WriteLine($"Using collection: {collectionName}");
    
    try
    {
        // Drop collection if it exists to start fresh
        database.DropCollection(collectionName);
        Console.WriteLine("Dropped existing collection (if any)");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Note: Could not drop collection: {ex.Message}");
    }
    
    // Insert some sample documents
    Console.WriteLine("\nInserting sample documents...");
    var documents = new List<BsonDocument>
    {
        new BsonDocument { { "name", "Document 1" }, { "value", 42 }, { "created", DateTime.UtcNow } },
        new BsonDocument { { "name", "Document 2" }, { "value", 73 }, { "created", DateTime.UtcNow } },
        new BsonDocument { { "name", "Document 3" }, { "value", 99 }, { "created", DateTime.UtcNow } }
    };
    
    collection.InsertMany(documents);
    Console.WriteLine($"Inserted {documents.Count} documents");
    
    // Perform a query
    Console.WriteLine("\nQuerying for documents...");
    var filter = Builders<BsonDocument>.Filter.Empty;
    var result = collection.Find(filter).ToList();
    Console.WriteLine($"Found {result.Count} documents:");
    
    foreach (var doc in result)
    {
        Console.WriteLine($"- {doc["name"]}: value = {doc["value"]}");
    }
    
    // Perform a filtered query
    Console.WriteLine("\nQuerying for documents with value > 50...");
    var valueFilter = Builders<BsonDocument>.Filter.Gt("value", 50);
    var filteredResult = collection.Find(valueFilter).ToList();
    Console.WriteLine($"Found {filteredResult.Count} documents with value > 50:");
    
    foreach (var doc in filteredResult)
    {
        Console.WriteLine($"- {doc["name"]}: value = {doc["value"]}");
    }
    
    // Update a document
    Console.WriteLine("\nUpdating Document 1...");
    var updateFilter = Builders<BsonDocument>.Filter.Eq("name", "Document 1");
    var update = Builders<BsonDocument>.Update.Set("value", 100).Set("updated", DateTime.UtcNow);
    collection.UpdateOne(updateFilter, update);
    Console.WriteLine("Update completed");
    
    // Delete a document
    Console.WriteLine("\nDeleting Document 3...");
    var deleteFilter = Builders<BsonDocument>.Filter.Eq("name", "Document 3");
    collection.DeleteOne(deleteFilter);
    Console.WriteLine("Delete completed");
    
    // Get final state
    Console.WriteLine("\nFinal collection state:");
    var finalResult = collection.Find(Builders<BsonDocument>.Filter.Empty).ToList();
    foreach (var doc in finalResult)
    {
        Console.WriteLine($"- {doc["name"]}: value = {doc["value"]}");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"MongoDB Error: {ex.Message}");
    Console.WriteLine("Note: If you don't have MongoDB running locally, modify the connection string.");
    Console.WriteLine("You may need to start MongoDB or connect to a different instance.");
}

Console.WriteLine("\nPress any key to exit...");
Console.ReadKey();
