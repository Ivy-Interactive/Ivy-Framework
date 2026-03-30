# Ivy CLI Skills

This document teaches AI agents and developers how to use the Ivy CLI to access framework documentation and get contextual answers. These commands complement [AGENTS.md](../../../AGENTS.md) for deeper exploration of the framework.

## ivy docs

Access and retrieve Ivy Framework documentation directly from the terminal.

### ivy docs list

Lists all available documentation paths as structured YAML output. Use this to discover valid paths for `ivy docs <path>`.

```bash
ivy docs list
```

### ivy docs \<path\>

Retrieves the raw Markdown content for a specific documentation page.

```bash
ivy docs "docs/ApiReference/IvyShared/Colors.md"
```

Paths can be discovered via `ivy docs list`. You can also convert any `docs.ivy.app` URL to a raw Markdown path by appending `.md` to the URL slug.

## ivy question / ivy ask

Semantic search against the framework knowledge base using Local RAG. Takes a natural language question in double quotes and returns contextual answers cross-referencing `Ivy.Docs.Shared`.

```bash
ivy ask "How do I implement a new Application Shell in Ivy?"
```

```bash
ivy question "What is the command to create an auto-incrementing migration in Ivy?"
```

## When to use which

| Scenario | Command |
|---|---|
| You know the topic and want the full reference page | `ivy docs <path>` |
| You need to browse what documentation exists | `ivy docs list` |
| You have a "how do I..." question and need a synthesized answer | `ivy ask "your question"` |

Use `ivy docs` for targeted lookups when you know what you're looking for. Use `ivy ask` when you need the framework to synthesize an answer from across the knowledge base.
