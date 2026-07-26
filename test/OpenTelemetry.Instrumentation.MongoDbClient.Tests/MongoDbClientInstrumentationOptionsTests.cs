using System;
using Xunit;
using OpenTelemetry.Instrumentation.MongoDbClient;

namespace OpenTelemetry.Instrumentation.MongoDbClient.Tests
{
    public class MongoDbClientInstrumentationOptionsTests
    {
        [Fact]
        public void DefaultOptions_ShouldBeInitializedCorrectly()
        {
            // Arrange
            var options = new MongoDbClientTraceInstrumentationOptions();

            // Act & Assert
            Assert.False(options.CaptureCommandText);
            Assert.False(options.RecordException);
        }

        [Fact]
        public void SettingOptions_ShouldBeSetCorrectly()
        {
            // Arrange
            var options = new MongoDbClientTraceInstrumentationOptions
            {
                CaptureCommandText = true,
                RecordException = true
            };

            // Act
            var isCaptureEnabled = options.CaptureCommandText;
            var isRecordEnabled = options.RecordException;

            // Assert
            Assert.True(isCaptureEnabled);
            Assert.True(isRecordEnabled);
        }
    }
}