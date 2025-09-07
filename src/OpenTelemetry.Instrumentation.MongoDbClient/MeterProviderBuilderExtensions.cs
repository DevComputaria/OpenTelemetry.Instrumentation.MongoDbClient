using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OpenTelemetry.Metrics;
using OpenTelemetry.Internal;

namespace OpenTelemetry.Instrumentation.MongoDbClient
{
    /// <summary>
    /// Extension methods for setting up MongoDB client instrumentation.
    /// </summary>
    public static class MeterProviderBuilderExtensions
    {
        /// <summary>
        /// Adds MongoDB client metrics instrumentation to the <see cref="MeterProviderBuilder"/>.
        /// </summary>
        /// <param name="builder">The <see cref="MeterProviderBuilder"/> to add instrumentation to.</param>
        /// <param name="configure">Optional configuration options for the instrumentation.</param>
        /// <returns>The <see cref="MeterProviderBuilder"/> for chaining.</returns>
        public static MeterProviderBuilder AddMongoDbClientInstrumentation(
            this MeterProviderBuilder builder,
            Action<MongoDbClientMetricsInstrumentationOptions>? configure = null)
        {
            Guard.ThrowIfNull(builder);

            if (configure != null)
            {
                builder.ConfigureServices(services =>
                    services.Configure(configure));
            }

            // Add MongoDB client metrics
            builder.AddMeter("OpenTelemetry.Instrumentation.MongoDbClient");

            return builder;
        }
    }
}
