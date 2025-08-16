using System.Text.Json.Nodes;
using Ivy.Shared;

namespace Ivy.Samples.Shared.Apps.Widgets.Primitives;

[App(icon: Icons.Braces, path: ["Widgets", "Primitives"])]
public class JsonApp : SampleBase
{
    protected override object? BuildSample()
    {
        var json = new JsonObject
        {
            ["name"] = "John Doe",
            ["age"] = 30,
            ["isStudent"] = false,
            ["address"] = new JsonObject
            {
                ["street"] = "123 Main St",
                ["city"] = "Anytown",
                ["state"] = "NY",
                ["zip"] = "12345"
            },
            ["phoneNumbers"] = new JsonArray
            {
                "555-1234",
                "555-5678"
            }
        };
        return json;
    }
}