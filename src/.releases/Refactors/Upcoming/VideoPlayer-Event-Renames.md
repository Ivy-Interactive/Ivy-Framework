# VideoPlayer Event Renames

## Summary

The event handler extension methods for the `VideoPlayer` widget have been renamed from `Handle[Event]` to `On[Event]` to align with the standard naming convention used across the Ivy Framework (e.g., `Button.OnClick`, `SelectInput.OnChange`).

## What Changed

### Before
```csharp
var video = new VideoPlayer(url)
    .HandlePlay(() => Console.WriteLine("Playing"))
    .HandlePause(() => Console.WriteLine("Paused"))
    .HandleEnded(() => Console.WriteLine("Ended"))
    .HandleLoaded(() => Console.WriteLine("Loaded"));
```

### After
```csharp
var video = new VideoPlayer(url)
    .OnPlay(() => Console.WriteLine("Playing"))
    .OnPause(() => Console.WriteLine("Paused"))
    .OnEnded(() => Console.WriteLine("Ended"))
    .OnLoaded(() => Console.WriteLine("Loaded"));
```

## How to Find Affected Code

Run `dotnet build`. The old methods have been removed, so any code using them will fail to compile.

Or search for:
- `.HandlePlay(`
- `.HandlePause(`
- `.HandleEnded(`
- `.HandleLoaded(`
