using System;

namespace OpenTelemetry.Internal
{
    /// <summary>
    /// Guard class for validating input parameters.
    /// </summary>
    internal static class Guard
    {
        /// <summary>
        /// Throw ArgumentNullException if the value is null.
        /// </summary>
        /// <param name="value">The value to check.</param>
        /// <param name="paramName">The parameter name.</param>
        /// <exception cref="ArgumentNullException">Thrown when value is null.</exception>
        public static void ThrowIfNull(object? value, string? paramName = null)
        {
            if (value == null)
            {
                throw new ArgumentNullException(paramName);
            }
        }
    }
}
