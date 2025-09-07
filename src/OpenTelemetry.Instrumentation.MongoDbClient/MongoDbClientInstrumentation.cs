using System;
using System.Diagnostics;
using System.Threading;
using OpenTelemetry.Instrumentation.MongoDbClient.Implementation;

namespace OpenTelemetry.Instrumentation.MongoDbClient
{
    /// <summary>
    /// MongoDbClient instrumentation.
    /// </summary>
    internal sealed class MongoDbClientInstrumentation : IDisposable
    {
        private static readonly MongoDbClientInstrumentationEventSource Log = MongoDbClientInstrumentationEventSource.Log;
        private static MongoDbClientInstrumentation? instance;
        private static readonly object lockObj = new();
        private readonly MongoDbClientDiagnosticListener diagnosticListener;
        private int refCount;

        /// <summary>
        /// Gets or sets the tracing options for MongoDB client instrumentation.
        /// </summary>
        internal static MongoDbClientTraceInstrumentationOptions TracingOptions { get; set; } = new();

        private MongoDbClientInstrumentation()
        {
            try
            {
                this.diagnosticListener = new MongoDbClientDiagnosticListener(TracingOptions);
                this.diagnosticListener.Subscribe();
                Log.Information("MongoDbClient instrumentation initialized successfully.");
            }
            catch (Exception ex)
            {
                Log.Error($"Error initializing MongoDbClient instrumentation. {ex}");
                throw;
            }
        }

        /// <summary>
        /// Gets or creates the singleton instance of <see cref="MongoDbClientInstrumentation"/>.
        /// </summary>
        /// <returns>The singleton instance.</returns>
        public static MongoDbClientInstrumentation GetInstance()
        {
            if (instance == null)
            {
                lock (lockObj)
                {
                    instance ??= new MongoDbClientInstrumentation();
                }
            }

            return instance;
        }

        /// <summary>
        /// Adds a reference to the instrumentation. This tracks the number of active users.
        /// </summary>
        /// <returns>A handle that decrements the reference count when disposed.</returns>
        public static IDisposable AddTracingHandle()
        {
            var instrumentation = GetInstance();
            Interlocked.Increment(ref instrumentation.refCount);
            return new ReferenceCountedDisposable(instrumentation);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            lock (lockObj)
            {
                if (Interlocked.Decrement(ref this.refCount) <= 0 && instance != null)
                {
                    Log.Information("Disposing MongoDbClient instrumentation.");
                    this.diagnosticListener?.Dispose();
                    instance = null;
                    TracingOptions = new();
                }
            }
        }

        private sealed class ReferenceCountedDisposable : IDisposable
        {
            private readonly MongoDbClientInstrumentation instrumentation;
            private bool isDisposed;

            public ReferenceCountedDisposable(MongoDbClientInstrumentation instrumentation)
            {
                this.instrumentation = instrumentation;
            }

            public void Dispose()
            {
                if (!this.isDisposed)
                {
                    this.instrumentation.Dispose();
                    this.isDisposed = true;
                }
            }
        }
    }
}
