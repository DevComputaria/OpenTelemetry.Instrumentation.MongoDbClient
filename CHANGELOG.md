# Changelog

All notable changes to the OpenTelemetry.Instrumentation.MongoDbClient project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased] - 2026-07-26

### Added

- **Project Restructuring:**
  - Reorganized directory structure to follow `src/`, `test/`, `examples/`, `build/` layout
  - Added `Directory.Build.props` (root, src, test, examples) with inheritance chain
  - Added `Directory.Packages.props` for centralized package version management
  - Added `build/Common.props`, `build/Common.nonprod.props` with rulesets and StyleCop
  - Moved test projects from `src/` to `test/` and sample from `samples/` to `examples/`
  - Updated `.sln` file with new project paths

- **CI/CD Pipeline:**
  - Added GitHub Actions `ci.yml` with `dorny/paths-filter` for change detection
  - Added reusable `Component.BuildTest.yml` workflow (build, test, code coverage)
  - Added `dotnet-format.yml` for linting
  - Added `ossf-scorecard.yml` with OSSF Scorecard + SARIF summary script
  - Added `package-validation.yml` for NuGet pack and validation
  - Updated `dependabot.yml` with nuget and github-actions ecosystem

- **Docker Compose (Observability Stack):**
  - Added `docker-compose.yml` with MongoDB, Elasticsearch, Kibana, APM Server, OTel Collector
  - Added `otel-collector-config.yml` for OTLP gRPC to HTTP bridging
  - Added `apm-server.yml` with preconfigured credentials
  - Added documentation at `docs/APM-DATA-COMPILATION.md`

- **Build Infrastructure:**
  - Added `build/OpenTelemetry.proj` for NuGet packaging
  - Added `build/format-scorecard-summary.py` for SARIF summary extraction
  - Updated `Makefile` with infra and run commands

### Fixed

- **Build System Issues:**
  - Fixed duplicate assembly attributes error by disabling auto-generated assembly info (`GenerateAssemblyInfo=false`)
  - Resolved MSBuild conflicts between manual `AssemblyInfo.cs` and SDK-style project generation
  - Successfully building for all target frameworks: .NET 8.0, .NET 6.0, and .NET Standard 2.0

- **Code Quality:**
  - Removed unnecessary using directives (IDE0005) across 9 files

- **Test Infrastructure:**
  - Removed incompatible `GitHubActions` logger (incompatible with Microsoft.Testing.Platform in .NET SDK 10)
  - Simplified mock implementations to avoid complex MongoDB interface implementation requirements
  - Updated test classes to use correct instrumentation options (`MongoDbClientTraceInstrumentationOptions`)
  - Fixed namespace issues in test files
  - Removed incomplete `IMongoClient`, `IMongoDatabase`, and `IMongoCollection<T>` implementations from mocks

- **Project Structure:**
  - Corrected test project dependencies and references
  - Fixed syntax errors in test files (removed duplicate closing braces)

### Known Issues

- **OpenTelemetry Compliance:** 
  - Span names do not follow current MongoDB semantic conventions (should be `"{operation} {collection}"` format)
  - Using deprecated semantic convention attributes that need updating to stable v1.37.0 attributes
  - Missing implementation of real MongoDB event hooks (currently using reflection stubs)
  - Missing support for `OTEL_SEMCONV_STABILITY_OPT_IN` environment variable

- **Dependencies:**
  - Sample project has warning about OpenTelemetry.Instrumentation.Http 1.7.0 vulnerability (GHSA-vh2m-22xx-q94f)

### Technical Debt

- Unit tests need MongoDB TestContainers or in-memory MongoDB implementation for realistic testing
- Mock classes should be replaced with proper test doubles or integration tests
- MongoDB event subscription implementation needs to be completed with actual EventSubscriber

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