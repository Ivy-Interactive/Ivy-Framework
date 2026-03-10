---
title: MCP Get Doc Content Command
searchHints:

- mcp
- docs content

# `ivy docs <path>`

<Ingress>
Retrieve the raw markdown payload of a specific framework documentation page through the MCP Context Pipeline.
</Ingress>

The `ivy docs <path>` command retrieves the raw Markdown text of any published Ivy documentation page. It resolves versioning logically, ensuring you always retrieve documentation relevant to the specific framework instantiation you have targeted.

It utilizes the `GetDocContentTool` from the Ivy MCP Server.

## Usage

```terminal
>ivy docs <DOC_PATH>
```

### Arguments

- `<DOC_PATH>`: The relative path or URL slug corresponding to the desired markdown file. You can discover valid paths via the `ivy docs list` command.

## Examples

Retrieve the documentation source for shared Colors:

```terminal
>ivy docs "docs/ApiReference/IvyShared/Colors.md"
```
