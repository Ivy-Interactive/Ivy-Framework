---
searchHints:
  - file
  - viewer
  - images
  - code snippet
icon: File
---

# File Viewer

<Ingress>
The native File App enables you to rapidly parse local assets, configuration snippets, and generated code logs without leaving the Tendril orchestration window.
</Ingress>

## Integrated Asset Support

During the `Review` phase or inside `Job` logs, you will commonly encounter references to specific physical paths. Clicking these references routes them to the File App dynamically.

Features include:

- **Syntax Highlighted Code**: Full code visualization mappings for `.cs`, `.js`, `.py`, `.sql`, `.tsx`, and a dozen other extensions, providing rich colorized readability.
- **Image Previews**: Native parsing for generated UI visual snapshots (`.png`, `.svg`, `.webp`, `.jpg`) without defaulting to system application popups.
- **Error Transparency**: Hard failures when parsing local state are trapped and displayed logically to identify corrupted execution outputs.

Because the component maps paths exclusively via absolute local system routes managed by `TENDRIL_HOME`, files stay securely air-gapped on your computer.
