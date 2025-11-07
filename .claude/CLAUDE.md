# Ivy Framework - Claude Code Configuration

<<<<<<< HEAD
## Overview

This file contains all coding conventions, standards, and guidelines for the Ivy Framework project. These rules are always active when working with Claude Code.

---

## C# Code Conventions

### Core Principles

1. Use `async`/`await` for asynchronous operations and `ValueTask` for hot paths
2. Always format code with `dotnet format` before committing
3. Run `dotnet build` to catch compilation errors - build output is the source of truth

### XML Documentation Requirements

**All Public APIs MUST Have:**
- `<summary>` tags describing the purpose
- `<param>` tags for each parameter
- `<returns>` tags for return values

```csharp
/// <summary>
/// Processes user input and validates against the specified rules.
/// </summary>
=======
## C# Conventions

**Core Rules:**
- Use `async`/`await` for async operations, `ValueTask` for hot paths
- Run `dotnet format` before committing
- Run `dotnet build` to verify - build errors are source of truth
- PascalCase for classes/methods/properties, _camelCase for private fields, camelCase for parameters/locals

**XML Documentation (Required for Public APIs):**
```csharp
/// <summary>Processes user input and validates against rules.</summary>
>>>>>>> ivy-fork/main
/// <param name="input">The user input to process.</param>
/// <param name="rules">The validation rules to apply.</param>
/// <returns>A validation result indicating success or failure.</returns>
public ValidationResult ProcessInput(string input, ValidationRules rules) { }
```

<<<<<<< HEAD
### Naming Conventions

- **Classes**: PascalCase (`UserService`, `DataProcessor`)
- **Methods**: PascalCase (`GetUserById`, `ProcessData`)
- **Properties**: PascalCase (`UserName`, `IsActive`)
- **Private fields**: _camelCase (`_userId`, `_dataStore`)
- **Parameters**: camelCase (`userId`, `inputData`)
- **Local variables**: camelCase (`result`, `userName`)

---

## TypeScript/React Conventions

### Core Principles

1. Follow [TypeScript style guide](https://github.com/microsoft/TypeScript/wiki/Coding-guidelines)
2. Use functional components with hooks
3. **NEVER use `any` type** - use proper typing
4. Use named exports over default exports
5. Follow React best practices

### Styling Requirements

**✅ ALWAYS Use:**
- Tailwind CSS for all styling
- shadcn/ui components for UI elements
- Lucide React for icons (via Icon component)
- CSS color variables: `var(--color-primary)`, `var(--color-background)`

**❌ NEVER Use:**
- styled-components or CSS-in-JS libraries (plain CSS files are allowed when needed)
- Alternative UI libraries (Material-UI, Ant Design, etc.)
- Different icon libraries (FontAwesome, Heroicons, etc.)
- Hardcoded colors (`#3b82f6`, `rgb()`) - always use CSS variables

```typescript
// ✅ GOOD
className="text-[var(--color-primary)] bg-[var(--color-background)]"
=======
## TypeScript/React Conventions

**Core Rules:**
- NEVER use `any` type - use proper typing
- Use named exports (no default exports)
- Run `npm run build` to verify - build errors are source of truth

**Styling (Required):**
- ✅ Tailwind CSS, shadcn/ui components, Lucide icons via `<Icon>`, CSS variables for colors
- ❌ NO styled-components, Material-UI, Ant Design, FontAwesome, hardcoded colors

```typescript
// ✅ GOOD
className="text-[var(--color-primary)]"
>>>>>>> ivy-fork/main
<Icon name="edit" size={16} />

// ❌ BAD
className="text-blue-500"
<ChevronRight size={20} />
```

<<<<<<< HEAD
**Theming Note:** CSS variables enable proper light/dark mode theming. Always use variables like `var(--color-primary)`, `var(--color-background)`, etc. for colors to ensure components adapt correctly to theme changes.

### Component Structure

```
src/components/UserCard/
├── UserCard.tsx           # Main component
├── UserCard.test.tsx      # Component tests
├── utils/                 # Utility functions
│   ├── formatName.ts
│   └── formatName.test.ts
└── hooks/                 # Custom hooks
    ├── useUserData.ts
    └── useUserData.test.ts
```

### TypeScript Best Practices

```typescript
// ✅ GOOD: Proper typing
interface User {
  id: string;
  name: string;
  role: 'admin' | 'user' | 'guest';
}
function processUser(user: User): void { }

// ✅ GOOD: Named exports
export function UserCard() { }

// ❌ BAD: Using 'any'
function processUser(user: any) { }

