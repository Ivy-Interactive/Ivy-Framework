# Frontend

**Node.js Version Requirement**: This project requires Node.js version 22.12.0 or greater, and uses **pnpm** as its package manager.

## Development

**Important Package Manager Note**: This ecosystem strictly uses `pnpm`. Running the legacy `npm install` command is deliberately blocked via a `preinstall` routine.

**For Full-Stack / .NET Developers:**
If you are simply cloning the repository and running `dotnet build` (or opening the solution in an IDE like visual studio or Rider), **you do not need to install anything manually**. We've engineered the MSBuild MSBuild targets to natively bootstrap `pnpm` safely over the network via standard `npx` routines. This completely automates the frontend installation and compilation pipeline seamlessly without requiring manual intervention.

**For Standalone Frontend Development:**
If you want to explicitly work on the frontend in isolation (using HMR for widgets), you must manually run:

```bash
vp i
```

or

```bash
pnpm i
```

After installing the packages, you can use the native Vite+ CLI:

```bash
vp dev
```

To build for production (which runs `tsc` for type-checking before bundling):

```bash
pnpm run build
```

## Developer Logging

The frontend includes a comprehensive logging system for debugging and development purposes. Detailed logging can be controlled via browser console commands.

### Console Commands

Open the browser console (F12 → Console tab) and use these commands:

```javascript
// Check current developer options
getDeveloperOptions();
// Returns: { showDetailedLogging: false }

// Toggle detailed logging on/off
toggleDeveloperLogging();
// Returns: true (if enabled) or false (if disabled)
// Also logs: "Developer logging enabled" or "Developer logging disabled"
```

### What Gets Logged

When detailed logging is enabled, you'll see debug messages for:

- **Select Input Interactions**: Value changes, conversions, clear operations
- **SignalR Communication**: Message processing, updates, events
- **Widget Tree Operations**: XML conversion, patches, updates
- **Authentication**: access token operations, theme changes
- **Error Handling**: Connection issues, parsing errors

### Log Levels

- **Debug**: Detailed information (controlled by `showDetailedLogging`)
- **Info**: General information (always visible)
- **Warn**: Warning messages (always visible)
- **Error**: Error messages (always visible)

### Persistence

Developer options are stored in localStorage and persist across:

- Page refreshes
- Browser sessions
- Browser restarts

## Code Quality

The frontend project uses **Vite+** integrated tools (**Oxlint** and **Oxfmt**) for high-performance code quality and formatting, alongside automatic pre-commit hooks. It is also responsible for handling `dotnet format` precommit hook for the BE.

### Pre-commit Hooks

We use a Husky npm package to setup precommit hooks for both the FE and the BE.

To get the auto-linting for staged files, you need to have run `pnpm install` in `./frontend` at least once. Ideally, you would not then need to run any formatting or lint commands as it will be done for you. In case you want to manually run them, you still can.

If there are issues that auto-linting and formatting can't resolve automatically, your commit will be blocked from being pushed. If you really need to push, you can specify checks behavior per commit (not recommended):

```bash
git commit --no-verify -m "Commit message"
```

### Code Formatting

Format all files with Oxfmt using the Vite+ CLI:

```bash
vp fmt .
```

Check if files are properly formatted:

```bash
vp fmt --check .
```

### Linting

Check for linting issues with Oxlint using the Vite+ CLI:

```bash
vp lint .
```

Automatically fix linting issues:

```bash
vp lint --fix .
```

### Configuration Files

- `vite.config.ts` - Contains Vite+ syntax formatting and linting preferences
- `package.json` - Contains lint-staged configuration and execution scripts

## Testing

This project uses Vitest (via Vite+) for unit testing and Playwright for end-to-end testing.

### Unit Testing with Vitest

Run unit tests interactively using Vite+:

```bash
vp test
```

Unit tests are configured to run only on files ending with `.test.ts`. Place your unit test files alongside your source code with the `.test.ts` extension.

### End-to-End Testing with Playwright

### Prerequisites

Make sure you're in the frontend directory:

```bash
cd frontend
```

### Install Dependencies

```bash
pnpm install
```

### Install Playwright Browsers

```bash
pnpm exec playwright install --with-deps
```

### Running Tests

Run all e2e tests:

```bash
pnpm run e2e
```

Run only Ivy.Docs e2e tests:

```bash
pnpm run e2e:docs
```

Run only Ivy.Samples e2e tests:

```bash
pnpm run e2e:samples
```

Run tests in a specific browser:

```bash
pnpm run e2e -- --project=chromium
pnpm run e2e -- --project=firefox
pnpm run e2e -- --project=webkit
```

Run tests in headed mode (to see the browser):

```bash
pnpm run e2e -- --headed
```

Run tests in debug mode:

```bash
pnpm run e2e -- --debug
```

Run a specific test file:

```bash
pnpm run e2e -- example.spec.ts
```

### Test Reports

View the HTML test report:

```bash
pnpm run e2e -- --reporter=html
# Then open the report
pnpm exec playwright show-report
```

### Test Files

- `e2e/` - End-to-end test files

### CI/CD

Tests are automatically run in GitHub Actions on push to main/master branches and pull requests. The CI pipeline includes:

1. Code formatting checks (`vp fmt --check .`)
2. Linting checks (`vp lint .`)
3. Unit tests (`vp test`)
4. Playwright end-to-end tests

## Available Commands and Scripts

| Command/Script         | Description                           |
| ---------------------- | ------------------------------------- |
| `vp dev`               | Start development server              |
| `pnpm run build`       | Build for production (typecheck + vp) |
| `vp preview`           | Preview production build              |
| `vp test`              | Run unit tests with Vitest            |
| `pnpm run e2e`         | Run all end-to-end tests              |
| `pnpm run e2e:docs`    | Run Ivy.Docs end-to-end tests         |
| `pnpm run e2e:samples` | Run Ivy.Samples end-to-end tests      |
| `vp lint .`            | Check for linting issues              |
| `vp lint --fix .`      | Fix linting issues automatically      |
| `vp fmt .`             | Format all files with Oxfmt           |
| `vp fmt --check .`     | Check if files are properly formatted |
