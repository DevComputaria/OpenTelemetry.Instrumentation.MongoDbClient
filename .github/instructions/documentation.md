# Documentation Instructions for AI Agents

## README.md
- Keep concise: overview, quick start, configuration options, examples
- Use code snippets in C# with XML-doc style
- Table for compatibility matrix
- Updated whenever public API changes

## CHANGELOG.md
- Keep a Changelog format
- Group by: Added, Fixed, Changed, Deprecated, Removed, Security
- Reference GitHub issues/PRs when applicable
- Date format: YYYY-MM-DD
- Version sections via git tags

## Code Documentation
- Public API: XML doc comments required (`<summary>`, `<param>`, `<returns>`)
- Internal code: no XML doc comments unless asked
- Inline comments: explain WHY, not WHAT
- Link to OpenTelemetry spec docs when implementing conventions

## Agent Instructions (.github/)
- `instructions/coding.md` — code conventions for AI agents
- `instructions/testing.md` — testing patterns
- `instructions/documentation.md` — writing docs
- `AGENTS.md` (root) — full project context for AI agents