// ❌ BAD: Default exports
export default UserCard;
```

**Note:** Always run `npm run build` to catch TypeScript errors - build errors are the source of truth for type correctness.

### React Hooks Guidelines

- Custom hooks must be in `./hooks/*` folder (within the same component module) with `use` prefix
- When a component has **3+ useEffect hooks**, refactor into custom hooks
- Always type hook return values

```typescript
// hooks/useUserData.ts
interface UseUserDataResult {
  user: User | null;
  loading: boolean;
  error: Error | null;
}

export function useUserData(userId: string): UseUserDataResult {
  // Implementation
}
```

### Utility Functions

- Must be in `utils/` folder
- Must have corresponding `.test.ts` file
- Must be properly typed

---

## Widget Contribution Requirements

**Important:** Before contributing, read [CONTRIBUTING.md](../CONTRIBUTING.md) for the complete contribution guide and workflow.

When contributing a widget, you MUST include:

### 1. Backend (C#)
- Widget class in `Ivy/Widgets/` inheriting from `WidgetBase<T>`
- Comprehensive XML documentation
- `[Prop]` attributes on all public properties
- Constructor overloads

### 2. Frontend (React/TypeScript)
- React component in `frontend/src/widgets/`
- TypeScript props interface
- Only Tailwind CSS and shadcn/ui for styling
- Accessibility support (ARIA attributes)
- Responsive design

### 3. Testing
- C# unit tests in `Ivy.Test/`
- Frontend unit tests (Vitest) with `.test.ts` extension
- E2E tests (Playwright) in `frontend/e2e/`
- Edge case testing (null, empty, invalid input)

### 4. Documentation
- XML documentation on C# widget
- Code examples showing usage
- Screenshots of different states (normal, hover, disabled, error)
- Light and dark mode examples

### 5. Dependencies
- **No new npm packages without approval**
- Stick to shadcn/ui and existing dependencies
- Tailwind CSS only (no custom CSS files)

---

## Testing Standards

### Backend Testing (C# / xUnit)

```csharp
[Fact]
public void GetUser_WithValidId_ReturnsUser()
{
    // Arrange
    var service = new UserService();

    // Act
    var result = service.GetUser("123");

    // Assert
    Assert.NotNull(result);
}

[Theory]
[InlineData(null)]
[InlineData("")]
public void GetUser_WithInvalidId_ThrowsException(string userId)
{
    Assert.Throws<ArgumentException>(() => service.GetUser(userId));
}
```

### Frontend Testing (Vitest)

```typescript
describe('UserCard', () => {
  it('renders user information', () => {
    render(<UserCard userId="1" name="John Doe" />);
    expect(screen.getByText('John Doe')).toBeInTheDocument();
  });

  it('calls onEdit when clicked', () => {
    const handleEdit = vi.fn();
    render(<UserCard userId="1" name="John" onEdit={handleEdit} />);
    fireEvent.click(screen.getByRole('button'));
    expect(handleEdit).toHaveBeenCalledOnce();
=======
**File Organization:**
```
src/components/UserCard/
├── UserCard.tsx
├── UserCardContext.tsx
├── utils/*.ts + *.test.ts
└── hooks/use*.ts + *.test.ts
```

**Hooks:**
- Put custom hooks in `./hooks/*` with `use` prefix
- Refactor when component has 3+ useEffect hooks
- Always type hook return values

## Testing

**Backend (xUnit):**
```csharp
[Fact]
public void GetUser_WithValidId_ReturnsUser() { /* Arrange, Act, Assert */ }

