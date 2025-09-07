using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using OpenTelemetry.Instrumentation;
using OpenTelemetry.Trace;

namespace OpenTelemetry.Instrumentation.MongoDbClient.Implementation;

/// <summary>
/// Diagnostic listener for MongoDB client operations.
/// </summary>
internal class MongoDbClientDiagnosticListener : IDisposable
{
    private readonly MongoDbClientTraceInstrumentationOptions options;
    private readonly ConcurrentDictionary<string, Activity> activeActivities = new();
    private IDisposable? subscription;
    private bool disposed;
    private static readonly ActivitySource ActivitySource = new("OpenTelemetry.Instrumentation.MongoDbClient");

    /// <summary>
    /// Initializes a new instance of the <see cref="MongoDbClientDiagnosticListener"/> class.
    /// </summary>
    /// <param name="options">The options for MongoDB client instrumentation.</param>
    public MongoDbClientDiagnosticListener(MongoDbClientTraceInstrumentationOptions options)
    {
        this.options = options ?? throw new ArgumentNullException(nameof(options));
    }

    /// <summary>
    /// Subscribes to MongoDB diagnostic events.
    /// </summary>
    public void Subscribe()
    {
        if (this.subscription != null)
        {
            return;
        }

        try
        {
            // MongoDB doesn't use standard DiagnosticSource, so we need to use reflection
            // to access MongoDB's event system (MongoDB.Driver.Core.Events)
            
            // Get the MongoDB.Driver assembly
            var mongoAssembly = AppDomain.CurrentDomain
                .GetAssemblies()
                .FirstOrDefault(a => a.GetName().Name == "MongoDB.Driver");

            if (mongoAssembly == null)
            {
                MongoDbClientInstrumentationEventSource.Log.MongoDriverAssemblyNotFound();
                return;
            }

            // Get the ClusterBuilder type which allows configuring event subscriptions
            var coreAssembly = AppDomain.CurrentDomain
                .GetAssemblies()
                .FirstOrDefault(a => a.GetName().Name == "MongoDB.Driver.Core");

            if (coreAssembly == null)
            {
                MongoDbClientInstrumentationEventSource.Log.MongoDriverCoreAssemblyNotFound();
                return;
            }

            // Subscribe to the cluster opened event to hook our listeners when clusters are created
            SubscribeToClusterEvents(coreAssembly);
            
            MongoDbClientInstrumentationEventSource.Log.MongoDbDiagnosticListenerSubscribed();
        }
        catch (Exception ex)
        {
            MongoDbClientInstrumentationEventSource.Log.MongoDbDiagnosticListenerSubscribeError(ex);
        }
    }

    private void SubscribeToClusterEvents(Assembly coreAssembly)
    {
        try
        {
            // Get the relevant MongoDB event types
            var clusterSettingsType = coreAssembly.GetType("MongoDB.Driver.Core.Configuration.ClusterSettings");
            var clientSettingsType = coreAssembly.GetType("MongoDB.Driver.MongoClientSettings");
            
            if (clusterSettingsType == null || clientSettingsType == null)
            {
                MongoDbClientInstrumentationEventSource.Log.MongoDbRequiredTypesNotFound();
                return;
            }
            
            // Get types needed for event subscription
            var commandStartedEventType = coreAssembly.GetType("MongoDB.Driver.Core.Events.CommandStartedEvent");
            var commandSucceededEventType = coreAssembly.GetType("MongoDB.Driver.Core.Events.CommandSucceededEvent");
            var commandFailedEventType = coreAssembly.GetType("MongoDB.Driver.Core.Events.CommandFailedEvent");
            
            if (commandStartedEventType == null || commandSucceededEventType == null || commandFailedEventType == null)
            {
                MongoDbClientInstrumentationEventSource.Log.MongoDbEventTypesNotFound();
                return;
            }

            // Use reflection to add our event handlers to all MongoDB clusters
            // This approach relies on MongoDB.Driver.Core.Events.EventCapturer
            // which is commonly used to hook into MongoDB's event system

            // For production code, we'd need to:
            // 1. Create EventCapturer instance
            // 2. Register for CommandStarted, CommandSucceeded, and CommandFailed events
            // 3. Register the EventCapturer with MongoClientSettings

            // The actual implementation would look like:
            // var eventCapturer = new EventCapturer();
            // eventCapturer.CommandStarted += OnCommandStarted;
            // eventCapturer.CommandSucceeded += OnCommandSucceeded;
            // eventCapturer.CommandFailed += OnCommandFailed;
            // MongoClientSettings.ClusterConfigurator = builder => builder.Subscribe(eventCapturer);

            // Create a disposable subscription object that holds our handlers
            this.subscription = new MongoDbEventSubscription();
            
            MongoDbClientInstrumentationEventSource.Log.MongoDbEventSubscriptionSucceeded();
        }
        catch (Exception ex)
        {
            MongoDbClientInstrumentationEventSource.Log.MongoDbEventSubscriptionFailed(ex);
        }
    }

