# Copilot Instructions for Sinking

## Project Overview
Sinking is a C#/.NET project for a 2-way sync service between Jira, GitHub, and Azure DevOps issues. The project uses Visual Studio solution structure and follows standard .NET development practices and conventions.

## Development Environment
- **Platform**: .NET (C#)
- **IDE**: Visual Studio (solution file: `src/Sinking.sln`)
- **Build System**: MSBuild/.NET CLI
- **Source Structure**: `src/` directory contains the main solution

## Coding Standards & Conventions
- Follow standard C# coding conventions and .NET naming guidelines
- Use PascalCase for public members, camelCase for private fields
- Maintain consistency with existing code style in the repository
- Ensure proper XML documentation for public APIs

## Build & Testing Guidelines
- Projects should build cleanly with no warnings when possible
- Follow .NET project structure conventions
- Use appropriate .NET project types (Console, Library, Web, etc.)
- Include proper package references and dependencies

## Repository Practices
- Keep commits focused and atomic
- Write clear, descriptive commit messages
- Maintain the existing .gitignore patterns for .NET projects
- Preserve the MIT license terms when adding new files

## Code Review Considerations
- CodeRabbit is configured for automated reviews
- Ensure code changes are minimal and focused
- Test changes thoroughly before committing
- Consider backward compatibility when making API changes

## Project-Specific Notes
- This appears to be an early-stage project with basic structure in place
- Maintain simplicity while building out functionality
- Follow .NET best practices for project organization and dependency management