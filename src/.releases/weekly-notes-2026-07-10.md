# Ivy Framework Weekly Notes - Week of 2026-07-10

> [!NOTE]
> We usually release on Fridays every week. Sign up on [https://ivy.app/](https://ivy.app/auth/sign-up) to get release notes directly to your inbox.

This release introduces native "About App" window metadata APIs for desktop applications powered by Rustino.NET 0.3.8.

## New Features

### Desktop
- **Desktop About App API**: Added new builder methods (`AboutName`, `AboutVersion`, `AboutCopyright`, `AboutWebsite`, `AboutLicense`, `AboutAuthor`, `AboutComments`) to `DesktopWindow`. This allows developers to easily configure native system About dialog metadata for desktop applications.

  ```csharp
  app.UseDesktop(window => window
      .Title("My Application")
      .AboutName("My Application")
      .AboutVersion("1.0.0")
      .AboutCopyright("Copyright © 2026")
      .AboutWebsite("https://ivy.app")
      .AboutLicense("MIT")
      .AboutAuthor("Ivy Team")
      .AboutComments("Built with the Ivy Framework"));
  ```

- **Rustino.NET Upgrade**: Updated `Rustino.NET` dependency to version `0.3.8` to support native desktop window metadata integration.
