// This file contains a mock implementation of the MongoDB client used for testing purposes, allowing for isolated tests without requiring a real MongoDB instance.

using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OpenTelemetry.Instrumentation.MongoDbClient.Tests.TestHelpers
{
    public class MockMongoDbClient : IMongoClient
    {
        public IMongoDatabase GetDatabase(string databaseName, MongoDatabaseSettings settings = null)
        {
            return new MockMongoDatabase();
        }

        public Task<IMongoDatabase> GetDatabaseAsync(string databaseName, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IMongoDatabase>(new MockMongoDatabase());
        }

        // Other IMongoClient methods can be mocked as needed
    }

    public class MockMongoDatabase : IMongoDatabase
    {
        public IMongoCollection<T> GetCollection<T>(string name, MongoCollectionSettings settings = null)
        {
            return new MockMongoCollection<T>();
        }

        // Other IMongoDatabase methods can be mocked as needed
    }

    public class MockMongoCollection<T> : IMongoCollection<T>
    {
        public Task InsertOneAsync(T document, CancellationToken cancellationToken = default)
        {
            // Mock insert logic
            return Task.CompletedTask;
        }

        // Other IMongoCollection methods can be mocked as needed
    }
}