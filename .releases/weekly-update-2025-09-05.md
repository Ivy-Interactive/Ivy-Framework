# Ivy Framework Weekly Update - September 5, 2025

This week brought significant improvements to both the Ivy CLI tools and the Ivy Framework, with new features for developers and enhanced UI components. Here's what's new since last Friday.

## 🚀 CLI Tools Updates

### Template Selection System
The CLI now supports GitHub-based template selection for project initialization. When creating a new project, developers can choose from available templates hosted on GitHub repositories.

```bash
ivy init my-project --template hello
```

### Enhanced Database Generator
The database generator now includes options for database deletion and seeding during generation, making development workflows more flexible.

### Version Command Improvements
The `ivy version` command now displays more comprehensive information including user entitlements, OS details, and session logging capabilities.

### Git Check Override
A new `--ignore-git` option allows bypassing Git repository checks, useful when this feature is just in the way. 

```bash
ivy build --ignore-git
ivy debug --ignore-git
```

### Session Logging

Commands now support session IDs when running agentic features. This makes it very to zip up a folder and send to us for debugging. We will then have the history of the project.

Logs are stored in .ivy/sessions.ldjons

## 🎨 Framework Core Features

### Automatic Port Management

The server now includes a `FindAvailablePort` option that automatically finds an available port when the default is in use. This feature is enabled by default in DEBUG mode.

```csharp
var server = new Server(new ServerArgs
{
    Port = 5000,
    FindAvailablePort = true  // Will try ports 5001, 5002, etc. if 5000 is busy
});
```

### Theme Support System

A comprehensive theming system has been introduced with both light and dark theme support, including API refactoring for better developer experience.

```csharp
// New theme configuration
services.AddIvyTheme(config =>
{
    config.EnableDarkMode = true;
    config.DefaultTheme = "light";
});
```

## 📊 New UI Components

### Pagination Widget

A new pagination component provides flexible navigation for large datasets with customizable options.

```csharp
var pagination = new Pagination
{
    CurrentPage = 1,
    TotalPages = 10,
    OnPageChange = page => { /* Handle page change */ }
};
```

Our first extrenal contribution from @ViktorWb!

### Expandable Component

Improved expandable/collapsible sections with smooth animations for better content organization. Used extensively in the docs.

```csharp
var expandable = new Expandable
{
    Header = "Click to expand",
    Content = new Container { /* Your content */ },
    IsExpanded = false
};
```

## 🐛 Bug Fixes & Improvements

### File Upload Enhancement

Fixed chunking issues in file upload component, improving reliability for large file transfers.

### UI/UX Improvements

- Fixed code input selection visibility issues
- Resolved terminal copy-paste functionality on Windows
- Improved responsive layout handling in documentation
- Fixed extra padding in product view samples
- Corrected table layouts in widget integration examples

### Documentation Enhancements

- Added expandable sections with `<details>` tags for advanced examples
- Fixed heading hierarchy in multiple documentation pages
- Made icons clickable in search results
- Improved SVG examples and animations
- Enhanced authentication documentation structure

### Performance Optimizations

- Removed unnecessary loading animations for tab content
- Improved scroll behavior and reset handling
- Optimized horizontal scrolling in table of contents
- Better handling of contributor component rendering

## 🔐 Authentication Updates

### Basic Auth Improvements

Enhanced JWT token handling with proper issuer and audience configuration for the basic authentication provider. These values can now be set directly in the connection string for easier configuration.

```csharp
// Improved basic auth configuration
services.ConfigureBasicAuth(options =>
{
    options.JwtIssuer = "your-issuer";
    options.JwtAudience = "your-audience";
});

// Or via connection string
"Data Source=app.db;JWT_ISSUER=your-issuer;JWT_AUDIENCE=your-audience"
```

### Microsoft Entra Documentation

Comprehensive documentation for Microsoft Entra (formerly Azure AD) authentication provider has been completed, along with major updates to all other authentication provider documentation for consistency and clarity.

## 🛠️ Developer Experience

### Build System Enhancements

- Added prerelease version support in deployment workflows
- Improved error handling and logging across AppHub and Server
- Better argument parsing with case-insensitive key lookup
- Enhanced timeout handling (default now 120 seconds)

### Release Automation

New PowerShell scripts for automated release creation streamline the deployment process.

```powershell
./CreateRelease.ps1 -Version 1.0.99 -Prerelease
```

## 📝 Contributing Guidelines

The CONTRIBUTING.md file has been updated with clearer guidelines and better organization for new contributors.

Stay tuned for next week's updates as we continue improving the Ivy Framework!
