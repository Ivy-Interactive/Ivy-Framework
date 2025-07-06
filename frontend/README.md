# Frontend

## Development

```bash
npm run dev
npm run build
```

## Code Quality

This project uses ESLint and Prettier for code quality and formatting, with automatic pre-commit hooks.

### Code Formatting

Format all files with Prettier:

```bash
npm run format
```

Check if files are properly formatted:

```bash
npm run format:check
```

### Linting

Check for linting issues:

```bash
npm run lint
```

Automatically fix linting issues:

```bash
npm run lint:fix
```

### Pre-commit Hooks

The project uses Git pre-commit hooks to automatically:

- Run ESLint with auto-fix on staged files
- Format staged files with Prettier
- Block commits if any issues remain

The pre-commit hook is configured at the repository root and automatically detects the frontend directory.

### Configuration Files

- `.prettierrc` - Prettier configuration
- `.prettierignore` - Files to exclude from formatting
- `eslint.config.js` - ESLint configuration with Prettier integration
- `package.json` - Contains lint-staged configuration and scripts

## Testing

This project uses Playwright for end-to-end testing.

### Prerequisites

Make sure you're in the frontend directory:

```bash
cd frontend
```

### Install Dependencies

```bash
npm ci
```

### Install Playwright Browsers

```bash
npx playwright install --with-deps
```

### Running Tests

Run all tests:

```bash
npx playwright test
```

Run tests in a specific browser:

```bash
npx playwright test --project=chromium
npx playwright test --project=firefox
npx playwright test --project=webkit
```

Run tests in headed mode (to see the browser):

```bash
npx playwright test --headed
```

Run tests in debug mode:

```bash
npx playwright test --debug
```

Run a specific test file:

```bash
npx playwright test example.spec.ts
```

### Test Reports

View the HTML test report:

```bash
npx playwright show-report
```

### Test Files

- `e2e/` - End-to-end test files

### CI/CD

Tests are automatically run in GitHub Actions on push to main/master branches and pull requests. The CI pipeline includes:

1. Code formatting checks (`npm run format:check`)
2. Linting checks (`npm run lint`)
3. Playwright end-to-end tests

## Available Scripts

| Script                 | Description                           |
| ---------------------- | ------------------------------------- |
| `npm run dev`          | Start development server              |
| `npm run build`        | Build for production                  |
| `npm run preview`      | Preview production build              |
| `npm run lint`         | Check for linting issues              |
| `npm run lint:fix`     | Fix linting issues automatically      |
| `npm run format`       | Format all files with Prettier        |
| `npm run format:check` | Check if files are properly formatted |
