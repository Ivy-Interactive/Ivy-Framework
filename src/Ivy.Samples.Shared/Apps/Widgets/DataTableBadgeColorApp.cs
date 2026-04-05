namespace Ivy.Samples.Shared.Apps.Widgets;

[App(icon: Icons.Palette, group: ["Widgets"], title: "DataTable Badge Colors")]
public class DataTableBadgeColorApp : SampleBase
{
    protected override object? BuildSample()
    {
        var data = new[]
        {
            new BadgeSample(1, "Alice Johnson", ["Python", "React", "Azure"], "High", 95),
            new BadgeSample(2, "Bob Smith", ["C#", "SQL", "DotNet"], "Medium", 82),
            new BadgeSample(3, "Charlie Brown", ["JavaScript", "CSS", "HTML"], "Low", 64),
            new BadgeSample(4, "Diana Prince", ["Leadership", "Management", "Agile"], "High", 98),
            new BadgeSample(5, "Edward Norton", ["Python", "C#", "SQL"], "Medium", 75),
        }.AsQueryable();

        return data.ToDataTable()
            .Header(e => e.Skills, "Skills (Multi-Color)")
            .Badges(e => e.Skills, new Dictionary<string, Colors> {
                { "Python", Colors.Sky },
                { "React", Colors.Blue },
                { "C#", Colors.Purple },
                { "SQL", Colors.Orange },
                { "Azure", Colors.Cyan },
                { "DotNet", Colors.Indigo },
                { "JavaScript", Colors.Yellow },
                { "CSS", Colors.Pink },
                { "HTML", Colors.Red },
                { "Leadership", Colors.Emerald },
                { "Management", Colors.Slate },
                { "Agile", Colors.Amber }
            })
            
            .Header(e => e.Priority, "Priority Labels")
            .Badges(e => e.Priority, new Dictionary<string, Colors> {
                { "High", Colors.Red },
                { "Medium", Colors.Orange },
                { "Low", Colors.Green }
            })

            .Width(Size.Full())
            .Height(Size.Units(80));
    }
}

public record BadgeSample(int Id, string Name, string[] Skills, string Priority, int Score);
