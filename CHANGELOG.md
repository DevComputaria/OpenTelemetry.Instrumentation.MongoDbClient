# Changelog

All notable changes to the OpenTelemetry.Instrumentation.MongoDbClient project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.1] - 2025-04-24

### Fixed

- Updated package dependencies to be compatible with OpenTelemetry 1.7.0
  - Microsoft.Extensions.DependencyInjection.Abstractions from 3.1.0 to 8.0.0
  - Microsoft.Extensions.Options from 3.1.0 to 8.0.0
  - System.Diagnostics.DiagnosticSource from 7.0.2 to 8.0.0
- Resolved class conflicts by removing duplicate MongoDbClientInstrumentation.cs file
- Improved type consistency using the same option types throughout the codebase
- Enhanced MongoDB event handling with proper parameter typing and dynamic casting
- Modernized activity status code usage by replacing deprecated Status references
- Simplified instrumentation architecture to better handle MongoDB driver events
- Fixed semantic conventions references in MongoDB diagnostic listener

## [1.0.0] - 2025-04-24

Initial release of the OpenTelemetry.Instrumentation.MongoDbClient package.

### Added

- Core instrumentation for MongoDB client operations using OpenTelemetry
- Extension methods for TracerProviderBuilder to add MongoDB client tracing
  - `AddMongoDbClientInstrumentation()` method with options configuration
- Extension methods for MeterProviderBuilder to add MongoDB client metrics
  - `AddMongoDbClientInstrumentation()` method with options configuration
- Activity creation for MongoDB commands with proper semantic conventions
  - MongoDB operation activities with `mongodb.{command}` naming pattern
  - Standard database attributes according to OpenTelemetry semantic conventions
- MongoDbClientTraceInstrumentationOptions for configuring tracing behavior
  - Filter option to control which operations are traced
  - Enrich option to add custom attributes to spans
  - RecordException option to control exception recording
- MongoDbClientMetricsInstrumentationOptions for configuring metrics collection
- Diagnostic listener implementation for MongoDB events
  - Support for command started, succeeded, and failed events
  - Proper correlation between events using MongoDB request IDs
- Activity source setup with proper naming conventions
- EventSource for logging instrumentation-related events
- Reference counting mechanism for proper resource management
- Singleton instrumentation pattern with thread-safe implementation
- Support for MongoDB.Driver via reflection-based event handling
- Basic MongoDB collection name and database name extraction

### Documentation

- Added README with installation and usage instructions
- Added code samples for basic and advanced configuration
- Added architecture documentation outlining design principles
- Added best practices for OpenTelemetry instrumentation

## [0.9.0] - 2025-03-15

### Added

- Initial project structure and dependency setup
- Basic implementations of MongoDB client activity tracking
- Prototype of extension methods for OpenTelemetry integration

## [0.8.0] - 2025-02-20

### Added

- Project planning and architecture design
- Research on MongoDB driver instrumentation approaches
- Initial exploration of MongoDB event system