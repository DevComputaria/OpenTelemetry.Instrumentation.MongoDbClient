using System;
using System.Diagnostics.Tracing;

namespace OpenTelemetry.Instrumentation.MongoDbClient.Implementation;

/// <summary>
/// EventSource events for MongoDB Client instrumentation.
/// </summary>
[EventSource(Name = "OpenTelemetry-Instrumentation-MongoDbClient")]
internal sealed class MongoDbClientInstrumentationEventSource : EventSource
{
    public static readonly MongoDbClientInstrumentationEventSource Log = new();

    [NonEvent]
    public void MongoDriverAssemblyNotFound()
    {
        if (IsEnabled(EventLevel.Warning, EventKeywords.All))
        {
            MongoDriverAssemblyNotFound_("MongoDB.Driver assembly was not found. MongoDB instrumentation will be disabled.");
        }
    }

    [Event(1, Message = "{0}", Level = EventLevel.Warning)]
    private void MongoDriverAssemblyNotFound_(string message) => WriteEvent(1, message);

    [NonEvent]
    public void MongoDriverCoreAssemblyNotFound()
    {
        if (IsEnabled(EventLevel.Warning, EventKeywords.All))
        {
            MongoDriverCoreAssemblyNotFound_("MongoDB.Driver.Core assembly was not found. MongoDB instrumentation will be disabled.");
        }
    }

    [Event(2, Message = "{0}", Level = EventLevel.Warning)]
    private void MongoDriverCoreAssemblyNotFound_(string message) => WriteEvent(2, message);

    [NonEvent]
    public void MongoDbRequiredTypesNotFound()
    {
        if (IsEnabled(EventLevel.Warning, EventKeywords.All))
        {
            MongoDbRequiredTypesNotFound_("Required MongoDB types were not found. MongoDB instrumentation will be disabled.");
        }
    }

    [Event(3, Message = "{0}", Level = EventLevel.Warning)]
    private void MongoDbRequiredTypesNotFound_(string message) => WriteEvent(3, message);

    [NonEvent]
    public void MongoDbEventTypesNotFound()
    {
        if (IsEnabled(EventLevel.Warning, EventKeywords.All))
        {
            MongoDbEventTypesNotFound_("MongoDB event types were not found. MongoDB instrumentation will be disabled.");
        }
    }

    [Event(4, Message = "{0}", Level = EventLevel.Warning)]
    private void MongoDbEventTypesNotFound_(string message) => WriteEvent(4, message);

    [NonEvent]
    public void MongoDbDiagnosticListenerSubscribed()
    {
        if (IsEnabled(EventLevel.Informational, EventKeywords.All))
        {
            MongoDbDiagnosticListenerSubscribed_("MongoDB diagnostic listener successfully subscribed.");
        }
    }

    [Event(5, Message = "{0}", Level = EventLevel.Informational)]
    private void MongoDbDiagnosticListenerSubscribed_(string message) => WriteEvent(5, message);

    [NonEvent]
    public void MongoDbDiagnosticListenerSubscribeError(Exception ex)
    {
        if (IsEnabled(EventLevel.Error, EventKeywords.All))
        {
            MongoDbDiagnosticListenerSubscribeError_($"Error subscribing MongoDB diagnostic listener: {ex}");
        }
    }

    [Event(6, Message = "{0}", Level = EventLevel.Error)]
    private void MongoDbDiagnosticListenerSubscribeError_(string message) => WriteEvent(6, message);

    [NonEvent]
    public void MongoDbEventSubscriptionSucceeded()
    {
        if (IsEnabled(EventLevel.Informational, EventKeywords.All))
        {
            MongoDbEventSubscriptionSucceeded_("Successfully subscribed to MongoDB events.");
        }
    }

    [Event(7, Message = "{0}", Level = EventLevel.Informational)]
    private void MongoDbEventSubscriptionSucceeded_(string message) => WriteEvent(7, message);

    [NonEvent]
    public void MongoDbEventSubscriptionFailed(Exception ex)
    {
        if (IsEnabled(EventLevel.Error, EventKeywords.All))
        {
            MongoDbEventSubscriptionFailed_($"Failed to subscribe to MongoDB events: {ex}");
        }
    }

