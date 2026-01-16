# Migration Guides for LLMs

Instructions for LLM assistants on how to use migration guides when refactoring code to a new Ivy Framework version.

## Directory Structure

```
Refactors/
├── {version}/           # e.g., 1.2.7, 1.2.11
│   ├── FeatureName.md   # Breaking change migration guide
│   └── optional/        # Recommended but non-critical migrations
└── README.md            # This file
```

## How to Apply Migrations

### 1. Identify Target Version

When migrating from version `A` to version `B`, apply all migrations where `A < version <= B`.

**Example:** Migrating from `1.2.6` to `1.2.11`:
```
Apply: 1.2.7/*.md → 1.2.11/*.md
Skip:  1.2.11/optional/*.md (optional)
```

### 2. Read Each Migration File

Each `.md` file contains some of the following sections:

| Section | Purpose |
|---------|---------|
| `## Summary` or `## Goal` | What changed and why |
| `## What Changed` or `## Before/After` | Code examples showing the change |
| `## How to Find Affected Code` or `## Locate Code` | Search patterns (regex, grep) |
| `## How to Refactor` or `## Required Changes` | Step-by-step instructions |
| `## Key Refactoring Rules` | Important rules to follow |
| `## Common Mistakes to Avoid` or `## Common Pitfalls` | Typical errors |
| `## Verification` | How to verify success |

### 3. Apply Refactoring

1. **Search** using provided patterns: `rg "Pattern" --type cs`
2. **Refactor** following Before/After examples
3. **Verify** with `dotnet build` after each file

### 4. Verify

```bash
dotnet build
dotnet test  # if tests exist
```

## Tips for LLMs

1. **Read the entire file** before making changes
2. **Start with `dotnet build`** — compiler errors often point directly to affected code
3. **Apply changes file by file**, running `dotnet build` after each
4. **Check `optional/` subfolder** after completing required migrations
5. **Use exact patterns** from the migration guide for search/replace
6. **Pass `CancellationToken`** to async methods when shown in examples
