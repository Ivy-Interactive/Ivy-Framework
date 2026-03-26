namespace Ivy;

public record ExceptionHint(string Title, string Description, CalloutVariant Variant = CalloutVariant.Info);

public static class ExceptionHints
{
    private static readonly Dictionary<string, ExceptionHint> Hints = new(StringComparer.OrdinalIgnoreCase)
    {
        ["BadImageFormatException"] = new ExceptionHint(
            "Hint: BadImageFormatException",
            "This usually means a **32-bit/64-bit mismatch** or a **corrupted assembly**. In Ivy apps, common causes:\n- Your Ivy project targets a different platform than a referenced native library\n- A NuGet package restored for the wrong architecture — try `dotnet nuget locals all --clear` then rebuild\n- Clean and rebuild the solution (`dotnet clean && dotnet build`)"),

        ["NullReferenceException"] = new ExceptionHint(
            "Hint: NullReferenceException",
            "An object reference was not set. In Ivy apps, common causes:\n- Accessing `UseState<T>().Value` before it has been initialized\n- Calling `UseArgs<T>()` when no args were passed to the view\n- A `UseQuery` result accessed before loading completes (check `.Loading` first)\n- A service not registered in `Program.cs` — ensure `server.Services.Add...()` is called"),

        ["FileNotFoundException"] = new ExceptionHint(
            "Hint: FileNotFoundException",
            "A required file or assembly could not be found. In Ivy apps, check:\n- Required NuGet packages are restored (`dotnet restore`)\n- If referencing local assemblies, ensure they are in the build output\n- Static file paths are correct and accessible from the server's working directory"),

        ["TypeLoadException"] = new ExceptionHint(
            "Hint: TypeLoadException",
            "A type could not be loaded, often due to **version mismatches**. In Ivy apps, check:\n- All Ivy NuGet packages are on the same version\n- No duplicate assembly versions in the output folder\n- Interface implementations match the expected signatures (rebuild after updating Ivy packages)"),

        ["TimeoutException"] = new ExceptionHint(
            "Hint: TimeoutException",
            "An operation timed out. In Ivy apps, check:\n- Database queries in `UseQuery` or `UseMutation` — consider adding a `CancellationToken` and increasing the timeout\n- External API calls — add retry logic with exponential backoff\n- Long-running `UseEffect` callbacks blocking the render pipeline"),

        ["InvalidOperationException"] = new ExceptionHint(
            "Hint: InvalidOperationException",
            "An operation was invalid for the current state. In Ivy apps, common causes:\n- Calling hooks (`UseState`, `UseEffect`, etc.) inside conditionals or loops — hooks must be called at the top of `Build()`\n- Modifying state during a render pass — use `UseEffect` or event handlers instead\n- Accessing `DbContext` directly instead of via `IDbContextFactory<T>`"),
    };

    /// <summary>
    /// Returns a hint for the given exception type name, or null if no hint is available.
    /// </summary>
    public static ExceptionHint? GetHint(string exceptionTypeName)
    {
        return Hints.GetValueOrDefault(exceptionTypeName);
    }
}
