# AGENTS.md — OpenTelemetry.Instrumentation.MongoDbClient

## Project Overview

Auto-instrumentation library for MongoDB client operations using OpenTelemetry. Provides tracing spans and metrics for MongoDB commands via `MongoDB.Driver.Core` event hooks.

## Directory Structure

```
.
├── .github/                          # GitHub workflows, instructions, prompts
│   ├── workflows/                    # CI/CD pipelines
│   ├── instructions/                 # Copilot/agent instructions
│   └── prompts/                      # Agent prompt definitions
├── src/
│   └── OpenTelemetry.Instrumentation.MongoDbClient/  # Library source
│       ├── Implementation/           # Internal implementation
│       ├── Internal/                 # Internal helpers
│       ├── *Instrumentation.cs       # Core instrumentation classes
│       └── *Extensions.cs            # TracerProviderBuilder/MeterProviderBuilder extensions
├── test/
│   └── OpenTelemetry.Instrumentation.MongoDbClient.Tests/
├── examples/
│   └── MongoDbClientSample/          # Sample app with OTLP export
├── build/                            # MSBuild infrastructure
│   ├── Common.props                  # Shared build properties
│   ├── Common.nonprod.props          # Test/example build properties
│   ├── OpenTelemetry.prod.ruleset    # Production code analysis rules
│   ├── OpenTelemetry.test.ruleset    # Test code analysis rules
│   └── stylecop.json                 # StyleCop configuration
├── docs/                             # Documentation
├── docker-compose.yml                # Observability stack (MongoDB + Elastic)
└── otel-collector-config.yml         # OTel Collector config
```

## Build & Test Commands

| Command | Description |
|---------|-------------|
| `make build` | Build sample project |
| `make run` | Start infra + build + run sample |
| `dotnet build src/` | Build library only |
| `dotnet build test/` | Build tests only |
| `dotnet test test/` | Run tests |
| `dotnet pack src/.../csproj -c Release` | Pack NuGet |
| `make infra-up` | Start Docker stack |
| `make infra-down` | Stop Docker stack |
| `make validate-all` | Query APM data in Elasticsearch |

## Versioning

- Uses **MinVer** — version derived from git tags prefixed `Instrumentation.MongoDbClient-v`
- Tag `Instrumentation.MongoDbClient-1.0.0` produces version `1.0.0`
- Untagged commits get prerelease suffix: `1.0.0-alpha.0.{N}`

## Conventions

### Code
- C# with nullable enabled, implicit usings disabled
- Warnings as Errors enabled
- StyleCop rules enforced (see `build/stylecop.json`)
- No XML doc comments in internal code (unless asked)
- One class per file
- Namespace: `OpenTelemetry.Instrumentation.MongoDbClient.{Implementation}`

### Git
- Conventional Commits (`feat:`, `fix:`, `docs:`, `refactor:`, `test:`, `chore:`)
- Commits in English (or Portuguese if user prefers)
- Commits must be `signed-off`

### CI/CD
- `ci.yml` orchestrates: detect-changes → lint, build-test, validate-packages
- `package-validation.yml` packs library and pushes to GitHub Packages
- `ossf-scorecard.yml` runs weekly
- `dependabot.yml` updates NuGet and Actions dependencies

## Architecture

1. **Extension Methods** (`TracerProviderBuilderExtensions`, `MeterProviderBuilderExtensions`) — public API entry points
2. **Instrumentation** (`MongoDbClientInstrumentation`) — core class, subscribes to MongoDB driver events via reflection
3. **Options** (`MongoDbClientTraceInstrumentationOptions`, `MongoDbClientMetricsInstrumentationOptions`) — filter, enrich, record exception config
4. **Implementation** (`MongoDbClientActivitySource`, `MongoDbClientEventSource`, `MongoDbClientMetrics`) — internal details
5. **Diagnostic Listener** hooks into `MongoDB.Driver.Core` events for command started/succeeded/failed

## NuGet Publishing

- Packages pushed to GitHub Packages via `package-validation.yml`
- NuGet source: `https://nuget.pkg.github.com/DevComputaria/index.json`
- Auth via `GITHUB_TOKEN` with `packages: write` permission

## Target Frameworks

- `net8.0` — primary
- `netstandard2.0` — legacy support

## Key Dependencies

- `OpenTelemetry` 1.7.0
- `MongoDB.Driver.Core` 2.19.0
- `MinVer` 4.3.0 (for versioning)
