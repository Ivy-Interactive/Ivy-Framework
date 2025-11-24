namespace Ivy.Samples.Shared.Apps.Concepts.Models;

public class DisplayExample
{
    [Display(
        Name = "Custom Name",
        Description = "This is a custom description.",
        Order = 2,
        Prompt = "Enter value here") //Shown as placeholder
    ]
    public string CustomDisplayString { get; set; } = "Foo";

    [Display(GroupName = "Extras")]
    public string Foo { get; set; } = "Foo Value";

    [Display(GroupName = "Extras")]
    public string Bar { get; set; } = "Bar Value";

}