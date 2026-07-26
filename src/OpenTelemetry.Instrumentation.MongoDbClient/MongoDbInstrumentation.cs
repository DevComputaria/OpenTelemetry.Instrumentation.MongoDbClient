using System;
using OpenTelemetry.Instrumentation.MongoDbClient.Implementation;

namespace OpenTelemetry.Instrumentation.MongoDbClient
{
    public static class MongoDbInstrumentation
    {
        public static void ConfigureClientSettings(object mongoClientSettings)
        {
            if (mongoClientSettings == null)
                throw new ArgumentNullException(nameof(mongoClientSettings));

            MongoDbClientDiagnosticListener.ConfigureMongoClientSettings(mongoClientSettings);
        }
    }
}