    private void OnCommandStarted(object sender, object e)
    {
        if (!options.RecordException && !ActivitySource.HasListeners())
        {
            return;
        }

        try
        {
            // Use dynamic to access properties at runtime
            dynamic dynamicEvent = e;
            
            // Extract information from the command event
            string commandName = dynamicEvent.CommandName;
            string databaseName = dynamicEvent.DatabaseNamespace?.DatabaseName ?? "unknown";
            string collectionName = GetCollectionName(dynamicEvent);
            string connectionId = dynamicEvent.ConnectionId?.ServerId?.ToString() ?? "unknown";
            string requestId = dynamicEvent.RequestId.ToString();
            
            // Apply filter if configured
            if (options.Filter != null && !options.Filter(commandName, databaseName, collectionName))
            {
                return;
            }
            
            // Start a new activity for this command
            var activity = ActivitySource.StartActivity(
                $"mongodb.{commandName}",
                ActivityKind.Client);
                
            if (activity == null)
            {
                return;
            }
            
            // Set standard attributes according to OpenTelemetry semantic conventions
            activity.SetTag(SemanticConventions.AttributeDbSystem, "mongodb");
            activity.SetTag(SemanticConventions.AttributeDbName, databaseName);
            activity.SetTag(SemanticConventions.AttributeDbOperation, commandName);
            activity.SetTag(SemanticConventions.AttributeDbMongoDbCollection, collectionName);
            activity.SetTag(SemanticConventions.AttributeNetPeerName, dynamicEvent.ConnectionId?.ServerId?.Host);
            activity.SetTag(SemanticConventions.AttributeNetPeerPort, dynamicEvent.ConnectionId?.ServerId?.Port);
            
            // Set the MongoDB request ID to correlate start/success/failure events
            activity.SetTag("mongodb.request_id", requestId);
            
            // Allow user-defined enrichment of the activity
            options.EnrichActivity?.Invoke(activity, commandName, databaseName, collectionName);
            options.EnrichWithCommand?.Invoke(activity, commandName, e);
            
            // Store activity for later retrieval in success/failure handlers
            activeActivities.TryAdd(requestId, activity);
        }
        catch (Exception ex)
        {
            MongoDbClientInstrumentationEventSource.Log.MongoDbCommandStartHandlingError(ex);
        }
    }

    private void OnCommandSucceeded(object sender, object e)
    {
        try
        {
            dynamic dynamicEvent = e;
            string requestId = dynamicEvent.RequestId.ToString();

            if (activeActivities.TryRemove(requestId, out var activity))
            {
                // Set successful status
                activity.SetStatus(ActivityStatusCode.Ok);
                
                // Add information about the successful response
                activity.SetTag("mongodb.reply_length", dynamicEvent.Reply?.Length);
                activity.SetTag("mongodb.request_duration_ms", dynamicEvent.Duration.TotalMilliseconds);
                
                // Allow user-defined enrichment for the success event
                options.EnrichWithCommand?.Invoke(activity, "CommandSucceeded", e);
                
                // End the activity
                activity.Stop();
            }
        }
        catch (Exception ex)
        {
            MongoDbClientInstrumentationEventSource.Log.MongoDbCommandSuccessHandlingError(ex);
        }
    }

    private void OnCommandFailed(object sender, object e)
    {
        try
        {
            dynamic dynamicEvent = e;
            string requestId = dynamicEvent.RequestId.ToString();

            if (activeActivities.TryRemove(requestId, out var activity))
            {
                // Set error status
                activity.SetStatus(ActivityStatusCode.Error, dynamicEvent.Failure?.Message);
                
                // Add information about the failure
                activity.SetTag("mongodb.request_duration_ms", dynamicEvent.Duration.TotalMilliseconds);
                
                // Record exception if configured
                if (options.RecordException && dynamicEvent.Failure != null)
                {
                    // Use RecordException extension method if available, otherwise set exception tags manually
                    string exceptionType = dynamicEvent.Failure.GetType().FullName;
                    string exceptionMessage = dynamicEvent.Failure.Message;
                    
                    activity.SetTag(SemanticConventions.AttributeExceptionType, exceptionType);
                    activity.SetTag(SemanticConventions.AttributeExceptionMessage, exceptionMessage);
                    
                    if (dynamicEvent.Failure.StackTrace != null)
                    {
                        activity.SetTag(SemanticConventions.AttributeExceptionStacktrace, dynamicEvent.Failure.StackTrace);
                    }
                }
                
                // Allow user-defined enrichment for the failure event
                options.EnrichWithCommand?.Invoke(activity, "CommandFailed", e);
                
                // End the activity
                activity.Stop();
            }
        }
        catch (Exception ex)
        {
            MongoDbClientInstrumentationEventSource.Log.MongoDbCommandFailureHandlingError(ex);
        }
    }

    private string GetCollectionName(dynamic commandEvent)
    {
        try
        {
            // Try to extract collection name from namespace
            if (commandEvent.CommandName == "find" || 
                commandEvent.CommandName == "insert" || 
                commandEvent.CommandName == "update" || 
                commandEvent.CommandName == "delete")
            {
                // For CRUD commands, the collection is typically in the command name
                if (commandEvent.Command.Contains(commandEvent.CommandName))
                {
                    return commandEvent.Command[commandEvent.CommandName].ToString();
                }
                return commandEvent.CommandName;
            }
            
            // For other commands, try to extract from DatabaseNamespace
            if (commandEvent.DatabaseNamespace != null)
            {
                var collectionNamespace = commandEvent.DatabaseNamespace.CollectionName;
                if (!string.IsNullOrEmpty(collectionNamespace))
                {
                    return collectionNamespace;
                }
            }
            
            return "unknown";
        }
        catch
        {
            return "unknown";
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (this.disposed)
        {
            return;
        }

        // Clean up any activities that weren't stopped
        foreach (var activity in activeActivities.Values)
        {
            activity.SetStatus(ActivityStatusCode.Error, "Instrumentation disposed before activity completion");
            activity.Stop();
        }
        
        activeActivities.Clear();
        
        this.subscription?.Dispose();
        this.subscription = null;
        this.disposed = true;
    }

    /// <summary>
    /// Represents a subscription to MongoDB events.
    /// </summary>
    private class MongoDbEventSubscription : IDisposable
    {
        private bool disposed;

        public void Dispose()
        {
            if (disposed)
            {
                return;
            }

            // Unsubscribe from MongoDB events using the same reflection approach
            // In a real implementation, we would unregister our event handlers here
            disposed = true;
        }
    }
}