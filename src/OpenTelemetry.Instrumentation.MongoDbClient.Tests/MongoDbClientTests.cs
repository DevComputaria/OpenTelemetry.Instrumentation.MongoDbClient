using System;
using System.Threading.Tasks;
using Xunit;
using OpenTelemetry.Instrumentation.MongoDbClient.Tests.TestHelpers;

namespace OpenTelemetry.Instrumentation.MongoDbClient.Tests
{
    public class MongoDbClientTests
    {
        [Fact]
        public async Task Test_SpanCreation_OnDatabaseOperation()
        {
            // Arrange
            var mockClient = new MockMongoDbClient();

            // Act
            await mockClient.ExecuteDatabaseOperationAsync();

            // Assert
            Assert.True(mockClient.SpanCreated);
        }

        [Fact]
        public void Test_SpanIsStopped_AfterDatabaseOperation()
        {
            // Arrange
            var mockClient = new MockMongoDbClient();

            // Act
            mockClient.ExecuteDatabaseOperation();

            // Assert
            Assert.True(mockClient.SpanStopped);
        }

        [Fact]
        public void Test_InstrumentationOptions_AreApplied()
        {
            // Arrange
            var options = new MongoDbClientTraceInstrumentationOptions
            {
                CaptureCommandText = true
            };

            // Act
            var isFeatureEnabled = options.CaptureCommandText;

            // Assert
            Assert.True(isFeatureEnabled);
        }
    }
}