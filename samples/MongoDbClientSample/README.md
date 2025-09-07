# MongoDB Client Sample with OpenTelemetry Instrumentation

This sample application demonstrates how to use the OpenTelemetry.Instrumentation.MongoDbClient library to automatically instrument MongoDB client operations with OpenTelemetry.

## Prerequisites

- .NET 8 SDK or later
- MongoDB server (local or remote)

## Features Demonstrated

This sample demonstrates:

1. Configuring OpenTelemetry tracing for MongoDB client operations
2. Configuring OpenTelemetry metrics for MongoDB client operations
3. Using filters to control which operations are traced
4. Enriching spans with custom attributes
5. Recording exceptions
6. Basic MongoDB operations (insert, find, update, delete)

## Running the Sample

1. Ensure MongoDB is running and accessible at the connection string specified in the code (default: `mongodb://localhost:27017`).
   - If you need to use a different connection string, modify it in `Program.cs`.

2. Build and run the sample:

```bash
dotnet build
dotnet run
```

3. Observe the console output showing both:
   - Application logs from MongoDB operations
   - OpenTelemetry console exporter output showing traces and metrics

## What to Look For

- The sample performs several MongoDB operations (insert, find, update, delete)
- Each operation generates OpenTelemetry telemetry data
- Custom attributes are added to spans based on operation type
- Console output shows both the application progress and the OpenTelemetry data

## Troubleshooting

If you see MongoDB connection errors:
- Ensure MongoDB is running
- Check the connection string in the code
- You may need to modify security settings if using a remote MongoDB instance

## Sample Output

When running the sample, you should see output similar to this:

```
=== MongoDB Client Sample with OpenTelemetry Instrumentation ===
Connecting to MongoDB at: mongodb://localhost:27017
Using database: sample_db
Using collection: sample_collection
Dropped existing collection (if any)

Inserting sample documents...
Inserted 3 documents

Querying for documents...
Found 3 documents:
- Document 1: value = 42
- Document 2: value = 73
- Document 3: value = 99

Querying for documents with value > 50...
Found 2 documents with value > 50:
- Document 2: value = 73
- Document 3: value = 99

Updating Document 1...
Update completed

Deleting Document 3...
Delete completed

Final collection state:
- Document 1: value = 100
- Document 2: value = 73

Press any key to exit...
```

Additionally, you will see OpenTelemetry spans and metrics in the console output from the console exporters.