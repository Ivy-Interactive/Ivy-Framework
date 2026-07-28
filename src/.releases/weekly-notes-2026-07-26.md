# Ivy Framework Release Notes - Version 1.3.14 (2026-07-26)

> [!NOTE]
> We usually release on Fridays every week. Sign up on [https://ivy.app/](https://ivy.app/auth/sign-up) to get release notes directly to your inbox.

This release fixes a platform issue where `ivy update` failed with `DllNotFoundException for Photino.Native` on macOS, and updates dependency security overrides.

## Bug Fixes & Improvements

### CLI & Updater
- **Fixed `ivy update` Native Library Packaging on macOS & Linux**: Resolved an issue where `Ivy.Updater` failed with `DllNotFoundException for Photino.Native` on macOS. The release workflow packaging step now includes native library files (`*.dylib` on macOS and `*.so` on Linux) inside the embedded updater `.zip` archives.
- **Linux Photino Native Library Target**: Added MSBuild target `CopyPhotinoNativeLinux` to ensure `Photino.Native.so` is copied to output and publish roots when building or publishing for Linux.

### Security
- **Security Updates**: Updated direct package dependencies and overrides to resolve package vulnerabilities.
