# Development Workflow Instructions

## Project Setup
- **Solution File**: `src/Sinking.sln`
- **Project Type**: C#/.NET
- **Development Environment**: Visual Studio compatible

## Workflow Guidelines

### Getting Started
1. Open the solution file in Visual Studio or compatible IDE
2. Restore NuGet packages if needed
3. Build the solution to ensure everything compiles
4. Run any existing tests

### Making Changes
1. Create focused branches for feature work
2. Write code following C# conventions
3. Build and test locally before committing
4. Keep changes minimal and well-tested

### Code Quality
- Follow .NET coding standards
- Use proper C# naming conventions
- Include XML documentation for public APIs
- Handle exceptions appropriately
- Consider performance implications

### Testing Strategy
- Write unit tests for new functionality
- Ensure existing functionality isn't broken
- Test edge cases and error conditions
- Use appropriate .NET testing frameworks

### Dependencies
- Use NuGet for package management
- Maintain package versions responsibly
- Avoid unnecessary dependencies
- Document any special requirements

## Repository Conventions
- **Commits**: Use clear, descriptive messages
- **Branches**: Use meaningful branch names
- **Issues**: Reference related issues in commits
- **Reviews**: CodeRabbit is configured for automated reviews

## Build Process
- Solution should build cleanly
- Address compiler warnings when possible
- Ensure compatibility with target .NET version
- Consider both development and production configurations