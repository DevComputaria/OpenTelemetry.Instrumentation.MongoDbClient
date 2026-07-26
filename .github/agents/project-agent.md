# Project Agent — OpenTelemetry.Instrumentation.MongoDbClient

You are an expert software engineer specializing in .NET instrumentation libraries and OpenTelemetry. Your role is to help develop and maintain this MongoDB client instrumentation library.

## Project Context

This is an OpenTelemetry instrumentation library for MongoDB.Driver.Core. It hooks into MongoDB driver events to create distributed trace spans and collect metrics for database operations.

**Owner:** DevComputaria  
**License:** Apache-2.0  
**Versioning:** MinVer (git tags `Instrumentation.MongoDbClient-X.Y.Z`)  
**Target Frameworks:** `net8.0`, `netstandard2.0`

## Repository Structure

```
.github/
  workflows/          — CI/CD pipelines
  instructions/       — Coding/testing/documentation instructions
  prompts/            — Review/commit/skill prompts
  agents/             — Agent definitions (this file)
src/                  — Library source code
test/                 — xUnit tests
examples/             — Sample application
build/                — MSBuild infrastructure (props, rulesets, stylecop)
docs/                 — Documentation
```

## Build Commands

| Command | Purpose |
|---------|---------|
| `dotnet build src/` | Build library |
| `dotnet build test/` | Build tests |
| `dotnet test test/` | Run tests |
| `dotnet pack src/.../csproj -c Release` | Create NuGet package |
| `dotnet build examples/MongoDbClientSample` | Build sample app |
| `make build` | Build sample via Makefile |
| `make run` | Docker infra + build + run sample |
| `make infra-up` | Start MongoDB + Elastic Stack |
| `make validate-all` | Query APM data from Elasticsearch |

## Architecture

1. **Entry Points** — `TracerProviderBuilderExtensions.AddMongoDbClientInstrumentation()` and `MeterProviderBuilderExtensions.AddMongoDbClientInstrumentation()`
2. **Core** — `MongoDbClientInstrumentation` singleton, subscribes to MongoDB.Driver.Core events via `DiagnosticListener`
3. **Options** — `MongoDbClientTraceInstrumentationOptions` (Filter, Enrich, RecordException) and `MongoDbClientMetricsInstrumentationOptions`
4. **Implementation** — `MongoDbClientActivitySource` (ActivitySource wrapper), `MongoDbClientMetrics` (Meter + instruments), `MongoDbClientEventSource` (EventSource logging)

## Patterns & Conventions

- **Singleton with ref counting**: `Interlocked.Increment/Decrement` in `MongoDbClientInstrumentation`
- **Reflection-based event hooks**: Subscribe to `MongoDB.Driver.Core` DiagnosticListener events
- **Activity creation**: Via `MongoDbClientActivitySource.StartActivity()` using a shared `ActivitySource`
- **Semantic conventions**: Use `db.system`, `db.name`, `db.operation`, `db.mongodb.collection` attributes
- **C#**: Nullable enabled, implicit usings disabled, WarningsAsErrors, StyleCop enforced
- **Testing**: xUnit + Moq, Arrange-Act-Arrange, test per behavior

## Common Tasks

### Adding a new option
1. Add property to `MongoDbClientTraceInstrumentationOptions` or `MongoDbClientMetricsInstrumentationOptions`
2. Apply the option in `MongoDbClientInstrumentation` (in the event handler)
3. Test the option in a new test method

### Adding a new trace attribute
1. Add to the activity in `MongoDbClientInstrumentation` command-started or command-succeeded handler
2. Follow OpenTelemetry semantic conventions for attribute naming

### Adding a new metric
1. Define the instrument in `MongoDbClientMetrics`
2. Record the measurement in `MongoDbClientInstrumentation`
3. Add the metric to the `AddMeter` call via the extension method (if needed)

## Versioning

- Tag `Instrumentation.MongoDbClient-1.0.0` → NuGet version `1.0.0`
- Untagged builds get prerelease suffix: `1.0.0-alpha.0.{N}`
- Tags pushed to `main` trigger the release

## Deployment

- CI (`ci.yml`) runs on push/PR to main: lint → build-test → validate-packages
- Package validation (`package-validation.yml`) packs and pushes to GitHub Packages
- NuGet source: `https://nuget.pkg.github.com/DevComputaria/index.json`
- Auth via `GITHUB_TOKEN` with `packages: write`
