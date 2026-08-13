# Ivy Framework Release Notes - Version 1.3.20 (2026-08-13)

> [!NOTE]
> We usually release on Fridays every week. Sign up on [https://ivy.app/](https://ivy.app/auth/sign-up) to get release notes directly to your inbox.

This release fixes macOS security password prompts and HTTPS connection issues in desktop applications, adds IvyML Studio, XamlBuilder syntax sugar, non-generic default input records, and CodeInput Ghost variant with XML support.

## New Features

### Desktop & Native Security
- **macOS Certificate Verification & Un-elevated Trust**: Resolved an issue where launching desktop applications on macOS triggered repeated macOS SecurityAgent admin password prompts ("You are making changes to the System Certificate Trust Settings"). `CertificateHelper` now queries native macOS trust via `security verify-cert -c`, recognizing installer-trusted certificates system-wide and using non-elevated user keychain trust without admin prompt elevation.

### IvyML & Studio
- **IvyML Studio App**: Introduced `Ivy.IvyML.Studio` for authoring IvyML markup with integrated chat, code, and live preview views.
- **IvyML Parse Command**: Added `parse` command to `ivyml` console for validating markup without rendering, and improved screenshot capture service to wait for widget tree rendering.
- **XamlBuilder Slot & Content Sugar**: Added support for dotted slot syntax (`<Card.Header>` mapping to `<Slot Name="Header">`) and inner-text shorthands for string content properties.
- **Non-Generic Default Input Records**: Added non-generic default records (`NumberInput`, `SelectInput`, `DateTimeInput`, etc.) so IvyML markup can reference widgets directly without type parameters.
- **CodeInput Ghost Variant & XML Support**: Added Ghost variant and XML syntax highlighting support for `CodeInput`.

## Bug Fixes & Improvements
- **macOS LocalMachine Certificate Store Check**: Added fallback checking for `StoreLocation.LocalMachine` when inspecting certificate trust.
