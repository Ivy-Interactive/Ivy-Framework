# .NET Project Gotchas for Worktree Execution

## SDK-style project globbing
When adding a test project as a subdirectory of an SDK-style project (e.g. `FooTest.Tests/` under `FooTest/`), the main project will glob the test `.cs` files. Fix: add `<Compile Remove="TestProject\**" />` to the main `.csproj`.

## .NET 10 solution format
`dotnet new sln` creates `.slnx` files (not `.sln`) in .NET 10. Use `dotnet sln add` as normal.

## dotnet format ambiguity
When both a `.csproj` and `.slnx` exist in the same directory, `dotnet format` fails without an explicit workspace argument. Use `dotnet format <name>.slnx`.

## Worktree re-creation on Windows: stale files survive rm -rf
When re-executing a plan, `rm -rf` on the worktree directory may not fully delete all files on Windows (file locks, long paths, etc.). After `git worktree add` re-creates the directory, the working tree may contain modified files from the previous execution. Fix: always run `git checkout -- . && git clean -fd` after creating a worktree that may have had prior content.

## Ivy-Framework: vp CLI and solution path
Ivy-Framework's solution is at `src/Ivy-Framework.slnx` (not root). The `Ivy.csproj` has MSBuild targets that invoke `vp install` and `vp run build`. The `vp` CLI is a local npm dependency (`node_modules/.bin/vp`), NOT a global tool. In worktrees: run `npm install` in `src/frontend/` first, then add `src/frontend/node_modules/.bin` to PATH before running `dotnet build/test/format`. The `package-lock.json` created by `npm install` and any CRLF diffs in `vite.config.mjs` should be discarded (not committed).

