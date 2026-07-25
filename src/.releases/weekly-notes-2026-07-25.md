# Ivy Framework Weekly Notes - Week of 2026-07-25

> [!NOTE]
> We usually release on Fridays every week. Sign up on [https://ivy.app/](https://ivy.app/auth/sign-up) to get release notes directly to your inbox.

This release makes `AppRepository` thread-safe to prevent duplicate or missing apps in the sidebar during concurrent plugin loads, and improves reporting for missing or nonexistent referenced plugin directories.

## Bug Fixes & Improvements

### Core & Server
- **Thread-Safe AppRepository**: Made `AppRepository` thread-safe by publishing an immutable tree `Snapshot` via `Volatile.Write` on reload. This prevents concurrent readers from observing half-rebuilt menu trees.
- **Serialized App Reloading**: Serialized `AppRepository.Reload` execution end-to-end to prevent race conditions during deferred plugin loading where slow background reloads could overwrite newer navigation states and cause duplicate apps in the sidebar.
- **Nonexistent Referenced Plugin Handling**: Updated plugin discovery to record nonexistent referenced plugin paths as failed plugin candidates rather than silently filtering them out or throwing exceptions. Missing plugin directories now cleanly appear as unloaded/failed in the UI.
