---
icon: Bot
searchHints:
  - claude
  - claude code
  - anthropic
  - coding agent
  - ai agent
---

# Claude Code

<Ingress>
Claude Code is the default coding agent in Tendril, powered by Anthropic's Claude models.
</Ingress>

## Configuration

Set Claude Code as your coding agent in `config.yaml`:

```yaml
codingAgent: claude
```

Or select it in **Settings > General > Coding Agent**.

## Requirements

- The Claude CLI must be installed and available as `claude` on your PATH
- Run `claude` once to complete authentication before using Tendril

## Profiles

Tendril maps effort levels to Claude models:

| Profile | Model | Use Case |
|---------|-------|----------|
| `deep` | Opus | Complex multi-file changes, architecture work |
| `balanced` | Sonnet | Standard plan execution, most tasks |
| `quick` | Haiku | Simple fixes, formatting, small edits |

The profile is selected automatically based on the plan's complexity level, or can be configured per promptware in `config.yaml`.
