using System;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using MongoDB.Driver.Core.Events;

namespace OpenTelemetry.Instrumentation.MongoDbClient.Implementation
{
    internal sealed class MongoDbClientMetricsInstrumentation : IDisposable
    {
        private static readonly Meter Meter = new("OpenTelemetry.Instrumentation.MongoDbClient");
        private static readonly Histogram<double> DbClientOperationDuration = Meter.CreateHistogram<double>(
            name: "db.client.operation.duration",
            unit: "s",
            description: "Duration of database client operations.");

        private bool disposed;

        public MongoDbClientMetricsInstrumentation()
        {
            MongoDbClientDiagnosticListener.OnCommandSucceededMetrics += OnCommandSucceeded;
            MongoDbClientDiagnosticListener.OnCommandFailedMetrics += OnCommandFailed;
        }

        private static void RecordDuration(double durationSeconds, string commandName, string serverAddress, string? statusCode, string? errorType)
        {
            var tags = new TagList();

            var emitNew = DatabaseSemanticConventionHelper.ShouldEmitNewAttributes();
            var emitOld = DatabaseSemanticConventionHelper.ShouldEmitOldAttributes();

            if (emitNew)
            {
                tags.Add(SemanticConventions.AttributeDbSystemName, "mongodb");
                tags.Add(SemanticConventions.AttributeDbOperationName, commandName);
                tags.Add(SemanticConventions.AttributeServerAddress, serverAddress);

                if (statusCode != null)
                    tags.Add(SemanticConventions.AttributeDbResponseStatusCode, statusCode);
                if (errorType != null)
                    tags.Add(SemanticConventions.AttributeErrorType, errorType);
            }

            if (emitOld)
            {
                tags.Add(SemanticConventions.AttributeDbSystem, "mongodb");
                tags.Add(SemanticConventions.AttributeDbOperation, commandName);
                tags.Add(SemanticConventions.AttributeNetPeerName, serverAddress);
            }

            DbClientOperationDuration.Record(durationSeconds, tags);
        }

        private static void OnCommandSucceeded(CommandSucceededEvent @event, double durationSeconds)
        {
            RecordDuration(
                durationSeconds: durationSeconds,
                commandName: @event.CommandName,
                serverAddress: @event.ConnectionId?.ServerId?.EndPoint?.ToString() ?? "unknown",
                statusCode: "OK",
                errorType: null);
        }

        private static void OnCommandFailed(CommandFailedEvent @event, double durationSeconds)
        {
            RecordDuration(
                durationSeconds: durationSeconds,
                commandName: @event.CommandName,
                serverAddress: @event.ConnectionId?.ServerId?.EndPoint?.ToString() ?? "unknown",
                statusCode: @event.Failure?.GetType()?.Name ?? "ERROR",
                errorType: @event.Failure?.GetType()?.FullName ?? "_OTHER");
        }

        public void Dispose()
        {
            if (disposed) return;
            MongoDbClientDiagnosticListener.OnCommandSucceededMetrics -= OnCommandSucceeded;
            MongoDbClientDiagnosticListener.OnCommandFailedMetrics -= OnCommandFailed;
            disposed = true;
        }
    }
}
