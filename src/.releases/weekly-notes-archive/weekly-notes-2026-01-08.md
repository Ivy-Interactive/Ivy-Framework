# Ivy Framework Weekly Notes - Week of 2026-01-08

> [!NOTE]
> We usually release on Fridays every week. Sign up on [https://ivy.app/](https://ivy.app/auth/sign-up) to get release notes directly to your inbox.

## Breaking Changes

> List of all breaking changes and instructions for LLMS how to apply them can be found [here](https://github.com/Ivy-Interactive/Ivy-Framework/tree/main/src/.releases/Refactors/1.2.7)

### UseBuilder Renamed to UseWebApplicationBuilder

The `Server.UseBuilder()` method has been renamed to `UseWebApplicationBuilder()` for better clarity and to distinguish it from the new `UseWebApplication()` method. [Read more here](https://docs.ivy.app/onboarding/concepts/program)

### NavigationPurpose Renamed to HistoryOp

- `NavigationPurpose` - `HistoryOp`
- `NavigationPurpose.NewDestination` - `HistoryOp.Push`
- `NavigationPurpose.HistoryTraversal` - `HistoryOp.Pop`

### Audio Widget Renamed to AudioPlayer

The `Audio` widget has been renamed to `AudioPlayer` for better clarity and consistency. It can be visible in the AudioPlayer [doc page](https://docs.ivy.app/widgets/primitives/audio-player)

<img width="782" height="100" alt="Screenshot 2026-01-10 at 04 48 14" src="https://github.com/user-attachments/assets/271105c4-124e-4f69-9287-e1b1c9f3904c" />

### Icons Enum Updated to Match Lucide React 0.562.0

The `Icons` enum [has been updated](https://github.com/Ivy-Interactive/Ivy-Framework/pull/1912) to align with the latest version of lucide-react (0.562.0).

- 10 icons were renamed (e.g., AlignCenter - TextAlignCenter, Chrome - Chromium)
- 16 icons were removed (mostly *2 variants like FileCheck2, plus Text, FileJson, LetterText)
More info on how to use Icons can be found [here](https://docs.ivy.app/widgets/primitives/icon)

## Improvements

### ReadOnlyInput Simplified Constructor

The `ReadOnlyInput` widget now includes a non-generic constructor for string values. [For more info visit full doc page](https://docs.ivy.app/widgets/inputs/read-only)

```csharp
var readOnly = new ReadOnlyInput("User ID: 12345");
```

<img width="786" height="94" alt="Screenshot 2026-01-10 at 04 48 37" src="https://github.com/user-attachments/assets/f0787a55-57ef-44c8-bea3-1eb819511ff4" />

### LLM-Friendly Documentation

[Ivy.Docs](https://docs.ivy.app/onboarding/getting-started/introduction) had gotten a lot of LLM-friendly updates. `/robots.txt` and `/sitemap.xml` are now provided at the root for bots

### Enhanced Calendar Navigation

Calendar widgets (`DateInput`, `DateTimeInput`, `DateRangeInput`) now include dropdown selectors for month and year. Instead of clicking through months one-by-one, you can now:

- **Select year from dropdown** - Choose any year from 1900 to 2100 directly
- **Select month from dropdown** - Jump to any month instantly
- **Improved date visibility** - Fixed an issue where some dates could appear invisible in certain scenarios

More info on the [Date Time input here](https://docs.ivy.app/widgets/inputs/date-time) and on [Date Range input here](https://docs.ivy.app/widgets/inputs/date-range)

<img width="663" height="315" alt="Screenshot 2026-01-10 at 04 49 30" src="https://github.com/user-attachments/assets/d4817c57-9f52-4381-8346-afdc05d70412" />

### Font Loading Performance

Font flickering during page load has been eliminated by migrating to the Ivy Design System package.

### Automatic Color Input Validation

The `ColorInput` widget now automatically validates color values and displays an error state when invalid formats are entered.

- **Hex colors** - Must match valid formats: `#RGB`, `#RRGGBB`, or `#RRGGBBAA`
- **Color enums** - Must be a valid value from the `Colors` enum
- **Invalid entries** - Automatically marked with "Invalid color format" error message

<img width="550" height="236" alt="Screenshot 2026-01-10 at 16 24 03" src="https://github.com/user-attachments/assets/4306ca18-9627-41f5-a6c0-3449bfeaa787" />

### Widget Serialization Optimization

Added `AlwaysSerialize` property to `Prop` attribute to bypass default value optimization for specific properties. This is used by default for values in `Select` input.

### Scale API inheritance in widgets

Widgets now inherit scale settings from their parent widgets, ensuring consistent sizing throughout nested component hierarchies. When you set a scale on a parent widget, all children automatically inherit that scale unless explicitly overridden.

<img width="1200" height="588" alt="Screenshot 2026-01-10 at 16 42 08" src="https://github.com/user-attachments/assets/3b084b7b-dbd8-42c7-b93c-80ecbb742cc5" />

### Terminal Widget Copy Button

The `Terminal` widget now includes a convenient copy-to-clipboard button that automatically extracts and copies only command lines from the terminal display.

<img width="780" height="96" alt="Screenshot 2026-01-10 at 04 51 43" src="https://github.com/user-attachments/assets/a1e917fe-19c2-404e-a639-c3bdd34b9da8" />

### Code Snippet Copy Button Enhancement

<img width="756" height="107" alt="Screenshot 2026-01-10 at 04 56 59" src="https://github.com/user-attachments/assets/1d31e0c6-97d8-4c6c-acc2-72219337e010" />

## Bug Fixes

- Fixed Page Padding in Non-Chrome Applications
- Improved Tooltip Text Handling
- Fixed bugs related with Root Widget Replacement
- Implemented Authentication Logout When User Info Unavailable
- Fixed OAuth Popup Blocking in Safari
- Chart Configuration Consistency
- GrpcDataTableService is now loaded lazily
- Fixed incorrent display of x-axis labels in some charts
- When applying a new theme, if some variables were not defined in the new theme, defaults are now applied.

### Clerk Authentication Provider

A new authentication provider for Clerk (<https://clerk.com>) has been added to the framework, allowing you to leverage Clerk's complete user management platform in your Ivy applications.

```bash
dotnet add package Ivy.Auth.Clerk
```

1. Create a Clerk application at [clerk.com](https://clerk.com)
2. Configure your Clerk keys using .NET user secrets (development) or environment variables (production):

```terminal
dotnet user-secrets set "Clerk:SecretKey" "your_secret_key"
dotnet user-secrets set "Clerk:PublishableKey" "your_publishable_key"
```

```csharp
var server = new Server();

var authProvider = new ClerkAuthProvider()
    .UseEmailPassword()
    .UseGoogle()
    .UseGithub()
    .UseMicrosoft();

server.UseAuth(authProvider);

await server.RunAsync();
```

Read more on [Clerk Auth Provider here](https://docs.ivy.app/onboarding/cli/authentication/clerk)

### GitHub OAuth Authentication Provider

A new authentication provider for GitHub OAuth 2.0 has been added to the framework, allowing users to sign in to your Ivy applications using their GitHub accounts.

```bash
dotnet add package Ivy.Auth.GitHub
```

```csharp
var server = new Server();

// Register HttpClient for GitHub API
server.Services.AddHttpClient("GitHubAuth", client =>
{
    client.DefaultRequestHeaders.Add("User-Agent", "Ivy-Framework");
    client.DefaultRequestHeaders.Add("Accept", "application/vnd.github+json");
    client.DefaultRequestHeaders.Add("X-GitHub-Api-Version", "2022-11-28");
});
server.Services.AddSingleton(server.Configuration);
server.UseAuth<GitHubAuthProvider>(c => c.UseGitHub());
await server.RunAsync();
```

Configure your GitHub OAuth App credentials using .NET user secrets (development) or environment variables (production):

```terminal
dotnet user-secrets set "GitHub:ClientId" "your_client_id"
dotnet user-secrets set "GitHub:ClientSecret" "your_client_secret"
dotnet user-secrets set "GitHub:RedirectUri" "https://localhost:5010/ivy/webhook"
```

### Dynamic Page Titles

The framework now automatically updates the browser page title to reflect your current application route.

When you define an `AppDescriptor`, the framework automatically uses its `Title` property to set the browser page title:

```csharp
yield return new AppDescriptor(
    Id: "dashboard",
    Title: "Dashboard",  // This becomes the browser page title
    Component: typeof(DashboardView),
    MenuItems: [/* ... */]
)
```

<img width="261" height="41" alt="Screenshot 2026-01-10 at 16 33 32" src="https://github.com/user-attachments/assets/fb59a13b-fe5b-445c-a921-206c9c1e494f" />

## What's Changed

- [PieChart]: disable magicType in toolbox by @defymecobra in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/1906>
- [frontend]: fix font flickering using ivy-design-system fonts by @defymecobra in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/1901>
- [Icons]: update Icons enum to match lucide-react last version by @defymecobra in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/1912>
- [frontend]: correct font preload paths to prevent flickering by @defymecobra in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/1914>
- [Docs]: fix misalignment in demo elements by @defymecobra in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/1915>
- [Icon]: update icon color from Muted to Neutral in CardExtensions by @ArtemLazarchuk in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/1916>
- Refactor widgets and core, enhance chart features, improve diff logic by @nielsbosma in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/1919>
- [widgetRenderer]: add scale inheritance for nested widgets by @defymecobra in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/1913>
- [AsyncSelectInput]: fix bad styling by @defymecobra in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/1918>
- [Datatables]: lazy initialize GrpcTableService to avoid overhead for apps without datatables by @defymecobra in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/1926>
- (tooltip): fix word breaks and limit max height by @ArtemKhvorostianyi in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/1929>
- (DateTimeInput): split FE into modal structure by @ArtemKhvorostianyi in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/1928>
- [Sheet]: remove focus outline on sheet container by @defymecobra in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/1923>
- (DatabaseGenerator): delete initial migration after applying by @zachwolfe in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/1930>
- fix(core): full replacement of root node by @zachwolfe in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/1931>
- [Auth]: allow user to logout even without user info by @defymecobra in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/1927>
- fix(docs): Authentication Overview link by @zachwolfe in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/1940>
- (fix): unblock OAuth popups on Safari by @zachwolfe in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/1939>
- (codex): rename `NavigationPurpose` to `HistoryOp` by @zachwolfe in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/1934>
- [Widgets]: Add missing parameterless constructors and fix docs API defaults by @defymecobra in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/1922>
- (Docs): Sync docs with Release v1.2.6 by @ArtemKhvorostianyi in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/1924>
- [Docs]: fix bunch of issues by @defymecobra in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/1943>
- [VideoPlayer]: add example of large video file in docs page by @defymecobra in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/1937>
- [Docs]: add Layout page by @defymecobra in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/1936>
- (feat): dynamic page titles by @zachwolfe in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/1935>
- [auth]: Update and improve GitHub Auth Provider by @ArtemLazarchuk in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/1944>
- feat: make Spacer constructor public by @rorychatt in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/1950>
- (docs tocs):  display proper names for GitHub contributors by @ArtemKhvorostianyi in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/1952>
- [TerminalWidget]: add copy-to-clipboard button by @defymecobra in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/1945>
- [Auth]: add login form customization API by @defymecobra in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/1941>
- [Color-Input]: add automatic validation for invalid hex values by @defymecobra in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/1907>
- [Docs]: refactor to appear in teaching order by @defymecobra in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/1957>
- [Widgets]: add missing defaults by @defymecobra in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/1956>
- (theme): add popover to theme presets by @dcrjodle in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/1897>
- chore: add readmes to some of the pacakges by @rorychatt in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/1960>
- [Calendar]: make date clickable and fix view by @defymecobra in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/1959>
- [Docs]: refactor by @defymecobra in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/1953>
- (fix): Fixed sidebar arrow for nested grouped items by @KaiserReich95 in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/1963>
- (docs): update and refactor typography by @dcrjodle in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/1961>
- (fix): Removed cropping from code snippets and edited copy button by @KaiserReich95 in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/1932>
- (docs): implement llms.txt by @rorychatt in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/1639>
- [Serialization]: add AlwaysSerialize for input Value properties by @defymecobra in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/1971>
- chore: Update Grpc.AspNetCore.* by @zachwolfe in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/1978>
- chore: prepare release notes by @rorychatt in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/1970>

**Full Changelog**: <https://github.com/Ivy-Interactive/Ivy-Framework/compare/v1.2.6...v1.2.7>
