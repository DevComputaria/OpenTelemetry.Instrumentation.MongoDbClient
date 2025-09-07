using System;
using Xunit;

public class MongoDbClientInstrumentationOptionsTests
{
    [Fact]
    public void DefaultOptions_ShouldBeInitializedCorrectly()
    {
        // Arrange
        var options = new MongoDbClientInstrumentationOptions();

        // Act & Assert
        Assert.False(options.EnableSomeFeature);
        Assert.Equal("default-value", options.SomeSetting);
    }

    [Fact]
    public void SettingOptions_ShouldBeSetCorrectly()
    {
        // Arrange
        var options = new MongoDbClientInstrumentationOptions
        {
            EnableSomeFeature = true,
            SomeSetting = "custom-value"
        };

        // Act
        var isFeatureEnabled = options.EnableSomeFeature;
        var settingValue = options.SomeSetting;

        // Assert
        Assert.True(isFeatureEnabled);
        Assert.Equal("custom-value", settingValue);
    }
}