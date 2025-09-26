# Coding Agent Instructions for Sinking

## Context
This is a C#/.NET project in early development stages for a 2-way sync service between Jira, GitHub, and Azure DevOps issues. The project uses Visual Studio solution structure with source code in the `src/` directory.

## Key Guidelines

### Code Organization
- All source code should be placed in the `src/` directory.
- Follow standard .NET project structure: console app entry in Program.cs, services for Jira/GitHub/AzureDevOps, converters for unified issues, sync engine for logic.
- Use appropriate project types: main app project, test project for unit tests.
- Maintain clean separation: API services, data models, database access, logging.

### Development Practices
- Write clean, readable C# code following .NET conventions.
- Use meaningful names for classes (e.g., UnifiedIssue, JiraService), methods, and variables.
- Include appropriate error handling, logging with Microsoft.Extensions.Logging (console provider), and async patterns.
- Write unit tests (xUnit) for issue conversions: Jira/GitHub/AzureDevOps to/from UnifiedIssue, including edge cases.
- Implement full functionality: poll systems, fetch updates, convert/diff/propagate changes, handle conflicts (last-modified wins), retries, rate limits.
- Don't stop until all code is implemented, builds, and tests pass.

### Dependencies & Packages
- Minimize 3rd-party libraries: use official SDKs only if essential (Atlassian.Jira, Octokit, Microsoft.VisualStudio.Services.Client); prefer HttpClient for APIs.
- Use NuGet for SQLite (Microsoft.Data.Sqlite), logging (Microsoft.Extensions.Logging.Console).
- Keep package versions up to date with security considerations.
- Document any special dependency requirements in README.

### Build & Deployment
- Ensure code builds cleanly with the existing solution file.
- Address compiler warnings when possible.
- Consider both Debug and Release configurations.
- Test changes locally: run sync loop, verify conversions/diffs/syncs.

### Repository Standards
- Follow the existing .gitignore patterns.
- Keep commit messages clear and descriptive (e.g., "Add UnifiedIssue model and converters").
- Make focused, atomic commits.
- Respect the MIT license terms.

## Technology Stack
- **Language**: C#
- **Framework**: .NET (async, DI via Microsoft.Extensions.DependencyInjection).
- **Database**: SQLite for internal storage (tables: UnifiedIssues, SyncMappings, SyncLogs).
- **Logging**: .NET logging with console.
- **Build Tool**: MSBuild/Visual Studio.
- **License**: MIT.

## Special Considerations
- This project is in early development - prioritize clean, simple implementations for 2-way sync.
- Define UnifiedIssue with Title, Description, Status (enum), Priority (enum), Assignee, Labels, Dates, Comments, Attachments
- Use hashes/field diffs for change detection.
- Config via appsettings.json: API keys, URLs, SQLite path, poll interval.
- Consider future extensibility but avoid over-engineering.
- Maintain compatibility with standard .NET tooling.
- Follow Microsoft's .NET coding guidelines and best practices.