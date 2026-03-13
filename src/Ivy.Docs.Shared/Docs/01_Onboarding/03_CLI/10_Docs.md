---
searchHints:
  - docs
  - documentation
  - reference
  - content
  - list docs
---

# ivy docs

<Ingress>
Access and retrieve Ivy Framework documentation directly from your terminal.
</Ingress>

The `ivy docs` command set provides built-in tools for exploring the comprehensive framework knowledge base. You can either list all available documentation topics or fetch the raw Markdown content for a specific page.

## Commands

### ivy docs list

Lists all available documentation paths natively registered inside the Ivy framework for subsequent manual or automated investigation.

The command outputs a structured YAML representation of all discoverable document titles and relative paths. Use this list to find valid `<path>` arguments for the `ivy docs <path>` sibling command.

#### list Usage

```terminal
>ivy docs list
```

---

### ivy docs [path]

Retrieves the raw Markdown payload of a specific framework documentation page.

This command resolves and standardizes versioning logically, ensuring you always retrieve documentation relevant to the specific framework instantiation you have targeted.

#### path Usage

```terminal
>ivy docs [path]
```

#### Arguments

- `<path>`: The relative path or URL slug corresponding to the desired markdown file. You can discover valid paths via the `ivy docs list` command.

#### Example

```terminal
>ivy docs "docs/ApiReference/IvyShared/Colors.md"
```
