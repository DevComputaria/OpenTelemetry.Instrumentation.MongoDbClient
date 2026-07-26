using System;
using System.Threading;
using OpenTelemetry.Instrumentation.MongoDbClient.Implementation;

namespace OpenTelemetry.Instrumentation.MongoDbClient
{
    internal sealed class MongoDbClientInstrumentation : IDisposable
    {
        private static readonly MongoDbClientInstrumentationEventSource Log = MongoDbClientInstrumentationEventSource.Log;
        private static MongoDbClientInstrumentation? instance;
        private static readonly object LockObj = new();
        private readonly MongoDbClientDiagnosticListener diagnosticListener;
        private int refCount;

        internal static MongoDbClientTraceInstrumentationOptions TracingOptions { get; set; } = new();

        private MongoDbClientInstrumentation()
        {
            diagnosticListener = new MongoDbClientDiagnosticListener(TracingOptions);
            diagnosticListener.Subscribe();
            Log.Information("MongoDbClient instrumentation initialized successfully.");
        }

        public static MongoDbClientInstrumentation GetInstance()
        {
            if (instance == null)
            {
                lock (LockObj)
                {
                    instance ??= new MongoDbClientInstrumentation();
                }
            }
            return instance;
        }

        public static IDisposable AddTracingHandle()
        {
            var instrumentation = GetInstance();
            Interlocked.Increment(ref instrumentation.refCount);
            return new ReferenceCountedDisposable(instrumentation);
        }

        public void Dispose()
        {
            lock (LockObj)
            {
                if (Interlocked.Decrement(ref refCount) <= 0 && instance != null)
                {
                    Log.Information("Disposing MongoDbClient instrumentation.");
                    diagnosticListener?.Dispose();
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
                if (!isDisposed)
                {
                    instrumentation.Dispose();
                    isDisposed = true;
                }
            }
        }
    }
}
