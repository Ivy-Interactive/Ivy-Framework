using System.Reflection;
using Ivy.Alerts;
using Ivy.Shared;

namespace Ivy.Apps;

public static class AppHelpers
{
    public static AppDescriptor[] GetApps(Assembly? assembly = null)
    {
        var apps = new List<AppDescriptor>();
        assembly ??= Assembly.GetEntryAssembly();
        if (assembly == null)
            throw new InvalidOperationException("Entry assembly not found.");
        foreach (var type in assembly.GetTypes())
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
            var path = appAttribute.Path ?? GetPathFromNamespace(type) ?? ["Apps"];

            string GetId()
            {
                if (type.Namespace == null)
                {
                    return Utils.TitleCaseToFriendlyUrl(type.Name);
                }
                var ns = type.Namespace!.Split(".");
                if (ns.Contains("Apps"))
                {
                    ns = ns[(Array.IndexOf(ns, "Apps") + 1)..];
                }
                ns = [.. ns, type.Name];
                return string.Join("/", ns.Select(Utils.TitleCaseToFriendlyUrl));
            }

            string GetTitle()
            {
                if (type.Name is "_Index" or "_IndexApp")
                {
                    return path[^1];
                }
                return Utils.TitleCaseToReadable(type.Name); //DatePickerApp => Date Picker
            }

            return new AppDescriptor()
            {
                Id = appAttribute.Id ?? GetId(),
                Title = appAttribute.Title ?? GetTitle(),
                Icon = appAttribute.Icon == Icons.None ? null : appAttribute.Icon,
                Description = appAttribute.Description,
                Type = type,
                Path = path,
                IsVisible = !type.Name.StartsWith("_") && appAttribute.IsVisible,
                IsIndex = type.Name is "_Index" or "_IndexApp",
                Order = appAttribute.Order,
                GroupExpanded = appAttribute.GroupExpanded,
                DocumentSource = appAttribute.DocumentSource
            };
        }
        throw new InvalidOperationException($"Type '{type.FullName}' is missing the [App] attribute.");
    }

    private static string[]? GetPathFromNamespace(Type type)
    {
        //Check that the namespace is in the form of *.Apps.* and return the parts after Apps
        //Ivy.Apps.Widgets.DatePickerApp => [ "Widgets", "DatePickerApp" ]

        var parts = type.Namespace?.Split('.');
        if (parts == null)
            return null;
        var index = Array.IndexOf(parts, "Apps");
        if (index == -1 || index == parts.Length - 1)
            return null;

        return parts[(index + 1)..].Select(Utils.TitleCaseToReadable).ToArray();
    }
}