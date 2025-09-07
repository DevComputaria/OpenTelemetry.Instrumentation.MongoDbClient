# Code Style Guidelines

This document outlines the coding standards and style guidelines for OpenTelemetry instrumentation libraries.

## General Principles

- Maintain consistency with the OpenTelemetry style in the main repository and other instrumentation libraries
- Follow C# best practices and .NET guidelines
- Prioritize readability and maintainability

## Naming Conventions

### Classes and Types

- Use PascalCase for class names and public members
- Use descriptive, clear names that indicate purpose
- Suffix instrumentation implementations with their type (e.g., `Listener`, `ActivitySource`)
- Prefix implementation-specific classes with the target library name (e.g., `MongoDbClientActivitySource`)

### Methods and Parameters

- Use PascalCase for public methods
- Use camelCase for parameters and local variables
- Prefix boolean parameters with auxiliary verbs (is, has, should, etc.)

### Constants and Fields

- Use PascalCase for public constants and static readonly fields
- Use camelCase with underscore prefix for private fields (`_fieldName`)
- For public API constants, use descriptive names without abbreviations

## Code Organization

### Namespaces

- Use the following namespace hierarchy:
  - `OpenTelemetry.Instrumentation.{LibraryName}` - Main namespace
  - `OpenTelemetry.Instrumentation.{LibraryName}.Implementation` - Internal implementation classes
  - `OpenTelemetry.Trace` - Extension methods for TracerProviderBuilder
  - `OpenTelemetry.Metrics` - Extension methods for MeterProviderBuilder

### File Structure

- One class per file (except for small, closely related classes)
- Follow the namespace structure in folder organization
- Group related implementation files in folders

## Documentation

### XML Documentation

- Document all public APIs with XML comments
- Include `<summary>` for all public types and members
- Document parameters with `<param>` and return values with `<returns>`
- Include `<remarks>` for additional important information
- Link to related items using `<see>` and `<seealso>`

### Comments

- Use `//` for single-line comments
- Use `/* */` for multi-line comments
- Write comments that explain "why" not "what"
- Keep comments updated when code changes

## Error Handling

- Use specific exception types for different error conditions
- Log errors using the instrumentation's EventSource
- Consider backwards compatibility when handling errors
- Don't swallow exceptions in instrumentation code

## Testing

- Write unit tests for all public APIs
- Write integration tests for end-to-end functionality
- Use mocks or fakes for dependencies
- Test edge cases and error conditions

## Example

```csharp
/// <summary>
/// Helper class for MongoDB client instrumentation activities.
/// </summary>
internal static class MongoDbClientActivitySource
{
    private static readonly ActivitySource ActivitySource = new ActivitySource("OpenTelemetry.Instrumentation.MongoDbClient");

    /// <summary>
    /// Starts a new activity for MongoDB client operations.
    /// </summary>
    /// <param name="name">The name of the activity.</param>
    /// <param name="kind">The kind of activity to create.</param>
    /// <returns>A new <see cref="Activity"/> or null if instrumentation is disabled.</returns>
    public static Activity StartActivity(string name, ActivityKind kind = ActivityKind.Client)
    {
        return ActivitySource.StartActivity(name, kind);
    }
}
```
