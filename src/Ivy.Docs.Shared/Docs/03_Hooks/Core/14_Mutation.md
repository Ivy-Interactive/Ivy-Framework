---
searchHints:
  - mutation
  - usemutation
  - query-mutation
  - data-mutation
  - update
  - invalidate
---

# Mutation

<Ingress>
The `UseMutation` [hook](../02_RulesOfHooks.md) provides a way to perform data mutations and invalidate [query](./09_Query.md) caches, enabling optimistic updates and cache management in your [application](../../../01_Onboarding/02_Concepts/15_Apps.md).
</Ingress>

## Overview

The `UseMutation` [hook](../02_RulesOfHooks.md) enables data mutations with cache management:

- **Data Mutations** - Perform create, update, and delete operations
- **Cache Invalidation** - Automatically invalidate related [query](./09_Query.md) caches
- **Optimistic Updates** - Update UI optimistically before server confirmation
- **Error Handling** - Built-in error handling and rollback support
