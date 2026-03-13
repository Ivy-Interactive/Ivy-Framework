using System.Text.Json.Nodes;

namespace Ivy.Samples.Shared.Apps.Widgets.Primitives;

[App(icon: Icons.Braces, path: ["Widgets", "Primitives"], searchHints: ["data", "format", "json", "syntax", "structure", "object"])]
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
        var dog = new { Breed = "Poodle", Color = "White", Age = 3 };

        return Layout.Vertical().Gap(4)
            | new Text("From JsonNode:")
            | new Json(json)
            | new Text("From object (auto-serialized):")
            | new Json(dog)
            | new Text("Expanded to depth 2:")
            | new Json(json) { Expanded = 2 }
            | new Text("Fully expanded:")
            | new Json(json) { Expanded = -1 };
    }
}
