---
searchHints:
  - args
  - useargs
  - parameters
  - route-parameters
  - navigation-args
  - component-args
---

# Args

<Ingress>
The `UseArgs` [hook](../02_RulesOfHooks.md) provides access to arguments passed to a [component](../../../01_Onboarding/02_Concepts/02_Views.md), such as route parameters or navigation arguments.
</Ingress>

## Overview

The `UseArgs` [hook](../02_RulesOfHooks.md) allows you to access component arguments:

- **Route Parameters** - Access parameters from the current route
- **Navigation Arguments** - Retrieve arguments passed during navigation
- **Type Safety** - Strongly typed argument access
- **Optional Arguments** - Returns null if arguments are not available