## Cross-repo absolute ProjectReference in worktrees
When a repo (e.g. `Ivy-Tendril`) has absolute `ProjectReference` paths to another repo (e.g. `D:\Repos\_Ivy\Ivy-Framework\src\Ivy\Ivy.csproj`), and both repos have changes in their worktrees, the build will fail because the Tendril worktree references the **original** Framework (without the plan's changes). Fix: create a temporary `Directory.Build.targets` in the Tendril worktree that removes the original references and adds references to the Framework worktree. Delete this file after verification (do not commit it).

## IVYHOOK005: No non-hook statements between hooks in Build()
The Ivy analyzer's IVYHOOK005 rule requires all hook calls (`UseService`, `UseState`, `UseInterval`, `UseRefreshToken`, etc.) to be at the top of Build() before any other statements. Extracting a hook parameter to an intermediate variable (e.g. `var pollInterval = ...; UseInterval(..., pollInterval);`) counts as a non-hook statement and triggers IVYHOOK005. Fix: inline the expression directly into the hook call argument.

## Ivy Analyzer pragmas for AppShell copies
When copying `DefaultSidebarAppShell` from Ivy-Framework into a consuming project (e.g. Tendril), the Ivy analyzer (`Ivy.Analyser`) will report IVYAPP001 (parameterless constructor required) and IVYHOOK005 (hooks not at top of Build). The original DefaultSidebarAppShell avoids these because Framework doesn't reference its own analyzer. Fix: add `#pragma warning disable IVYAPP001` and `#pragma warning disable IVYHOOK005` at the top of the copied file (without restore, so it covers the whole file).

## QueryResult<T>.Value for value types
`QueryResult<int>.Value` is `int`, not `int?`. C# unconstrained generic `TValue?` resolves to the default value for value types, not `Nullable<TValue>`. Use `.Loading` to check if the query has data, not `!= null`.

## External widget pnpm + Node 22 compatibility
When creating new external widget frontends with `@voidzero-dev/vite-plus-core@0.1.13` and Node.js 22, pnpm's default isolated module resolution breaks the `#module-sync-enabled` imports map. Fix: add `node-linker=hoisted` to a `.npmrc` file in the widget's `frontend/` directory. Also externalize `react/jsx-runtime` in the vite config alongside `react` and `react-dom` (mapping it to the `React` global). If the widget imports CSS from a dependency (e.g. `react-diff-view/style/index.css`), add `cssCodeSplit: false` to vite config and set `StylePath` in the `[ExternalWidget]` attribute.

## Ivy widget API: Button.Ghost() and Layout.Gap()
`Button.Ghost()` takes no parameters (returns self with ghost styling). There is no `Ghost(bool)` overload — use ternary to conditionally apply it: `condition ? (object)new Button("X") : new Button("X").Ghost()`. `Layout.Gap()` takes an `int`, not `Size.Of(int)`. `AlignItems(Align)` may not exist on `LayoutView` — omit if compilation fails.

## Ivy Widget extension method shadowing by record properties
When an Ivy widget record has an `[Event]` property (e.g. `OnSave`) that is a `Func<Event<T,V>, ValueTask>?`, calling `.OnSave(arg)` with a single argument resolves to **delegate invocation** (the property), not the extension method. This happens because instance members shadow extension methods. Fix: call the extension method explicitly, e.g. `ScreenshotFeedbackExtensions.OnSave(widget, handler)`. The zero-arg overload (`.OnSave(() => ...)`) works fine because the delegate requires one parameter and doesn't match.

## C# record ToString() override must be sealed for base classes
When overriding `ToString()` on a base record (e.g. `AbstractWidget`), the override must be `sealed` — otherwise every derived record (e.g. `Badge : WidgetBase<Badge>`) generates its own `ToString()` that calls `PrintMembers()`, which accesses all properties including delegates. Use `public sealed override string ToString() => ...;` to prevent this.

## Ivy.Widgets.Xterm.Terminal vs Ivy.Terminal type ambiguity
`Ivy.Widgets.Xterm.Terminal` and `Ivy.Terminal` (primitive widget) share the same class name. In projects under the `Ivy.*` namespace (e.g. `Ivy.Tendril.Apps`), C# namespace lookup resolves `Terminal` to `Ivy.Terminal` first, breaking fluent extension method chaining from `Ivy.Widgets.Xterm.TerminalExtensions`. Fix: use fully-qualified static method calls (`Ivy.Widgets.Xterm.TerminalExtensions.Stream(terminal, ...)`) instead of fluent `.Stream(...)`. The `using Ivy.Widgets.Xterm;` import and type aliases don't resolve this because ancestor namespace types take precedence.

## CodeInput is generic — cannot instantiate directly
`CodeInput<TString>` requires a type parameter. You cannot do `new CodeInput(someString, ...)`. Instead use the `.ToCodeInput()` extension method on an `IState<string>`, or use a `Markdown` widget with a fenced code block as a simpler alternative when you just need read-only display.

## Ivy-Framework: Rust builds in worktrees (Ivy.Docs.Shared)
The `Ivy.Docs.Shared` project runs `cargo run` during build to compile a Rust CLI tool. In worktrees with long paths, `link.exe` fails with exit code 1104 (path too long). Fix: set `CARGO_TARGET_DIR` to the original repo's Rust target directory (e.g. `export CARGO_TARGET_DIR="D:/Repos/_Ivy/Ivy-Framework/src/Ivy.Docs.Tools/rust_cli/target"`). This reuses the cached build artifacts and avoids recompilation with long worktree paths. Also add `/c/msys64/mingw64/bin` to PATH for `dlltool.exe` if needed.

## Ivy-Framework: vp CLI not on PATH (pnpm isolated layout)
In the original Ivy-Framework repo, pnpm uses isolated `node_modules` layout without a `.bin/` directory at `src/frontend/node_modules/.bin/`. The `vp` binary is at `src/frontend/node_modules/vite-plus/bin/vp`. The MSBuild targets `NpmInstall` and `BuildFrontend` call `vp install` and `vp run build` directly. If `vp` is not on the system PATH, create a temporary `vp.cmd` shim: `@node "D:\Repos\_Ivy\Ivy-Framework\src\frontend\node_modules\vite-plus\bin\vp" %*` in a PATH directory (e.g. `C:\Users\<user>\bin\`). Delete the shim during cleanup. Alternatively, ensure the `dist/.build-stamp` file is fresh (touch it) so MSBuild skips the frontend build entirely.

## Worktree cross-repo relative references
When a repo (e.g. `Ivy`) has relative `ProjectReference` paths to sibling repos (e.g. `..\..\Ivy-Framework\...`), these break in worktrees because the worktree is in a different directory. Fix: create a temporary directory junction at the expected relative path (e.g. `worktrees/Ivy-Framework` -> `D:\Repos\_Ivy\Ivy-Framework`). Remove the junction after build/verification using PowerShell `[IO.Directory]::Delete("$path")` — `cmd.exe /c "rmdir <path>"` fails silently on long worktree paths. For `dotnet format` and `dotnet test`, target the specific `.csproj` instead of the `.slnx` to avoid restore failures from missing sibling projects.

The `Ivy` repo also has a **conditional** project reference to `../../Ivy-Agent/Ivy.Agent.Shared` (falls back to NuGet when the path doesn't exist). In worktrees, the path doesn't exist, so the NuGet fallback is used — but if the NuGet package is outdated relative to the source, builds will fail with type mismatches. Fix: also create a junction for `Ivy-Agent` (e.g. `worktrees/Ivy-Agent` -> `D:\Repos\_Ivy\Ivy-Agent`).

## Roslyn CodeFixProvider requires Workspaces package
The `CodeFixProvider` base class, `CodeAction`, `Document`, and related types live in `Microsoft.CodeAnalysis.CSharp.Workspaces` (not `Microsoft.CodeAnalysis.CSharp`). When adding a CodeFixProvider to an analyzer project that only references `Microsoft.CodeAnalysis.CSharp`, you must add `Microsoft.CodeAnalysis.CSharp.Workspaces` and suppress `RS1038` (analyzer shouldn't reference Workspaces). This is the standard pattern for combined analyzer+codefix assemblies.

## Roslyn testing package incompatible with xunit 2.5+
`Microsoft.CodeAnalysis.CSharp.CodeFix.Testing.XUnit` 1.1.2 (latest as of April 2026) is incompatible with xunit 2.5+ due to a breaking change in `Xunit.Sdk.EqualException` constructor. Fix: use `Microsoft.CodeAnalysis.CSharp.CodeFix.Testing` 1.1.3 (base package without XUnit suffix) with `DefaultVerifier` instead. Create a custom `CSharpCodeFixVerifier<TAnalyzer, TCodeFix>` helper that wraps `CSharpCodeFixTest<TAnalyzer, TCodeFix, DefaultVerifier>`. For tests with multiple diagnostics, set `NumberOfFixAllIterations` to the number of diagnostics (BatchFixer applies one fix per iteration).

## Roslyn CodeFix: SyntaxFactory produces CRLF trivia on Windows
`SyntaxFactory.Block()`, `SyntaxFactory.ConstructorDeclaration()`, and other factory methods produce `\r\n` (CRLF) EndOfLine trivia on Windows, regardless of the document's actual line endings. This breaks Roslyn code fix verifier tests (which use LF). `ReplaceTrivia` on `DescendantTrivia()` may not catch all CRLF tokens. Fix: parse the new syntax from a string literal with the correct line ending: `SyntaxFactory.ParseMemberDeclaration($"public {name}(){eol}{indent}{{{eol}{indent}}}")`. Detect the document's line ending from existing trivia.

## Roslyn CodeFix: FindNode can return parent invocations
When using `root.FindNode(diagnosticSpan).FirstAncestorOrSelf<InvocationExpressionSyntax>()` to locate a hook call inside a nested expression like `CreateWidget(UseEffect(42))`, FindNode may return the `ArgumentSyntax` node, causing `FirstAncestorOrSelf` to return `CreateWidget(...)` instead of `UseEffect(...)`. Fix: after finding the node, search its descendants for an `InvocationExpressionSyntax` matching the diagnostic span AND the expected naming pattern (e.g. `Use*` for hooks). Use `getInnermostNodeForTie: true` when calling `FindNode`.

## Roslyn verifier: diagnostic markers must wrap actual code spans
The `{|#0:text|}` marker syntax in Roslyn verifier tests defines both the diagnostic span AND the actual code text. The markers must wrap the EXACT code that the analyzer reports the diagnostic on. For invocation diagnostics (reported on `InvocationExpressionSyntax`), wrap the full invocation including arguments: `{|#0:UseState(0)|}` not just `{|#0:UseState|}`. Markers in comments are ignored.

## PowerShell Set-StrictMode and Get-ChildItem .Count
When using `Set-StrictMode -Version Latest`, calling `.Count` on a `Get-ChildItem` result that returns a single item (not an array) will throw `The property 'Count' cannot be found on this object`. Fix: always wrap with `[array]$var = @(Get-ChildItem ...)` to ensure array type. This applies to all PowerShell tools in the ExecutePlan toolchain.
