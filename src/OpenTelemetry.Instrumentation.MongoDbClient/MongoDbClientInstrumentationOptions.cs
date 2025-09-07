// Copyright The OpenTelemetry Authors
// SPDX-License-Identifier: Apache-2.0

using System;

namespace OpenTelemetry.Instrumentation.MongoDbClient
{
    /// <summary>
    /// Options for MongoDB client instrumentation.
    /// </summary>
    public class MongoDbClientInstrumentationOptions : MongoDbClientTraceInstrumentationOptions
    {
        // This class inherits all properties from MongoDbClientTraceInstrumentationOptions
        // Additional properties specific to general instrumentation can be added here
    }
}