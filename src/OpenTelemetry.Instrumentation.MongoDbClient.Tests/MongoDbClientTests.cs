using System;
using System.Threading.Tasks;
using Xunit;

namespace OpenTelemetry.Instrumentation.MongoDbClient.Tests
{
    public class MongoDbClientTests
    {
        [Fact]
        public async Task Test_SpanCreation_OnDatabaseOperation()
        {
            // Arrange
            var mockClient = new MockMongoDbClient();
            var instrumentation = new MongoDbClientInstrumentation(mockClient);

            // Act
            await instrumentation.ExecuteDatabaseOperationAsync();

            // Assert
            Assert.True(mockClient.SpanCreated);
        }

        [Fact]
        public void Test_SpanIsStopped_AfterDatabaseOperation()
        {
            // Arrange
            var mockClient = new MockMongoDbClient();
            var instrumentation = new MongoDbClientInstrumentation(mockClient);

            // Act
            instrumentation.ExecuteDatabaseOperation();

            // Assert
            Assert.True(mockClient.SpanStopped);
        }

        [Fact]
        public void Test_InstrumentationOptions_AreApplied()
        {
            // Arrange
            var options = new MongoDbClientInstrumentationOptions
            {
                EnableSomeFeature = true
            };
            var instrumentation = new MongoDbClientInstrumentation(options);

            // Act
            var isFeatureEnabled = instrumentation.IsSomeFeatureEnabled();

            // Assert
            Assert.True(isFeatureEnabled);
        }
    }
}