Review the following code changes for the OpenTelemetry.Instrumentation.MongoDbClient project.

Focus on:

1. **Correctness**
   - Does the code follow OpenTelemetry semantic conventions?
   - Are Activity sources properly created and disposed?
   - Is reference counting implemented correctly for singleton instrumentation?
   - Are exceptions handled without breaking the application?

2. **Style & Conventions**
   - Follows C# conventions (nullable enabled, no implicit usings)
   - One class per file
   - Correct namespacing (`OpenTelemetry.Instrumentation.MongoDbClient.Implementation`)
   - StyleCop rules respected

3. **Testing**
   - Are there tests for new functionality?
   - Do existing tests still pass?
   - Are edge cases covered (null, empty, disposed states)?

4. **Performance**
   - No allocations in hot paths
   - `Activity.Current` checked before creating spans
   - Filter checked before enrichment

5. **Backward Compatibility**
   - Public API changes are additive (not breaking)
   - Options class changes are backward compatible
   - XML doc comments updated for new public APIs
