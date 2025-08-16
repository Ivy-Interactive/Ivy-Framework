# Contributing to Ivy Framework

Thank you for your interest in contributing to Ivy Framework! This document provides guidelines and information for contributors.

## Table of Contents

- [Code of Conduct](#code-of-conduct)
- [Getting Started](#getting-started)
- [Development Setup](#development-setup)
- [Contributing Guidelines](#contributing-guidelines)
- [Pull Request Process](#pull-request-process)
- [Code Style and Standards](#code-style-and-standards)
- [Testing](#testing)
- [Documentation](#documentation)
- [Reporting Issues](#reporting-issues)
- [Feature Requests](#feature-requests)

## Code of Conduct

This project and everyone participating in it is governed by our [Code of Conduct](CODE_OF_CONDUCT.md). By participating, you are expected to uphold this code.

## Getting Started

1. **Fork** the repository on GitHub
2. **Clone** your fork locally
3. **Set up** the development environment (see [Development Setup](#development-setup))
4. **Create** a feature branch for your changes
5. **Make** your changes
6. **Test** your changes thoroughly
7. **Submit** a pull request

## Development Setup

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/9.0)
- [Node.js 22.12+ & npm](https://docs.npmjs.com/downloading-and-installing-node-js-and-npm)

### Setup Steps

1. **Clone your fork:**

   ```bash
   git clone https://github.com/YOUR_USERNAME/Ivy-Framework.git
   cd Ivy-Framework
   ```

2. **Follow the development setup in the main [README.md](README.md#developer-build)**

   The main README contains detailed instructions for:
   - Building the frontend
   - Pre-generating documentation files
   - Running the backend (Ivy.Samples or Ivy.Docs)
   - Opening the application in your browser

## Contributing Guidelines

### Before You Start

- Check existing issues and pull requests to avoid duplicates
- Discuss major changes in an issue before starting work
- Ensure your changes align with the project's goals and architecture

### What We're Looking For

- **Bug fixes** - Help us squash bugs and improve stability
- **Feature enhancements** - Add new functionality that benefits users
- **Documentation improvements** - Help make Ivy easier to understand and use
- **Performance optimizations** - Make Ivy faster and more efficient
- **UI/UX improvements** - Enhance the user experience

### What We're NOT Looking For

- Breaking changes without discussion
- Changes that don't align with Ivy's architecture
- Features that are too specific to individual use cases
- Changes that compromise security or performance

## Pull Request Process

1. **Create a descriptive branch name:**

   ```text
   feature/add-new-widget
   fix/button-styling-issue
   docs/update-getting-started
   ```

2. **Write a clear commit message:**

   ```text
   feat: add new BadgeWidget component
   fix: resolve button hover state issue
   docs: update installation instructions
   ```

3. **Ensure your code follows our standards:**
   - Passes all tests
   - Follows code style guidelines
   - Includes appropriate documentation
   - Doesn't introduce new warnings

4. **Update documentation** if your changes affect:
   - Public APIs
   - User-facing features
   - Installation or setup processes

5. **Submit your pull request** with:
   - Clear description of changes
   - Link to related issues
   - Screenshots for UI changes
   - Test results

## Code Style and Standards

### Linting and Formatting

Before submitting your code, ensure it follows the project's linting and formatting rules:

#### Backend (C#)

- Use `dotnet format` to format C# code according to the project's ruleset
- Run this command in the root directory or specific project directories

#### Frontend (TypeScript/React)

- Use `npm run lint:fix` to automatically fix ESLint issues
- Use `npm run format` to format code with Prettier
- These commands should be run from the `frontend/` directory

#### Pre-commit Hooks

The project uses pre-commit hooks that will automatically run linting and formatting on staged files. Make sure your code passes these checks before committing. More details how pre-commit hooks are being set up can be found in `./frontend/README.md`

TL;DR, if you have run `npm install` in the `./frontend`, most likely all pre-commit hooks will be working for you, that cover linting and formatting for Ivy-Framework.

### C# Guidelines

- Follow [Microsoft C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- Use meaningful variable and method names
- Add XML documentation for public APIs
- Keep methods focused and concise
- Use async/await for asynchronous operations

### TypeScript/React Guidelines

- Follow [TypeScript style guide](https://github.com/microsoft/TypeScript/wiki/Coding-guidelines)
- Use functional components with hooks
- Prefer TypeScript over JavaScript
- Avoid using `any` type - use proper typing
- Follow React best practices

### General Guidelines

- Write self-documenting code
- Add comments for complex logic
- Keep functions small and focused
- Use consistent naming conventions
- Follow the DRY principle

## Testing

### Running Tests

```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test Ivy.Test

# Run frontend tests
cd frontend
npx playwright test
```

### Writing Tests

- Write tests for new functionality
- Ensure existing tests pass
- Use descriptive test names
- Test both happy path and edge cases
- Mock external dependencies

## Documentation

### Documentation Guidelines

- Write clear, concise documentation
- Use proper Markdown formatting
- Include code examples where appropriate
- Keep documentation up to date with code changes

## Reporting Issues

### Before Reporting

- Check existing issues for duplicates
- Try to reproduce the issue
- Gather relevant information

### Issue Template

When reporting an issue, include:

- **Description** - Clear description of the problem
- **Steps to reproduce** - Detailed steps to recreate the issue
- **Expected behavior** - What you expected to happen
- **Actual behavior** - What actually happened
- **Environment** - OS, .NET version, Node version, etc.
- **Screenshots** - If applicable
- **Code samples** - Minimal code to reproduce the issue

## Feature Requests

### Before Requesting

- Check if the feature already exists
- Consider if it aligns with Ivy's goals
- Think about the implementation complexity

### Feature Request Template

- **Description** - What the feature should do
- **Use case** - Why this feature is needed
- **Proposed solution** - How it could be implemented
- **Alternatives** - Other ways to achieve the goal
- **Impact** - Who would benefit from this feature

## Getting Help

- **GitHub Issues** - For bugs and feature requests
- **GitHub Discussions** - For questions and general discussion
- **Documentation** - Check the [Ivy.Docs](Ivy.Docs/) for guides and examples

Thank you for contributing to Ivy Framework! Your contributions help make Ivy better for everyone.
