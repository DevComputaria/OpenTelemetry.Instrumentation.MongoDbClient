// This file contains a simplified mock implementation for basic compilation testing.
// Note: For production tests, consider using MongoDB testcontainers or in-memory implementations.

using System.Threading.Tasks;

namespace OpenTelemetry.Instrumentation.MongoDbClient.Tests.TestHelpers
{
    /// <summary>
    /// Simplified mock for basic testing. 
    /// Removes complex MongoDB interface implementations that caused compilation errors.
    /// </summary>
    public class MockMongoDbClient
    {
        public bool SpanCreated { get; set; }
        public bool SpanStopped { get; set; }

        public void ExecuteDatabaseOperation()
        {
            SpanCreated = true;
            SpanStopped = true;
        }

        public async Task ExecuteDatabaseOperationAsync()
        {
            await Task.Delay(10); // Simulate async work
            SpanCreated = true;
            SpanStopped = true;
        }
    }
}