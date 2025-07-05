# Frontend

## Development

```bash
npm run dev
npm run build
```

### Linting

To check code formatting and linting rules:

```bash
npm run lint
```

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

Tests are automatically run in GitHub Actions on push to main/master branches and pull requests.
