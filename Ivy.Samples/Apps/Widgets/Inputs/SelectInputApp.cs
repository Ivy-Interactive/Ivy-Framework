using Ivy.Shared;

namespace Ivy.Samples.Apps.Widgets.Inputs;

public enum Color
{
    Red,
    Green,
    Blue
}

[App(icon: Icons.LassoSelect)]
public class SelectInputApp : SampleBase
{
    private static readonly Option<Guid>[] GuidOptions =
    [
        new("Niels", Guid.NewGuid()),
        new("Frida", Guid.NewGuid()),
        new("Julia", Guid.NewGuid()),
        new("Olivia", Guid.NewGuid())
    ];

    private static readonly IAnyOption[] IntOptions =
    [
        new Option<int>("Niels", 1),
        new Option<int>("Frida", 2),
        new Option<int>("Julia", 3),
        new Option<int>("Olivia", 4)
    ];

    private static readonly IAnyOption[] StringOptions =
    [
        new Option<string>("Niels"),
        new Option<string>("Frida"),
        new Option<string>("Julia"),
        new Option<string>("Olivia")
    ];

    private enum Features
    {
        AllowOverage,
        RemoveBranding
    }

    protected override object? BuildSample()
    {
        var enumArrayState = this.UseState<List<Features>>();

        return Layout.Vertical()
               | enumArrayState.ToSelectInput().List()
               | enumArrayState;


        // var intArrayState = this.UseState<int[]>();
        // var intArrayInput = intArrayState.ToSelectInput(IntOptions).List();
        //
        // var intState = this.UseState(1);
        // var intInput = intState.ToSelectInput(IntOptions);
        //
        // var stringArrayState = this.UseState<string[]>();
        // var stringArrayInput = stringArrayState.ToSelectInput(StringOptions).List();
        //
        // var stringState = this.UseState("Niels");
        // var stringInput = stringState.ToSelectInput(StringOptions);
        //
        // var guidArrayState = this.UseState<Guid[]>();
        // var guidArrayInput = guidArrayState.ToSelectInput(GuidOptions).List();
        //
        // var guidState = this.UseState<Guid>(GuidOptions[0].TypedValue);
        // var guidInput = guidState.ToSelectInput(GuidOptions);
        //
        // var colorState = this.UseState(Color.Red);
        //
        // var options = typeof(Color).ToOptions();
        //
        // return Layout.Vertical(
        //     intArrayInput,
        //     intInput,
        //     stringArrayInput,
        //     stringInput,
        //     guidArrayInput,
        //     guidInput,
        //
        //     colorState.ToSelectInput(options),
        //     colorState.ToSelectInput(options).Disabled(),
        //     colorState.ToSelectInput(options).Variant(SelectInputs.List),
        //     colorState.ToSelectInput(options).Variant(SelectInputs.List).Disabled(),
        //     colorState.ToSelectInput(options).Variant(SelectInputs.Toggle),
        //     colorState.ToSelectInput(options).Variant(SelectInputs.Toggle).Disabled()
        // );
    }
}