---
searchHints:
  - service
  - useservice
  - dependency-injection
  - di
  - ioc
  - services
  - dependency
---

# Service

<Ingress>
The `UseService` [hook](../02_RulesOfHooks.md) provides access to registered services from the dependency injection container, enabling clean separation of concerns and testable [components](../../../01_Onboarding/02_Concepts/02_Views.md).
</Ingress>

## Overview

The `UseService` [hook](../02_RulesOfHooks.md) allows you to access services registered in your Ivy [application](../../../01_Onboarding/02_Concepts/15_Apps.md):

- **Dependency Injection** - Access registered services from the DI container
- **Service Resolution** - Automatically resolves service dependencies
- **Type Safety** - Strongly typed service access
- **Lifecycle Management** - Services follow their configured lifecycle (singleton, scoped, transient)
