using Ivy.Core.Helpers;
using Ivy.Shared;

namespace Ivy.Samples.Shared.Apps.Concepts;

[App(icon: Icons.BrainCog)]
public class MemoizationApp : SampleBase
{
    protected override object? BuildSample()
    {
        var list = this.UseState(() => ImmutableArray.Create(1, 2, 3));

        return Layout.Vertical(
            Layout.Horizontal(
                new Button("Add", _ => list.Set(list.Value.Add(list.Value.Length + 1))),
                new Button("Remove", _ => list.Set(list.Value.RemoveAt(list.Value.Length - 1)))
            ),
            new Separator(),
            Layout.Vertical(
                list.Value.Select(
                    (x, i) => new ListItem(x, i,
                        () => { list.Set(list.Value.MoveUp(i)); },
                        () => { list.Set(list.Value.MoveDown(i)); }
                    ).Key(x) // key is needed for memoization to work where the items can be edited
                )
            )
        );
    }
}

public class ListItem(int value, int index, Action moveUp, Action moveDown) : ViewBase, IMemoized
{
    private int Value { get; } = value;

    private int Index { get; } = index;

    public object[] GetMemoValues()
    {
        return [Value, Index]; //if index has changed the item need a re-render
    }

    public override object? Build()
    {
        return
            Layout.Vertical(
                Layout.Horizontal(
                    new Button("", _ =>
                    {
                        moveUp();
                    }, icon: Icons.ArrowBigUp, variant: ButtonVariant.Secondary),
                    new Button("", _ =>
                    {
                        moveDown();
                    }, icon: Icons.ArrowBigDown, variant: ButtonVariant.Secondary),
                    Text.Block(this.Value + ":" + DateTime.Now.Ticks)
                ).Align(Align.Center),
                new Separator()
            );
    }
}

