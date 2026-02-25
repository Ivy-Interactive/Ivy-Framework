using Ivy.Core;
using Ivy.Shared;

namespace Ivy.Charts;

/// <summary>
/// Represents a node in a hierarchical data structure for a SunburstChart.
/// </summary>
public record SunburstNode
{
  public SunburstNode(string name, double value, SunburstNode[]? children = null)
  {
    Name = name;
    Value = value;
    Children = children ?? [];
  }

  public string Name { get; init; }
  public double Value { get; init; }
  public Colors? Fill { get; init; } = null;  // Optional per-node color
  public SunburstNode[] Children { get; init; } = [];
}
