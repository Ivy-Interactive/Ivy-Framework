# Contributing to Ivy Framework

Thank you for your interest in contributing to Ivy Framework! This document provides guidelines and information for contributors.

## Table of Contents

- [Code of Conduct](#code-of-conduct)
- [Getting Started](#getting-started)
- [Development Setup](#development-setup)
- [Contributing Guidelines](#contributing-guidelines)
- [Widget Contribution Requirements](#widget-contribution-requirements)
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
- **Ivy Console Tool** (for development):

  ```bash
  # For stable releases
  dotnet tool install -g Ivy.Console

  # For prerelease versions (recommended for contributors)
  dotnet tool install -g Ivy.Console --prerelease
  ```

  - **ARM Mac Users**: [Rosetta 2](https://support.apple.com/en-us/HT211861) is required for the Google Protocol Buffers package to work properly:

  ```bash
  # Install Rosetta 2 if not already installed
  /usr/sbin/softwareupdate --install-rosetta --agree-to-license
  ```

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

## Testing

Ivy Framework uses multiple testing approaches to ensure code quality:

### Unit Testing

- **Backend (C#)**: Run `dotnet test` in the root directory
- **Frontend (TypeScript)**: Run `npm run test` in the `frontend/` directory

### End-to-End Testing

E2E tests are written using Playwright and are located in `frontend/e2e/`. We provide npm scripts for running these tests:

```bash
# Run all E2E tests
npm run e2e

# Run only Ivy.Docs E2E tests
npm run e2e:docs

# Run only Ivy.Samples E2E tests
npm run e2e:samples
```

**Important**: Always use the npm scripts (`npm run e2e*`) instead of `npx playwright test` to ensure consistent usage of the locally installed Playwright version and avoid version conflicts.

Additional Playwright options can be passed after `--`:

```bash
npm run e2e -- --headed  # Run tests in headed mode
npm run e2e -- --debug   # Run tests in debug mode
npm run e2e:samples -- --project=chromium  # Run samples tests in Chrome only
```

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
- **Widget contributions** - New widgets that extend Ivy's functionality (see [Widget Contribution Requirements](#widget-contribution-requirements))

### What We're NOT Looking For

- Breaking changes without discussion
- Changes that don't align with Ivy's architecture
- Features that are too specific to individual use cases
- Changes that compromise security or performance

## Widget Contribution Requirements

When contributing a new widget to Ivy Framework, your submission **must** include all of the following components:

### 1. Backend Widget Implementation (C#)

- **Widget class** in `Ivy/Widgets/` with proper inheritance from `WidgetBase<T>`
- **Comprehensive XML documentation** with `<summary>`, `<param>`, and `<remarks>` tags
- **Property documentation** for all public properties using `[Prop]` attribute
- **Constructor overloads** where appropriate for different use cases
- **Proper namespace** declaration (`namespace Ivy;`)

### 2. Frontend Widget Implementation (React/TypeScript)

- **React component** in `frontend/src/widgets/` following TypeScript conventions
- **Props interface** with proper typing and optional properties
- **Consistent styling** using only Tailwind CSS and shadcn/ui components
- **Accessibility support** with proper ARIA attributes and semantic HTML
- **Responsive design** that works across different screen sizes

### 3. Documentation Requirements

- **XML documentation** on the C# widget class (comprehensive, not verbose)
- **Code examples** showing common usage patterns
- **Property descriptions** for all configurable options
- **Integration examples** demonstrating how the widget fits into larger applications

### 4. Visual Requirements

- **Screenshots** showing the widget in different states (normal, disabled, error, etc.)
- **Multiple variants** if the widget supports different visual styles
- **Light and dark mode** examples where applicable
- **Responsive behavior** screenshots on different screen sizes

### 5. Testing Requirements

- **C# unit tests** in `Ivy.Test/` covering widget functionality
- **Frontend unit tests** using Vitest in `frontend/src/` with `.test.ts` extension
- **Frontend E2E tests** using Playwright in `frontend/e2e/`
  - Run all E2E tests: `npm run e2e`
  - Run Ivy.Docs tests: `npm run e2e:docs`
  - Run Ivy.Samples tests: `npm run e2e:samples`
- **Edge case testing** including null values, empty states, and error conditions
- **Accessibility testing** ensuring proper keyboard navigation and screen reader support

### 6. Package Dependencies

- **Approved dependencies only**: New widgets may only use existing dependencies
- **No new npm packages** without prior approval and discussion
- **Stick to shadcn/ui**: Use existing shadcn/ui components where possible
- **Tailwind CSS only**: No custom CSS files or external styling libraries

### 7. Contributor-Friendly Issues

When creating issues for widget contributions, use the `contributors-welcome` label and ensure:

- **Clear requirements** with detailed specifications
- **Design mockups** or examples when possible
- **Acceptance criteria** that can be easily verified
- **Difficulty level** indication (beginner, intermediate, advanced)

### Widget Submission Checklist

Before submitting your widget PR, ensure you have:

- [ ] Backend C# widget class with full XML documentation
- [ ] Frontend React component with TypeScript interfaces
- [ ] Comprehensive unit tests (C#) and frontend unit tests (Vitest)
- [ ] E2E tests (Playwright) for user interactions
- [ ] Screenshots showing different widget states
- [ ] Usage examples in documentation
- [ ] No new external dependencies added
- [ ] Accessibility features implemented
- [ ] Responsive design verified
- [ ] Code follows project style guidelines
- [ ] All existing tests still pass

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

### Frontend Package Selection Rules

**IMPORTANT**: Ivy Framework has strict guidelines for frontend dependencies to maintain consistency and avoid bloat.

#### Approved Libraries (Use These)

- **shadcn/ui** - Primary UI component library
- **Tailwind CSS** - For all styling (no custom CSS files)
- **Radix UI** - Underlying primitives (already included via shadcn/ui)
- **Lucide React** - Icon library
- **React Hook Form** - Form handling
- **Zod** - Schema validation

#### Package Addition Policy

- **No new npm packages** without explicit approval
- **Discuss first** - Open an issue before adding any dependency
- **Justify necessity** - Explain why existing tools can't solve the problem
- **Consider bundle size** - New packages must have minimal impact
- **Security review** - All packages undergo security evaluation

#### What NOT to Use

- ❌ Custom CSS files or styled-components
- ❌ Alternative UI libraries (Material-UI, Ant Design, etc.)
- ❌ Different icon libraries (FontAwesome, Heroicons, etc.)
- ❌ Utility libraries that duplicate existing functionality
- ❌ Packages that conflict with our existing stack

### General Guidelines

- Write self-documenting code
- Add comments for complex logic
- Keep functions small and focused
- Use consistent naming conventions
- Follow the DRY principle

## Testing

### Running Tests

```bash
# Run all backend tests
dotnet test

# Run specific test project
dotnet test Ivy.Test

# Run frontend unit tests
cd frontend
npm run test

# Run all frontend E2E tests
cd frontend
npm run e2e

# Run specific E2E test suites
npm run e2e:docs      # Ivy.Docs tests only
npm run e2e:samples   # Ivy.Samples tests only
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
- **Join our Discord** - https://discord.gg/sSwGzZAYb6
- **Join our Slack** - If you want, we can invite you to a dedicated Slack channel for Ivy Framework development.
- **Documentation** - Check the [Ivy.Docs](Ivy.Docs/) for guides and examples

Thank you for contributing to Ivy Framework! Your contributions help make Ivy better for everyone.
