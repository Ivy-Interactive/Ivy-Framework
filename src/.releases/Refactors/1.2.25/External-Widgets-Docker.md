# External Widgets & Dockerfile Refactors

## External Widgets Frontend

- The `ivy widget init` CLI now generates external widgets with an embedded `frontend/` directory using Vite+ (`vp`).
- The `package.json`, `vite.config.ts`, and UMD rollup window bindings have been updated to align with the canonical `vite-plus` toolchain, ensuring seamless integration of customized frontend components into Ivy's interactive architecture.
- Instead of using `node:24-alpine`, we are temporarily reverting Dockerfile generation templates to `node:24-bookworm-slim` due to upstream Vite+ issues with `musl` libc compatibility on Alpine Linux (tracked in [voidzero-dev/vite-plus#992](https://github.com/voidzero-dev/vite-plus/issues/992)).

## Dockerfile CLI Generation

- The CLI deployment templates (`Dockerfile.tpl`) have been updated. If your project contains a frontend, it will now generate a `node:24-bookworm-slim` build stage that installs `curl` and `bash` and runs the `vite.plus` install script before building the `.NET` application.
- Tracking issues have been opened to monitor the migration back to Alpine once it's officially supported by VoidZero.
