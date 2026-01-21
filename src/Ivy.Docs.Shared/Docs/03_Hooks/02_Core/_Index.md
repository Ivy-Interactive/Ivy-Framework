---
title: Hooks
---

# Hooks

Hooks are functions that let you "hook into" Ivy state and lifecycle features from functional components. They allow you to use state and other features without writing a class.

## Core Hooks

- [UseState](./03_UseState.md): Add local state to your components.
- [UseEffect](./04_UseEffect.md): Perform side effects in your components.

## Performance Hooks

- [UseMemo](./05_UseMemo.md): Memoize expensive calculations.
- [UseCallback](./06_UseCallback.md): Memoize callback functions.

## Other Hooks

- [UseRef](./08_UseRef.md): Store mutable values.

## Creating Custom Hooks

You can build your own hooks to reuse stateful logic between components. A custom hook is a function whose name starts with "Use" and that may call other hooks.
