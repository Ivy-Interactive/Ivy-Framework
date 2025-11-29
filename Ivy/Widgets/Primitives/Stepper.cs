using System.Runtime.CompilerServices;
using Ivy.Core;
using Ivy.Shared;

// ReSharper disable once CheckNamespace
namespace Ivy;

public record StepperItem(string Symbol, Icons? Icon = null, string? Label = null, string? Description = null);

public record Stepper : WidgetBase<Stepper>
{
    [OverloadResolutionPriority(1)]
    public Stepper(Func<Event<Stepper, int>, ValueTask>? onSelect, int? selectedIndex, params IEnumerable<StepperItem> items)
    {
        OnSelect = onSelect;
        SelectedIndex = selectedIndex;
        Items = items.ToArray();
    }

    [Prop] public int? SelectedIndex { get; set; }

    [Prop] public StepperItem[] Items { get; set; }

    [Prop] public bool AllowSelectForward { get; set; } = false;

    [Event] public Func<Event<Stepper, int>, ValueTask>? OnSelect { get; set; }
}

public static class StepperExtensions
{
    public static Stepper HandleSelect(this Stepper stepper, Func<Event<Stepper, int>, ValueTask> onSelect) => stepper with { OnSelect = onSelect };
    public static Stepper AllowSelectForward(this Stepper stepper, bool allowSelectForward = true) => stepper with { AllowSelectForward = allowSelectForward };
}



