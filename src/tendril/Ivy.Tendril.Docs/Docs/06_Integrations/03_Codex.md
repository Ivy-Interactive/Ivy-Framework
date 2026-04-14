---
icon: Bot
searchHints:
  - codex
  - openai
  - gpt
  - coding agent
---

# Codex

<Ingress>
Codex is an alternative coding agent powered by OpenAI's GPT models.
</Ingress>

## Configuration

Set Codex as your coding agent in `config.yaml`:

```yaml
codingAgent: codex
```

Or select it in **Settings > General > Coding Agent**.

## Profiles

Tendril maps effort levels to Codex models:

| Profile | Model | Use Case |
|---------|-------|----------|
| `deep` | gpt-5.4 | Complex multi-file changes |
| `balanced` | gpt-5.4-mini | Standard plan execution |
| `quick` | gpt-5.3-codex | Simple fixes and small edits |

The profile is selected automatically based on the plan's complexity level.
