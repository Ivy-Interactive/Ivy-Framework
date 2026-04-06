# Ivy Playwright Test Knowledge Base — Index

This knowledge base has been split into focused files for easier navigation. Load only the files relevant to your current task.

## Quick Reference

| File | Description | When to Load |
|------|-------------|--------------|
| [PlaywrightKnowledge-Framework.md](PlaywrightKnowledge-Framework.md) | Ivy framework basics, app construction, hooks rules, test project setup, frontend build issues | Setting up a new test project or understanding Ivy app architecture |
| [PlaywrightKnowledge-Process.md](PlaywrightKnowledge-Process.md) | **(Critical)** Process cleanup, app lifecycle, stale process management, `test-utils.ts` patterns | Always — process cleanup is mandatory for all test runs |
| [PlaywrightKnowledge-Locators.md](PlaywrightKnowledge-Locators.md) | Locator strategies, text matching, role-based locators, Chrome navigation, network/navigation tips | Writing or debugging Playwright test assertions |
| [PlaywrightKnowledge-Widgets.md](PlaywrightKnowledge-Widgets.md) | Widget-specific patterns: DataTable, SelectInput, Dialog/Sheet, Form, DateInput, CodeInput, Card, ECharts, Upload, Markdown, and more | Testing specific Ivy widgets |
| [PlaywrightKnowledge-DOM.md](PlaywrightKnowledge-DOM.md) | Canvas rendering, virtualized grids, React fiber introspection, Radix UI patterns, CSS quirks, clipboard testing | Debugging DOM structure or visual rendering issues |
| [PlaywrightKnowledge-Gotchas.md](PlaywrightKnowledge-Gotchas.md) | **(Critical)** ValueTuple crash, Layout.Vertical IEnumerable bug, decimal columns, Html/Card invisibility, timer race conditions | Before writing any Ivy app code — avoid known crash patterns |
| [PlaywrightKnowledge-Testing.md](PlaywrightKnowledge-Testing.md) | Test setup, video recording, test structure, common categories, widget library testing, run history | Setting up tests or reviewing past verification results |

## Critical Files (Always Review)

1. **PlaywrightKnowledge-Process.md** — Process cleanup is the #1 cause of flaky tests and build failures
2. **PlaywrightKnowledge-Gotchas.md** — Crash patterns that will waste 15+ minutes if hit unknowingly

## File Categories

### For Test Authors
- Start with **Framework** (project setup) → **Process** (lifecycle) → **Locators** (assertions) → **Widgets** (specific patterns)

### For Debugging
- **DOM** for rendering issues → **Gotchas** for known crash patterns → **Testing** run history for similar past issues

### For New Widget Verification
- **Framework** (frontend build) → **Widgets** (specific widget) → **Locators** (assertion strategies) → **Testing** (test structure)
