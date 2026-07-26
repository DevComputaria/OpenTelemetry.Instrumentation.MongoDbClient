# Testing Instructions for AI Agents

## Test Framework
- xUnit.net v2
- Moq for mocking
- `dotnet test test/` to run all tests
- Target framework: `net8.0`

## Test Patterns
- Follow Arrange-Act-Assert (AAA)
- One test method per behavior/scenario
- Use meaningful test names: `MethodName_StateUnderTest_ExpectedBehavior`
- Group related tests in a single class
- Use `Theory` + `InlineData` for parameterized tests

## Mocking
- Use Moq for interfaces and abstract classes
- Create `Mock<T>()` for driver types
- Avoid mocking concrete classes; prefer interfaces

## What to Test
- Extension methods register properly with `TracerProviderBuilder` / `MeterProviderBuilder`
- Instrumentation starts/stops correctly (reference counting)
- Options are applied (filter, enrich, record exception)
- Activities are created with correct names and tags
- Metrics are recorded correctly
- Event handlers (started/succeeded/failed) produce correct telemetry
- Edge cases: null/empty options, disposed instrumentation, driver exceptions

## What NOT to Test
- MongoDB driver internals (assume driver works correctly)
- OpenTelemetry SDK internals
- Network connectivity or integration scenarios (use mocks)
- Performance (use dedicated benchmarks)

## Coverage
- Aim for >80% line coverage on `src/` code
- Cover all public API surfaces
- Cover exception paths and error handling
