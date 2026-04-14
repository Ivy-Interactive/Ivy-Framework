---
icon: FolderInput
searchHints:
  - openclaw
  - inbox
  - folder
  - file watcher
  - drop folder
---

# OpenClaw

<Ingress>
Integrate OpenClaw or any file-based tool with Tendril by dropping markdown files into the Inbox folder.
</Ingress>

## Overview

Tendril watches an **Inbox folder** for new markdown files and automatically converts them into plans. This provides a simple, file-based integration point for any tool that can write files to disk.

## Inbox Folder Location

```
$TENDRIL_HOME/Inbox/
```

The `InboxWatcherService` monitors this directory for new `.md` files.

## File Format

Drop a markdown file (`.md`) with optional YAML frontmatter:

```markdown
---
project: ProjectName
sourcePath: optional/path/to/code
---

Describe the plan here. This text becomes the plan description
and is passed to the MakePlan promptware.
```

| Field | Required | Description |
|-------|----------|-------------|
| `project` | No | Target project name (defaults to `Auto`) |
| `sourcePath` | No | Path hint for related source code |

The content after the frontmatter becomes the plan description.

<Callout type="info">
If you omit the frontmatter entirely, the entire file content is used as the plan description with default settings.
</Callout>

## File Lifecycle

1. **Drop** a `.md` file into the Inbox folder
2. **Processing** — The file is renamed to `.md.processing` while being handled
3. **Completion** — The file is deleted once the plan is created successfully

## Recovery

If Tendril restarts during processing, any `.md.processing` files are automatically recovered back to `.md` and reprocessed on startup.

## Setting Up with OpenClaw

Configure OpenClaw to write its output as markdown files to the Tendril Inbox folder. Each file becomes a separate plan:

1. Set the output directory to `$TENDRIL_HOME/Inbox/`
2. Use markdown format with YAML frontmatter for project targeting
3. Tendril picks up new files automatically — no polling or API calls needed
