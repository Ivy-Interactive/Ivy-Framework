# Ivy Framework - Claude Code Configuration

## Overview

This file contains all coding conventions, standards, and guidelines for the Ivy Framework project. These rules are always active when working with Claude Code.

---

## C# Code Conventions

### Core Principles

1. Follow [Microsoft C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
2. Use meaningful variable and method names
3. Keep methods focused and concise
4. Use async/await for asynchronous operations
5. Always format code with `dotnet format`

### XML Documentation Requirements

**All Public APIs MUST Have:**
- `<summary>` tags describing the purpose
- `<param>` tags for each parameter
- `<returns>` tags for return values
- `<remarks>` tags for additional context when needed

```csharp
/// <summary>
/// Processes user input and validates against the specified rules.
/// </summary>
/// <param name="input">The user input to process.</param>
/// <param name="rules">The validation rules to apply.</param>
/// <returns>A validation result indicating success or failure.</returns>
public ValidationResult ProcessInput(string input, ValidationRules rules) { }
```

### Widget Development Standards

**Widget Class Requirements:**
- Inherit from `WidgetBase<T>`
- Use namespace: `namespace Ivy;`
- Include comprehensive XML documentation
- Use `[Prop]` attribute for all public properties with descriptions
- Provide constructor overloads for different use cases

```csharp
namespace Ivy;

/// <summary>
/// Displays a badge with customizable text and styling.
/// </summary>
public class BadgeWidget : WidgetBase<BadgeWidget>
{
    [Prop("The text content of the badge")]
    public string Text { get; set; } = string.Empty;

    public BadgeWidget() { }
    public BadgeWidget(string text) { Text = text; }
}
```

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
- Custom CSS files or styled-components
- Alternative UI libraries (Material-UI, Ant Design, etc.)
- Different icon libraries (FontAwesome, Heroicons, etc.)
- Hardcoded colors (`#3b82f6`, `rgb()`)

```typescript
// ✅ GOOD
className="text-[var(--color-primary)] bg-[var(--color-background)]"
<Icon name="edit" size={16} />

// ❌ BAD
className="text-blue-500"
<ChevronRight size={20} />
```

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

### React Hooks Guidelines

- Custom hooks must be in `hooks/` folder with `use` prefix
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
  });
});
```

### E2E Testing (Playwright)

```typescript
test('user can create new item', async ({ page }) => {
  await page.goto('/items');
  await page.click('button[aria-label="Add Item"]');
  await page.fill('input[name="title"]', 'New Item');
  await page.click('button[type="submit"]');
  await expect(page.locator('text=New Item')).toBeVisible();
});
```

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
