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
Interactive **claude-code** in an embedded terminal (Xterm) inside Tendril.
</Ingress>

## When to use

Background promptware handles structured work (`ExecutePlan`, `MakePlan`). **Open Claude** is for ad hoc exploration in a real PTY (stdin/stdout to the CLI).

## Notes

- Same working directory and credentials as your normal terminal context.
- Copy/paste works as expected.
- Closing the window does not necessarily wipe the session—similar to an IDE terminal panel.
