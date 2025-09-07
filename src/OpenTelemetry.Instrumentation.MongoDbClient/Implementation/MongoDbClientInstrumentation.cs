using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using MongoDB.Driver.Core.Events;
using MongoDB.Driver.Core.Clusters;
using OpenTelemetry.Trace;

namespace OpenTelemetry.Instrumentation.MongoDbClient.Implementation
{
    /// <summary>
    /// MongoDB instrumentation implementation.
    /// </summary>
    internal sealed class MongoDbClientInstrumentation : IDisposable
    {
        private static readonly object Lock = new object();
        private static MongoDbClientInstrumentation? instance;
        private static int refCount;

        internal static MongoDbClientTraceInstrumentationOptions TracingOptions { get; set; } = new MongoDbClientTraceInstrumentationOptions();
        
        private readonly MongoDbClientDiagnosticListener diagnosticListener;

        private MongoDbClientInstrumentation()
        {
            diagnosticListener = new MongoDbClientDiagnosticListener(TracingOptions);
            
            try
            {
                // Start the diagnostic listener to monitor MongoDB events
                diagnosticListener.Subscribe();
                
                MongoDbClientInstrumentationEventSource.Log.MongoInstrumentationInitialized();
            }
            catch (Exception ex)
            {
                MongoDbClientInstrumentationEventSource.Log.MongoInstrumentationException(ex.ToString());
            }
        }

        /// <summary>
        /// Adds a handle to the instrumentation.
        /// </summary>
        /// <returns>An IDisposable that when disposed decrements the reference count.</returns>
        public static IDisposable AddTracingHandle()
        {
            lock (Lock)
            {
                if (instance == null)
                {
                    instance = new MongoDbClientInstrumentation();
                }

                Interlocked.Increment(ref refCount);
                return new DisposableHandle();
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            try
            {
                // Dispose the diagnostic listener
                diagnosticListener.Dispose();
                
                MongoDbClientInstrumentationEventSource.Log.MongoInstrumentationDisposed();
            }
            catch (Exception ex)
            {
                MongoDbClientInstrumentationEventSource.Log.MongoInstrumentationException($"Error disposing MongoDB instrumentation: {ex}");
            }
        }

        private sealed class DisposableHandle : IDisposable
        {
            private bool disposed;

            public void Dispose()
            {
                if (disposed)
                {
                    return;
                }

                disposed = true;

                lock (Lock)
                {
                    int count = Interlocked.Decrement(ref refCount);
                    if (count <= 0 && instance != null)
                    {
                        var inst = instance;
                        instance = null;
                        inst.Dispose();
                    }
                }
            }
        }
    }
}