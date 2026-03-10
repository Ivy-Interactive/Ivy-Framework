---
searchHints:
  - mcp
  - question
---

# ivy question

<Ingress>
Query the local context dynamically using integrated Local RAG features specifically tailored to your semantic `ivyVersion`.
</Ingress>

The `ivy question` command executes semantic queries across the comprehensive framework knowledge base. When asked "how" to do something or for code examples regarding Ivy internals, the underlying engine cross-references the latest indexed state of `Ivy.Docs.Shared`.

## Usage

```terminal
>ivy question <QUESTION>
```

### Arguments

- `<QUESTION>`: The natural language string prompt or instruction to run against the knowledge base. Wrap the query in double quotes.

## Examples

Ask a standard architectural question:

```terminal
>ivy question "How do I implement a new Application Shell in Ivy?"
```

Ask for specific CLI advice:

```terminal
>ivy question "What is the command to create an auto-incrementing migration in Ivy?"
```
