using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OpenTelemetry.Instrumentation.MongoDbClient.Implementation;
using OpenTelemetry.Internal;
using OpenTelemetry.Metrics;

namespace OpenTelemetry.Instrumentation.MongoDbClient
{
    public static class MeterProviderBuilderExtensions
    {
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

            builder.AddInstrumentation(sp =>
            {
                var options = sp.GetRequiredService<IOptionsMonitor<MongoDbClientMetricsInstrumentationOptions>>().CurrentValue;
                return new MongoDbClientMetricsInstrumentation();
            });

            builder.AddMeter("OpenTelemetry.Instrumentation.MongoDbClient");

            return builder;
        }
    }
}
