# Ivy Framework - Claude Code Configuration

## C# Conventions

**Core Rules:**
- Use `async`/`await` for async operations, `ValueTask` for hot paths
- Run `dotnet format` before committing
- Run `dotnet build` to verify - build errors are source of truth
- PascalCase for classes/methods/properties, _camelCase for private fields, camelCase for parameters/locals

**XML Documentation (Required for Public APIs):**
```csharp
/// <summary>Processes user input and validates against rules.</summary>
/// <param name="input">The user input to process.</param>
/// <param name="rules">The validation rules to apply.</param>
/// <returns>A validation result indicating success or failure.</returns>
public ValidationResult ProcessInput(string input, ValidationRules rules) { }
```

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
<Icon name="edit" size={16} />

// ❌ BAD
className="text-blue-500"
<ChevronRight size={20} />
```

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
  });
});
```

**E2E (Playwright):**
```typescript
test('user can create item', async ({ page }) => {
  await page.goto('/items');
  await page.click('button[aria-label="Add Item"]');
  await expect(page.locator('text=New Item')).toBeVisible();
});
```

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