[Theory]
[InlineData(null)]
public void GetUser_WithInvalidId_ThrowsException(string userId) { }
```

**Frontend (Vitest):**
```typescript
describe('UserCard', () => {
  it('renders user information', () => {
    render(<UserCard userId="1" name="John" />);
    expect(screen.getByText('John')).toBeInTheDocument();
>>>>>>> ivy-fork/main
  });
});
```

<<<<<<< HEAD
### E2E Testing (Playwright)

```typescript
test('user can create new item', async ({ page }) => {
  await page.goto('/items');
  await page.click('button[aria-label="Add Item"]');
  await page.fill('input[name="title"]', 'New Item');
  await page.click('button[type="submit"]');
=======
**E2E (Playwright):**
```typescript
test('user can create item', async ({ page }) => {
  await page.goto('/items');
  await page.click('button[aria-label="Add Item"]');
>>>>>>> ivy-fork/main
  await expect(page.locator('text=New Item')).toBeVisible();
});
```

<<<<<<< HEAD
### Running Tests

```bash
# Backend tests
dotnet test

# Frontend unit tests
cd frontend && npm run test

# E2E tests (ALWAYS use npm scripts, not npx)
npm run e2e           # All E2E tests
npm run e2e:docs      # Ivy.Docs tests only
npm run e2e:samples   # Ivy.Samples tests only
```

---

## Code Review Guidelines

### Review Checklist

**Functionality:**
- [ ] Code does what it's supposed to do
- [ ] Logic is correct and handles edge cases
- [ ] Error handling is appropriate

**Quality:**
- [ ] Code is readable and maintainable
- [ ] Functions are small and focused
- [ ] No code duplication (DRY)

**Testing:**
- [ ] Unit tests for new functionality
- [ ] Tests cover happy path and edge cases
- [ ] All tests pass

**Documentation:**
- [ ] XML docs on C# public APIs
- [ ] Comments for complex logic
- [ ] README updates if needed

**Style:**
- [ ] C#: XML docs, `dotnet format` run, no warnings
- [ ] TypeScript: No `any` types, proper naming, lint/format run
- [ ] Follows project conventions

**Security:**
- [ ] No security vulnerabilities
- [ ] Input validation present
- [ ] No hardcoded secrets

### Review Comment Format

```markdown
**🟠 Important**: Missing error handling

The API call doesn't handle network errors.

**Suggestion:**
\`\`\`typescript
try {
  const response = await fetch(\`/api/users/\${userId}\`);
  if (!response.ok) throw new Error('Failed to fetch');
  return await response.json();
} catch (error) {
  console.error('Error:', error);
  throw error;
}
\`\`\`
```

### Severity Levels

- 🔴 **Critical**: Security, crashes, breaking changes (MUST FIX)
- 🟠 **Important**: Code quality, missing tests, performance (SHOULD FIX)
- 🟡 **Minor**: Style, optimizations, docs (NICE TO HAVE)
- 💡 **Suggestion**: Alternative approaches, future improvements (OPTIONAL)

---

## Code Quality Checklist

### Before Any Commit

**C# Code:**
- [ ] Run `dotnet format`
- [ ] Run `dotnet test` - all pass
- [ ] No compiler warnings
- [ ] XML documentation on public APIs
- [ ] Meaningful variable/method names

**TypeScript Code:**
- [ ] Run `npm run lint:fix` (in frontend/)
- [ ] Run `npm run format` (in frontend/)
- [ ] Run `npm run test` - all pass
- [ ] No TypeScript errors
- [ ] No `any` types
- [ ] Proper file organization (utils/, hooks/)

**E2E Tests:**
- [ ] Run `npm run e2e` - all pass

---

## Package Dependencies

### Approved Frontend Libraries

- ✅ shadcn/ui (Primary UI components)
- ✅ Tailwind CSS (Styling)
- ✅ Radix UI (Primitives via shadcn/ui)
- ✅ Lucide React (Icons - via Icon component)
- ✅ React Hook Form (Forms)
- ✅ Zod (Validation)

### Policy

- ❌ **NO new npm packages without explicit approval**
- ❌ **NO custom CSS files or alternative UI libraries**
- ✅ **Discuss in an issue before adding any dependency**
- ✅ **Justify necessity and consider bundle size**

---

## Common Issues to Avoid

### C# Issues

❌ Missing XML documentation
❌ Not using async/await properly
❌ Poor exception handling
❌ String concatenation instead of interpolation
❌ Not disposing IDisposable resources

### TypeScript Issues

❌ Using `any` type
❌ Hardcoded colors instead of CSS variables
❌ Direct icon imports instead of Icon component
❌ Missing dependency arrays in useEffect
❌ Not cleaning up useEffect subscriptions

### React Issues

❌ Too many responsibilities in one component
❌ Not memoizing expensive calculations
❌ Prop drilling instead of context
❌ Re-rendering unnecessarily
❌ Missing error boundaries

---

## Pre-commit Hooks

The project uses pre-commit hooks that automatically:
- Run linting and formatting on staged files
- Ensure code quality before commits

**Setup:**
```bash
cd frontend
npm install  # Sets up all pre-commit hooks
```

---

## Reference Links

- [Microsoft C# Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- [TypeScript Style Guide](https://github.com/microsoft/TypeScript/wiki/Coding-guidelines)
- [React Best Practices](https://react.dev/)
- [CONTRIBUTING.md](../CONTRIBUTING.md) - Full contribution guide
- [Ivy Discord](https://discord.gg/sSwGzZAYb6)
=======
**Run Tests:**
```bash
dotnet test                    # Backend
cd frontend && npm run test    # Frontend
npm run e2e                    # E2E (use npm scripts, not npx)
```

## Before Commit Checklist

**C#:** `dotnet format`, `dotnet test`, no warnings, XML docs on public APIs
**TypeScript:** `npm run lint:fix`, `npm run format`, `npm run test`, no `any` types
**E2E:** `npm run e2e` passes

## Approved Dependencies

shadcn/ui, Tailwind CSS, Radix UI, Lucide React, React Hook Form, Zod

**Policy:** ❌ NO new npm packages without approval. Discuss in issue first.

## Common Mistakes

**C#:** Missing XML docs, improper async/await, poor error handling, string concatenation, not disposing IDisposable
**TypeScript:** Using `any`, hardcoded colors, direct icon imports, missing useEffect dependencies, not cleaning up subscriptions
**React:** Too many component responsibilities, not memoizing expensive calculations, prop drilling, unnecessary re-renders, missing error boundaries

## Resources

See [CONTRIBUTING.md](../CONTRIBUTING.md) for full contribution guide.
>>>>>>> ivy-fork/main
