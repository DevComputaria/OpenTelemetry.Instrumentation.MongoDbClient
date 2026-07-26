using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDbClientSample.Configuration;

namespace MongoDbClientSample.Services;

public class MongoDbDemoService
{
    private readonly MongoClient _client;
    private readonly MongoDbSettings _settings;
    private readonly ActivitySource _activitySource;
    private readonly ILogger<MongoDbDemoService>? _logger;

    public MongoDbDemoService(
        MongoDbSettings settings,
        ActivitySource activitySource,
        ILogger<MongoDbDemoService>? logger = null)
    {
        _settings = settings;
        _activitySource = activitySource;
        _logger = logger;

        var mongoSettings = MongoClientSettings.FromConnectionString(settings.ConnectionString);
        OpenTelemetry.Instrumentation.MongoDbClient.MongoDbInstrumentation.ConfigureClientSettings(mongoSettings);

        _client = new MongoClient(mongoSettings);
    }

    public async Task RunAsync()
    {
        _logger?.LogInformation("Starting MongoDB demo with database: {Database}, collection: {Collection}",
            _settings.DatabaseName, _settings.CollectionName);

        var database = _client.GetDatabase(_settings.DatabaseName);
        var collection = database.GetCollection<BsonDocument>(_settings.CollectionName);

        using var parentActivity = _activitySource.StartActivity("MongoDbSampleOperations", ActivityKind.Internal);
        parentActivity?.SetTag("sample.operation_group", "full_demo");

        Console.WriteLine($"\nConnected to MongoDB at: {_settings.ConnectionString}");
        Console.WriteLine($"Database: {_settings.DatabaseName}, Collection: {_settings.CollectionName}");

        await CleanCollection(collection, database);
        await InsertDocuments(collection);
        await QueryDocuments(collection);
        await FilteredQuery(collection);
        await UpdateDocument(collection);
        await DeleteDocument(collection);
        await ShowFinalState(collection);

        _logger?.LogInformation("MongoDB demo completed successfully");
    }

    private async Task CleanCollection(IMongoCollection<BsonDocument> collection, IMongoDatabase database)
    {
        try
        {
            await database.DropCollectionAsync(_settings.CollectionName);
            Console.WriteLine("Dropped existing collection (if any)");
        }
        catch (Exception ex)
        {
            _logger?.LogWarning("Could not drop collection: {Message}", ex.Message);
            Console.WriteLine($"Note: Could not drop collection: {ex.Message}");
        }
    }

    private async Task InsertDocuments(IMongoCollection<BsonDocument> collection)
    {
        Console.WriteLine("\nInserting sample documents...");
        var documents = new List<BsonDocument>
        {
            new BsonDocument { { "name", "Document 1" }, { "value", 42 }, { "created", DateTime.UtcNow } },
            new BsonDocument { { "name", "Document 2" }, { "value", 73 }, { "created", DateTime.UtcNow } },
            new BsonDocument { { "name", "Document 3" }, { "value", 99 }, { "created", DateTime.UtcNow } }
        };

        await collection.InsertManyAsync(documents);
        _logger?.LogInformation("Inserted {Count} documents", documents.Count);
        Console.WriteLine($"Inserted {documents.Count} documents");
    }

    private async Task QueryDocuments(IMongoCollection<BsonDocument> collection)
    {
        Console.WriteLine("\nQuerying for documents...");
        var result = await collection.Find(Builders<BsonDocument>.Filter.Empty).ToListAsync();
        Console.WriteLine($"Found {result.Count} documents:");
        foreach (var doc in result)
            Console.WriteLine($"- {doc["name"]}: value = {doc["value"]}");
    }

    private async Task FilteredQuery(IMongoCollection<BsonDocument> collection)
    {
        Console.WriteLine("\nQuerying for documents with value > 50...");
        var filter = Builders<BsonDocument>.Filter.Gt("value", 50);
        var result = await collection.Find(filter).ToListAsync();
        Console.WriteLine($"Found {result.Count} documents with value > 50:");
        foreach (var doc in result)
            Console.WriteLine($"- {doc["name"]}: value = {doc["value"]}");
    }

    private async Task UpdateDocument(IMongoCollection<BsonDocument> collection)
    {
        Console.WriteLine("\nUpdating Document 1...");
        var filter = Builders<BsonDocument>.Filter.Eq("name", "Document 1");
        var update = Builders<BsonDocument>.Update.Set("value", 100).Set("updated", DateTime.UtcNow);
        await collection.UpdateOneAsync(filter, update);
        _logger?.LogInformation("Update completed");
        Console.WriteLine("Update completed");
    }

    private async Task DeleteDocument(IMongoCollection<BsonDocument> collection)
    {
        Console.WriteLine("\nDeleting Document 3...");
        var filter = Builders<BsonDocument>.Filter.Eq("name", "Document 3");
        await collection.DeleteOneAsync(filter);
        _logger?.LogInformation("Delete completed");
        Console.WriteLine("Delete completed");
    }

    private async Task ShowFinalState(IMongoCollection<BsonDocument> collection)
    {
        Console.WriteLine("\nFinal collection state:");
        var result = await collection.Find(Builders<BsonDocument>.Filter.Empty).ToListAsync();
        foreach (var doc in result)
            Console.WriteLine($"- {doc["name"]}: value = {doc["value"]}");
    }
}
