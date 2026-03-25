using System.Reflection;

namespace Ivy.Core.Apps;

using Ivy;

public static class AppHelpers
{
    public static AppDescriptor[] GetApps(Assembly? assembly = null)
    {
        var apps = new List<AppDescriptor>();
        assembly ??= Assembly.GetEntryAssembly();
        if (assembly == null)
            throw new InvalidOperationException("Entry assembly not found.");
        foreach (var type in assembly.GetLoadableTypes())
        {
            if (type.GetCustomAttribute<AppAttribute>() != null)
            {
                apps.Add(GetApp(type));
            }
        }
        return apps.ToArray();
    }

    public static AppDescriptor GetApp(Type type)
    {
        var appAttribute = type.GetCustomAttribute<AppAttribute>();
        if (appAttribute != null)
        {
            var group = appAttribute.Group ?? GetGroupFromNamespace(type) ?? ["Apps"];

            string GetId()
            {
                if (type.Namespace == null)
                {
                    return global::Ivy.StringHelper.TitleCaseToFriendlyUrl(type.Name);
                }
                var ns = type.Namespace!.Split(".");
                if (ns.Contains("Apps"))
                {
                    ns = ns[(Array.IndexOf(ns, "Apps") + 1)..];
                }
                ns = [.. ns, type.Name];
                return string.Join("/", ns.Select(global::Ivy.StringHelper.TitleCaseToFriendlyUrl));
            }

            string GetTitle()
            {
                if (type.Name is "_Index" or "_IndexApp")
                {
                    return group[^1];
                }
                return global::Ivy.StringHelper.TitleCaseToReadable(type.Name); //DatePickerApp => Date Picker
            }

            return new AppDescriptor()
            {
                Id = appAttribute.Id ?? GetId(),
                Title = appAttribute.Title ?? GetTitle(),
                Icon = appAttribute.Icon == Icons.None ? null : appAttribute.Icon,
                Description = appAttribute.Description,
                Type = type,
                Group = group,
                IsVisible = !type.Name.StartsWith("_") && appAttribute.IsVisible,
                IsIndex = type.Name is "_Index" or "_IndexApp",
                Order = appAttribute.Order,
                GroupExpanded = appAttribute.GroupExpanded,
                DocumentSource = appAttribute.DocumentSource,
                SearchHints = appAttribute.SearchHints,
            };
        }
        throw new InvalidOperationException($"Type '{type.FullName}' is missing the [App] attribute.");
    }

    private static string[]? GetGroupFromNamespace(Type type)
    {
        //Check that the namespace is in the form of *.Apps.* and return the parts after Apps
        //Ivy.Apps.Widgets.DatePickerApp => [ "Widgets", "DatePickerApp" ]

        var parts = type.Namespace?.Split('.');
        if (parts == null)
            return null;
        var index = Array.IndexOf(parts, "Apps");
        if (index == -1 || index == parts.Length - 1)
            return null;

        return parts[(index + 1)..].Select(global::Ivy.StringHelper.TitleCaseToReadable).ToArray();
    }

    public static void RegisterBeacons(Assembly assembly, NavigationBeaconRegistry registry)
    {
        foreach (var type in assembly.GetLoadableTypes())
        {
            var appAttr = type.GetCustomAttribute<AppAttribute>();
            if (appAttr == null) continue;

            var beaconAttrs = type.GetCustomAttributes<NavigationBeaconAttribute>();
            foreach (var attr in beaconAttrs)
            {
                var method = type.GetMethod(attr.FactoryMethodName,
                    BindingFlags.Public | BindingFlags.Static);
                if (method == null)
                    throw new InvalidOperationException(
                        $"Static method '{attr.FactoryMethodName}' not found on '{type.FullName}'.");

                var beacon = method.Invoke(null, null)
                    ?? throw new InvalidOperationException(
                        $"Beacon factory '{attr.FactoryMethodName}' on '{type.FullName}' returned null.");

                var registerMethod = typeof(NavigationBeaconRegistry)
                    .GetMethod(nameof(NavigationBeaconRegistry.Register))!
                    .MakeGenericMethod(attr.EntityType);
                registerMethod.Invoke(registry, [beacon]);
            }
        }
    }
}
