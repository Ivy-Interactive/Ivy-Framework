# Ivy.Plugin.Abstractions

Provides the core abstractions and interfaces for building plugins in the Ivy framework.

This assembly ships inside the `Ivy` NuGet package (`lib/net10.0/Ivy.Plugin.Abstractions.dll`) rather
than as a package of its own. It deliberately does not reference `Ivy`, so a plugin can be written
against these abstractions alone.
