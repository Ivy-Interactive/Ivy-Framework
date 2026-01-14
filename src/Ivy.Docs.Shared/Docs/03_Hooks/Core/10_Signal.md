---
searchHints:
  - signal
  - usesignal
  - communication
  - messaging
  - pub-sub
  - events
  - reactive
  - broadcast
---

# Signal

<Ingress>
The `UseSignal` [hook](../02_RulesOfHooks.md) enables reactive communication between [components](../../../01_Onboarding/02_Concepts/02_Views.md) using a publish-subscribe pattern, allowing components to send and receive messages across your [application](../../../01_Onboarding/02_Concepts/15_Apps.md).
</Ingress>

## Overview

The `UseSignal` [hook](../02_RulesOfHooks.md) provides a way to implement reactive communication patterns in Ivy [applications](../../../01_Onboarding/02_Concepts/15_Apps.md):

- **Publish-Subscribe Pattern** - Components can send signals and receive them reactively
- **Type-Safe Communication** - Signals are strongly typed with input and output types
- **Broadcasting** - Signals can be broadcast across server, user, app, or chrome scopes
- **Decoupled Components** - Components can communicate without direct references
