using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OpenTelemetry.Instrumentation.MongoDbClient;
using OpenTelemetry.Internal;
using OpenTelemetry.Trace;

namespace OpenTelemetry.Trace
{
    public static class TracerProviderBuilderExtensions
    {
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
