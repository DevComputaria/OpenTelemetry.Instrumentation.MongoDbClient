using System;

namespace OpenTelemetry.Instrumentation.MongoDbClient.Implementation
{
    internal enum SemanticConventionOptIn
    {
        Old,
        New,
        Duplicate,
    }

    internal static class DatabaseSemanticConventionHelper
    {
        private const string EnvVarName = "OTEL_SEMCONV_STABILITY_OPT_IN";

        private static SemanticConventionOptIn? _cachedValue;

        public static SemanticConventionOptIn GetOptIn()
        {
            if (_cachedValue.HasValue)
                return _cachedValue.Value;

            var raw = Environment.GetEnvironmentVariable(EnvVarName);
            var result = raw switch
            {
                "database" => SemanticConventionOptIn.New,
                "database/dup" => SemanticConventionOptIn.Duplicate,
                _ => SemanticConventionOptIn.Old,
            };

            _cachedValue = result;
            return result;
        }

        public static bool ShouldEmitNewAttributes()
        {
            var optIn = GetOptIn();
            return optIn == SemanticConventionOptIn.New || optIn == SemanticConventionOptIn.Duplicate;
        }

        public static bool ShouldEmitOldAttributes()
        {
            var optIn = GetOptIn();
            return optIn == SemanticConventionOptIn.Old || optIn == SemanticConventionOptIn.Duplicate;
        }

        internal static void ResetCache()
        {
            _cachedValue = null;
        }
    }
}
