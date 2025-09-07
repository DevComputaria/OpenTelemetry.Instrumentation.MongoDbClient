// TracerProviderBuilderExtensions.cs
using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OpenTelemetry.Instrumentation.MongoDbClient;
using OpenTelemetry.Instrumentation.MongoDbClient.Implementation;
using OpenTelemetry.Internal;
using OpenTelemetry.Trace;
using MongoDbClientInstrumentation = OpenTelemetry.Instrumentation.MongoDbClient.MongoDbClientInstrumentation;

namespace OpenTelemetry.Trace
{
    /// <summary>
    /// Extension methods to simplify registering MongoDB client instrumentation.
    /// </summary>
    public static class TracerProviderBuilderExtensions
    {
        /// <summary>
        /// Adds MongoDB client instrumentation to the <see cref="TracerProviderBuilder"/>.
        /// </summary>
        /// <param name="builder">The <see cref="TracerProviderBuilder"/> to add instrumentation to.</param>
        /// <param name="configure">Optional configuration options for the MongoDB client instrumentation.</param>
        /// <returns>The <see cref="TracerProviderBuilder"/> for chaining.</returns>
        public static TracerProviderBuilder AddMongoDbClientInstrumentation(
            this TracerProviderBuilder builder,
            Action<MongoDbClientTraceInstrumentationOptions>? configure = null)
        {
            Guard.ThrowIfNull(builder);

            if (configure != null)
            {
                builder.ConfigureServices(services =>
                    services.Configure(configure));
            }

            builder.AddInstrumentation(sp =>
            {
                var options = sp.GetRequiredService<IOptionsMonitor<MongoDbClientTraceInstrumentationOptions>>().CurrentValue;
                MongoDbClientInstrumentation.TracingOptions = options;
                return MongoDbClientInstrumentation.AddTracingHandle();
            });

            builder.AddSource("OpenTelemetry.Instrumentation.MongoDbClient");

            return builder;
        }
    }
}