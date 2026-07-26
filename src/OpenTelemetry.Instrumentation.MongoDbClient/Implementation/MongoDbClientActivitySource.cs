using System;
using System.Diagnostics;
using OpenTelemetry.Trace;

namespace OpenTelemetry.Instrumentation.MongoDbClient.Implementation
{
    internal static class MongoDbClientActivitySource
    {
        private static readonly ActivitySource ActivitySource = new("OpenTelemetry.Instrumentation.MongoDbClient");

        public static Activity? StartMongoActivity(string commandName, string databaseName, string collectionName)
        {
            var spanName = $"{commandName} {collectionName}";
            var activity = ActivitySource.StartActivity(spanName, ActivityKind.Client);

            if (activity == null || !activity.IsAllDataRequested)
                return activity;

            var emitNew = DatabaseSemanticConventionHelper.ShouldEmitNewAttributes();
            var emitOld = DatabaseSemanticConventionHelper.ShouldEmitOldAttributes();

            if (emitNew)
            {
                activity.SetTag(SemanticConventions.AttributeDbSystemName, "mongodb");
                activity.SetTag(SemanticConventions.AttributeDbNamespace, databaseName);
                activity.SetTag(SemanticConventions.AttributeDbCollectionName, collectionName);
                activity.SetTag(SemanticConventions.AttributeDbOperationName, commandName);
            }

            if (emitOld)
            {
                activity.SetTag(SemanticConventions.AttributeDbSystem, "mongodb");
                activity.SetTag(SemanticConventions.AttributeDbName, databaseName);
                activity.SetTag(SemanticConventions.AttributeDbMongoDbCollection, collectionName);
                activity.SetTag(SemanticConventions.AttributeDbOperation, commandName);
            }

            return activity;
        }
    }
}
