# Ivy Framework Weekly Notes - Week of 2026-06-29

> [!NOTE]
> We usually release on Fridays every week. Sign up on [https://ivy.app/](https://ivy.app/auth/sign-up) to get release notes directly to your inbox.

This release fixes a critical localhost connection error in the desktop webview on macOS and Linux.

## Bug Fixes and Improvements

### Desktop & Webview

- **Fix localhost connection error on macOS/Linux**: Resolved a connection issue in the desktop webview on macOS and Linux by disabling TLS (HTTPS) by default for localhost on these platforms. TLS remains enabled by default on Windows.
