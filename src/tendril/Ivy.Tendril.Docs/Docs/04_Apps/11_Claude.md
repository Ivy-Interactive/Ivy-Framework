---
searchHints:
  - claude
  - terminal
  - xterm
  - pty
  - command line
icon: Terminal
---

# Embedded Claude

<Ingress>
The Claude App integrates a fully functional, headless instance of the Anthropic `claude-code` CLI, housed within a high-speed Xterm instance directly inside the Tendril Desktop.
</Ingress>

## Autonomous Execution

While Tendril primarily relies on defined background Promptwares for structural jobs (`ExecutePlan`, `MakePlan`), you frequently need isolated generative investigation tools.

Clicking **Open Claude** triggers a native Pseudo-terminal (PTY) binding mapping stdin/stdout streams directly to an underlying Claude executable. 

## Xterm Layout & Capabilities

- **Deep Context Pipeline**: It shares the working directory boundaries and credential architecture utilized by standard terminal environments.
- **Copy / Paste Integrity**: Native clipboard polling permissions enabled.
- **Session Continuity**: Discarding or collapsing the Tendril window preserves your underlying Claude conversational history efficiently, mimicking an IDE's terminal drawer architecture cleanly.