    [Event(8, Message = "{0}", Level = EventLevel.Error)]
    private void MongoDbEventSubscriptionFailed_(string message) => WriteEvent(8, message);

    [NonEvent]
    public void MongoDbCommandStartHandlingError(Exception ex)
    {
        if (IsEnabled(EventLevel.Error, EventKeywords.All))
        {
            MongoDbCommandStartHandlingError_($"Error handling MongoDB command start event: {ex}");
        }
    }

    [Event(9, Message = "{0}", Level = EventLevel.Error)]
    private void MongoDbCommandStartHandlingError_(string message) => WriteEvent(9, message);

    [NonEvent]
    public void MongoDbCommandSuccessHandlingError(Exception ex)
    {
        if (IsEnabled(EventLevel.Error, EventKeywords.All))
        {
            MongoDbCommandSuccessHandlingError_($"Error handling MongoDB command success event: {ex}");
        }
    }

    [Event(10, Message = "{0}", Level = EventLevel.Error)]
    private void MongoDbCommandSuccessHandlingError_(string message) => WriteEvent(10, message);

    [NonEvent]
    public void MongoDbCommandFailureHandlingError(Exception ex)
    {
        if (IsEnabled(EventLevel.Error, EventKeywords.All))
        {
            MongoDbCommandFailureHandlingError_($"Error handling MongoDB command failure event: {ex}");
        }
    }

    [Event(11, Message = "{0}", Level = EventLevel.Error)]
    private void MongoDbCommandFailureHandlingError_(string message) => WriteEvent(11, message);

    [NonEvent]
    public void Information(string message)
    {
        if (IsEnabled(EventLevel.Informational, EventKeywords.All))
        {
            Information_(message);
        }
    }

    [Event(12, Message = "{0}", Level = EventLevel.Informational)]
    private void Information_(string message) => WriteEvent(12, message);

    [NonEvent]
    public void Error(string message)
    {
        if (IsEnabled(EventLevel.Error, EventKeywords.All))
        {
            Error_(message);
        }
    }

    [Event(13, Message = "{0}", Level = EventLevel.Error)]
    private void Error_(string message) => WriteEvent(13, message);

    [NonEvent]
    public void MongoInstrumentationInitialized()
    {
        if (IsEnabled(EventLevel.Informational, EventKeywords.All))
        {
            MongoInstrumentationInitialized_("MongoDB instrumentation initialized successfully.");
        }
    }

    [Event(14, Message = "{0}", Level = EventLevel.Informational)]
    private void MongoInstrumentationInitialized_(string message) => WriteEvent(14, message);

    [NonEvent]
    public void MongoInstrumentationException(string errorMessage)
    {
        if (IsEnabled(EventLevel.Error, EventKeywords.All))
        {
            MongoInstrumentationException_($"MongoDB instrumentation error: {errorMessage}");
        }
    }

    [Event(15, Message = "{0}", Level = EventLevel.Error)]
    private void MongoInstrumentationException_(string message) => WriteEvent(15, message);

    [NonEvent]
    public void MongoCommandIntercepted(string commandName, string databaseName, string collectionName)
    {
        if (IsEnabled(EventLevel.Verbose, EventKeywords.All))
        {
            MongoCommandIntercepted_($"MongoDB command intercepted: {commandName}, Database: {databaseName}, Collection: {collectionName}");
        }
    }

    [Event(16, Message = "{0}", Level = EventLevel.Verbose)]
    private void MongoCommandIntercepted_(string message) => WriteEvent(16, message);

    [NonEvent]
    public void MongoInstrumentationDisposed()
    {
        if (IsEnabled(EventLevel.Informational, EventKeywords.All))
        {
            MongoInstrumentationDisposed_("MongoDB instrumentation disposed.");
        }
    }

    [Event(17, Message = "{0}", Level = EventLevel.Informational)]
    private void MongoInstrumentationDisposed_(string message) => WriteEvent(17, message);
}
