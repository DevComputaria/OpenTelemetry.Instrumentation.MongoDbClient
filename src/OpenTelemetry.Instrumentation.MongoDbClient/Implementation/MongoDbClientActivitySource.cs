// Copyright The OpenTelemetry Authors
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Diagnostics;
using OpenTelemetry.Trace;

namespace OpenTelemetry.Instrumentation.MongoDbClient.Implementation
{
    /// <summary>
    /// Helper class for MongoDB client instrumentation activities.
    /// </summary>
    internal static class MongoDbClientActivitySource
    {
        private static readonly ActivitySource ActivitySource = new ActivitySource("OpenTelemetry.Instrumentation.MongoDbClient");

        /// <summary>
        /// Starts a new activity for MongoDB client operations.
        /// </summary>
        /// <param name="commandName">The MongoDB command name.</param>
        /// <param name="databaseName">The database name.</param>
        /// <param name="collectionName">The collection name.</param>
        /// <returns>A new <see cref="Activity"/> or null if instrumentation is disabled.</returns>
        public static Activity? StartMongoActivity(string commandName, string databaseName, string collectionName)
        {
            var activityName = $"mongodb.{commandName.ToLowerInvariant()}";
            
            var activity = ActivitySource.StartActivity(
                activityName,
                ActivityKind.Client);
            
            if (activity == null || !activity.IsAllDataRequested)
            {
                return activity;
            }

            // Add standard attributes following OpenTelemetry semantic conventions
            activity.SetTag(SemanticConventions.AttributeDbSystem, "mongodb");
            activity.SetTag(SemanticConventions.AttributeDbName, databaseName);
            
            if (!string.IsNullOrEmpty(collectionName))
            {
                activity.SetTag(SemanticConventions.AttributeDbMongoDbCollection, collectionName);
            }
            
            activity.SetTag(SemanticConventions.AttributeDbOperation, commandName);
            
            return activity;
        }
    }
}