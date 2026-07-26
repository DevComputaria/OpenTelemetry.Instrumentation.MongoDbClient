using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using MongoDB.Driver.Core.Configuration;
using MongoDB.Driver.Core.Events;
using OpenTelemetry.Trace;

namespace OpenTelemetry.Instrumentation.MongoDbClient.Implementation;

internal class MongoDbClientDiagnosticListener : IDisposable
{
    private readonly MongoDbClientTraceInstrumentationOptions _options;
    private readonly ConcurrentDictionary<int, Activity> _activeActivities = new();
    private IEventSubscriber? _eventSubscriber;
    private IDisposable? _globalHandle;
    private static readonly ActivitySource ActivitySource = new("OpenTelemetry.Instrumentation.MongoDbClient");
    private const string MongoDbSystemName = "mongodb";

    internal static Action<CommandSucceededEvent, double>? OnCommandSucceededMetrics;
    internal static Action<CommandFailedEvent, double>? OnCommandFailedMetrics;

    public MongoDbClientDiagnosticListener(MongoDbClientTraceInstrumentationOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    public void Subscribe()
    {
        var capturer = new Capturer();
        capturer.CommandStarted += OnCommandStarted;
        capturer.CommandSucceeded += OnCommandSucceeded;
        capturer.CommandFailed += OnCommandFailed;
        _eventSubscriber = capturer;

        _globalHandle = TryRegisterGlobal(capturer);

        MongoDbClientInstrumentationEventSource.Log.MongoDbDiagnosticListenerSubscribed();
    }

    public static void ConfigureMongoClientSettings(object settings)
    {
        try
        {
            var settingsType = settings.GetType();
            var clusterConfigProp = settingsType.GetProperty("ClusterConfigurator");

            var capturer = new Capturer();
            var listener = new MongoDbClientDiagnosticListener(new MongoDbClientTraceInstrumentationOptions());
            capturer.CommandStarted += listener.OnCommandStarted;
            capturer.CommandSucceeded += listener.OnCommandSucceeded;
            capturer.CommandFailed += listener.OnCommandFailed;

            var existing = clusterConfigProp?.GetValue(settings) as Delegate;
            Action<ClusterBuilder> configurator = builder =>
            {
                existing?.DynamicInvoke(builder);
                builder.Subscribe(capturer);
            };

            clusterConfigProp?.SetValue(settings, configurator);
        }
        catch (Exception ex)
        {
            MongoDbClientInstrumentationEventSource.Log.MongoDbDiagnosticListenerSubscribeError(ex);
        }
    }

    private static IDisposable? TryRegisterGlobal(Capturer capturer)
    {
        try
        {
            var settingsType = Type.GetType("MongoDB.Driver.MongoClientSettings, MongoDB.Driver");
            if (settingsType != null)
            {
                var defaultsField = settingsType.GetField("__defaults", BindingFlags.Static | BindingFlags.NonPublic);
                if (defaultsField?.GetValue(null) is object defaults)
                {
                    var clusterConfigProp = settingsType.GetProperty("ClusterConfigurator");
                    if (clusterConfigProp != null)
                    {
                        var existing = clusterConfigProp.GetValue(defaults) as Delegate;
                        clusterConfigProp.SetValue(defaults, (Action<ClusterBuilder>)(builder =>
                        {
                            existing?.DynamicInvoke(builder);
                            builder.Subscribe(capturer);
                        }));

                        MongoDbClientInstrumentationEventSource.Log.Information("Global MongoDB event capturer registered.");
                        return new GlobalSubscriptionHandle();
                    }
                }
            }

            var clusterBuilderType = typeof(ClusterBuilder);
            var subscribersField = clusterBuilderType.GetField("_subscribers", BindingFlags.Static | BindingFlags.NonPublic)
                                ?? clusterBuilderType.GetField("_eventSubscribers", BindingFlags.Static | BindingFlags.NonPublic);

            if (subscribersField?.GetValue(null) is IList subscribers)
            {
                subscribers.Add(capturer);
                MongoDbClientInstrumentationEventSource.Log.Information("Global MongoDB event capturer registered via ClusterBuilder.");
                return new GlobalSubscriptionHandle();
            }

            MongoDbClientInstrumentationEventSource.Log.Information("Global registration unavailable. Instrumentation may require explicit MongoClientSettings configuration.");
        }
        catch (Exception ex)
        {
            MongoDbClientInstrumentationEventSource.Log.MongoDbDiagnosticListenerSubscribeError(ex);
        }

        return null;
    }

    private void OnCommandStarted(CommandStartedEvent @event)
    {
        if (!ActivitySource.HasListeners())
            return;

        var commandName = @event.CommandName;
        var databaseName = @event.DatabaseNamespace?.DatabaseName ?? "unknown";
        var collectionName = GetCollectionName(@event);
        var requestId = @event.RequestId;

        if (_options.Filter != null && !_options.Filter(commandName, databaseName, collectionName))
            return;

        var spanName = $"{commandName} {collectionName}";
        var activity = ActivitySource.StartActivity(spanName, ActivityKind.Client);

        if (activity == null)
            return;

        var emitNew = DatabaseSemanticConventionHelper.ShouldEmitNewAttributes();
        var emitOld = DatabaseSemanticConventionHelper.ShouldEmitOldAttributes();
        var host = @event.ConnectionId?.ServerId?.EndPoint?.ToString();

        if (emitNew)
        {
            activity.SetTag(SemanticConventions.AttributeDbSystemName, MongoDbSystemName);
            activity.SetTag(SemanticConventions.AttributeDbNamespace, databaseName);
            activity.SetTag(SemanticConventions.AttributeDbOperationName, commandName);
            activity.SetTag(SemanticConventions.AttributeDbCollectionName, collectionName);

            if (!string.IsNullOrEmpty(host))
            {
                var hostParts = host!.Split(':');
                activity.SetTag(SemanticConventions.AttributeServerAddress, hostParts[0]);
                if (hostParts.Length > 1 && int.TryParse(hostParts[1], out var port))
                    activity.SetTag(SemanticConventions.AttributeServerPort, port);
            }

            if (@event.Command != null && _options.CaptureCommandText)
            {
                activity.SetTag(SemanticConventions.AttributeDbQueryText, @event.Command.ToString());
                activity.SetTag(SemanticConventions.AttributeDbQuerySummary, $"{commandName} {collectionName}");
            }
        }

        if (emitOld)
        {
            activity.SetTag(SemanticConventions.AttributeDbSystem, MongoDbSystemName);
            activity.SetTag(SemanticConventions.AttributeDbName, databaseName);
            activity.SetTag(SemanticConventions.AttributeDbOperation, commandName);
            activity.SetTag(SemanticConventions.AttributeDbMongoDbCollection, collectionName);
            if (!string.IsNullOrEmpty(host))
                activity.SetTag(SemanticConventions.AttributeNetPeerName, host);
        }

        activity.SetTag("mongodb.request_id", requestId);

        _options.EnrichActivity?.Invoke(activity, commandName, databaseName, collectionName);
        _options.EnrichWithCommand?.Invoke(activity, commandName, @event);

        _activeActivities.TryAdd(requestId, activity);
    }

    private void OnCommandSucceeded(CommandSucceededEvent @event)
    {
        if (_activeActivities.TryRemove(@event.RequestId, out var activity))
        {
            activity.SetStatus(ActivityStatusCode.Ok);
            activity.SetTag("mongodb.request_duration_ms", @event.Duration.TotalMilliseconds);
            _options.EnrichWithCommand?.Invoke(activity, "CommandSucceeded", @event);
            activity.Stop();

            OnCommandSucceededMetrics?.Invoke(@event, @event.Duration.TotalSeconds);
        }
    }

    private void OnCommandFailed(CommandFailedEvent @event)
    {
        if (_activeActivities.TryRemove(@event.RequestId, out var activity))
        {
            activity.SetStatus(ActivityStatusCode.Error, @event.Failure?.Message);
            activity.SetTag("mongodb.request_duration_ms", @event.Duration.TotalMilliseconds);

            var emitNew = DatabaseSemanticConventionHelper.ShouldEmitNewAttributes();
            var emitOld = DatabaseSemanticConventionHelper.ShouldEmitOldAttributes();

            if (emitNew)
            {
                activity.SetTag(SemanticConventions.AttributeErrorType, @event.Failure?.GetType()?.FullName ?? "_OTHER");
            }

            if (_options.RecordException && @event.Failure != null)
            {
                if (emitOld)
                {
                    activity.SetTag(SemanticConventions.AttributeExceptionType, @event.Failure.GetType().FullName);
                    activity.SetTag(SemanticConventions.AttributeExceptionMessage, @event.Failure.Message);
                    if (@event.Failure.StackTrace != null)
                        activity.SetTag(SemanticConventions.AttributeExceptionStacktrace, @event.Failure.StackTrace);
                }

                activity.RecordException(@event.Failure);
            }

            _options.EnrichWithCommand?.Invoke(activity, "CommandFailed", @event);
            activity.Stop();

            OnCommandFailedMetrics?.Invoke(@event, @event.Duration.TotalSeconds);
        }
    }

    private static string GetCollectionName(CommandStartedEvent @event)
    {
        try
        {
            var commandName = @event.CommandName;
            if (@event.Command != null && @event.Command.Contains(commandName))
            {
                var value = @event.Command[commandName];
                if (value != null && !value.IsBsonNull)
                    return value.AsString;
            }

            if (@event.DatabaseNamespace != null)
            {
                var dbName = @event.DatabaseNamespace.DatabaseName;
                if (!string.IsNullOrEmpty(dbName))
                    return commandName;
            }
        }
        catch
        {
        }

        return "unknown";
    }

    public void Dispose()
    {
        foreach (var kvp in _activeActivities)
        {
            kvp.Value.SetStatus(ActivityStatusCode.Error, "Instrumentation disposed before activity completion");
            kvp.Value.Stop();
        }

        _activeActivities.Clear();
        _globalHandle?.Dispose();
        _eventSubscriber = null;
    }

    private sealed class Capturer : IEventSubscriber
    {
        private readonly Dictionary<Type, Delegate> _handlers = new();

        public event Action<CommandStartedEvent>? CommandStarted;
        public event Action<CommandSucceededEvent>? CommandSucceeded;
        public event Action<CommandFailedEvent>? CommandFailed;

        public Capturer()
        {
            _handlers[typeof(CommandStartedEvent)] = new Action<CommandStartedEvent>(e => CommandStarted?.Invoke(e));
            _handlers[typeof(CommandSucceededEvent)] = new Action<CommandSucceededEvent>(e => CommandSucceeded?.Invoke(e));
            _handlers[typeof(CommandFailedEvent)] = new Action<CommandFailedEvent>(e => CommandFailed?.Invoke(e));
        }

        public bool TryGetEventHandler<TEvent>(out Action<TEvent> handler)
        {
            if (_handlers.TryGetValue(typeof(TEvent), out var del))
            {
                handler = (Action<TEvent>)del;
                return true;
            }

            handler = null!;
            return false;
        }
    }

    private sealed class GlobalSubscriptionHandle : IDisposable
    {
        private int _disposed;

        public void Dispose()
        {
            Interlocked.Exchange(ref _disposed, 1);
        }
    }
}
