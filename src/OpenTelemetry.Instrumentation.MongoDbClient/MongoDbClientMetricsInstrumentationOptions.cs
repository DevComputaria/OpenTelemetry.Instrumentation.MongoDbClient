// Copyright The OpenTelemetry Authors
// SPDX-License-Identifier: Apache-2.0

using System;

namespace OpenTelemetry.Instrumentation.MongoDbClient;

/// <summary>
/// Options for MongoDB metrics instrumentation.
/// </summary>
public class MongoDbClientMetricsInstrumentationOptions
{
    /// <summary>
    /// Gets or sets a filter function that determines which command metrics should be collected.
    /// </summary>
    /// <remarks>
    /// <para>string: the command name.</para>
    /// <para>string: the database name.</para>
    /// <para>string: the collection name.</para>
    /// </remarks>
    public Func<string, string, string, bool>? Filter { get; set; }
}
