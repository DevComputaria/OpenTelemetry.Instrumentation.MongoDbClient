# Coding Instructions for AI Agents

## Language & Framework
- C# 12+, .NET 8.0+, nullable enabled, implicit usings disabled
- Follow StyleCop rules (see `build/stylecop.json`)
- Warnings as Errors — never suppress warnings

## Code Style
- One class per file, one file per class
- Namespaces: `OpenTelemetry.Instrumentation.MongoDbClient` (public), `.Implementation` (internal)
- Private fields: `_camelCase` with underscore prefix
- Use `ActivitySource` for tracing, `Meter` for metrics
- No XML doc comments on internal members (unless asked)
- Use `ConfigureAwait(false)` in library code (not in examples/tests)

## Tests
- xUnit + Moq
- Test class per target class
- Arrange-Act-Assert pattern
- Use `GitHubActionsTestLogger` only in CI (not compatible with .NET SDK 10)

## Build System
- `Directory.Build.props` root sets LangVersion, Nullable, WarningsAsErrors
- `src/Directory.Build.props` adds package metadata (Authors, Company, Copyright)
- `test/Directory.Build.props` and `examples/Directory.Build.props` set `IsPackable=false`
- `build/Common.props` for production, `build/Common.nonprod.props` for non-production
- Package versions managed centrally in `Directory.Packages.props`

## Instrumentation Pattern
- Singleton instrumentation class with `IDisposable`
- Reference counting via `Interlocked.Increment/Decrement`
- Subscribe to driver events using reflection (`DiagnosticListener` or `EventSubscriber`)
- Always check `Activity.Current` before creating spans

## NuGet Package
- Pack via `dotnet pack src/.../csproj --configuration Release`
- Versioned via MinVer (git tags)
- Pushed to GitHub Packages: `https://nuget.pkg.github.com/DevComputaria/index.json`

## Git Workflow
- Conventional Commits format
- Signed-off commits
- Branch from `main`, PR to `main`
- Keep changes focused and atomic
