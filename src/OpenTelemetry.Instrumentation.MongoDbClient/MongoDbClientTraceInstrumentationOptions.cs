// Copyright The OpenTelemetry Authors
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Diagnostics;
using MongoDB.Driver.Core.Events;

namespace OpenTelemetry.Instrumentation.MongoDbClient;

/// <summary>
/// Options for MongoDB instrumentation.
/// </summary>
public class MongoDbClientTraceInstrumentationOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether the command text is captured as an activity tag.
    /// Default value: <see langword="false"/>.
    /// </summary>
    public bool CaptureCommandText { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether exceptions should be recorded as ActivityEvent.
    /// Default value: <see langword="false"/>.
    /// </summary>
    public bool RecordException { get; set; }

    /// <summary>
    /// Gets or sets an action to enrich an Activity.
    /// </summary>
    /// <remarks>
    /// <para><see cref="Activity"/>: the activity being enriched.</para>
    /// <para>string: the command name.</para>
    /// <para>object: the command event (can be CommandStartedEvent, CommandSucceededEvent, or CommandFailedEvent).</para>
    /// </remarks>
    public Action<Activity, string, object>? EnrichWithCommand { get; set; }

    /// <summary>
    /// Gets or sets an action to enrich an Activity with database and collection information.
    /// </summary>
    /// <remarks>
    /// <para><see cref="Activity"/>: the activity being enriched.</para>
    /// <para>string: the command name.</para>
    /// <para>string: the database name.</para>
    /// <para>string: the collection name.</para>
    /// </remarks>
    public Action<Activity, string, string, string>? EnrichActivity { get; set; }

    /// <summary>
    /// Gets or sets a filter function that determines which command events should be collected.
    /// </summary>
    /// <remarks>
    /// <para>string: the command name.</para>
    /// <para>string: the database name.</para>
    /// <para>string: the collection name.</para>
    /// </remarks>
    public Func<string, string, string, bool>? Filter { get; set; }
}
