namespace Ivy.Core.Helpers;

public class TreeRebuildSolver
{
    public static string[] FindMinimalRebuildNodes(string[][] paths)
    {
        if (paths.Length == 0) return Array.Empty<string>();
        if (paths.Length == 1) return [paths[0].Last()];

        // Use HashSet to store results for O(1) lookups and removals
        var result = new HashSet<string>();

        // Process paths from shortest to longest to minimize removals
        var sortedPaths = paths
            .Where(p => p.Length > 0)
            .OrderBy(p => p.Length)
            .ToArray();

        foreach (var path in sortedPaths)
        {
            string lastNode = path[^1]; // Using index from end operator

            // Skip if any ancestor is already in result
            bool needsAdd = true;
            for (int i = 0; i < path.Length - 1; i++)
            {
                if (result.Contains(path[i]))
                {
                    needsAdd = false;
                    break;
                }
            }

            if (needsAdd)
            {
                // Remove any descendants that are in result
                result.RemoveWhere(existing =>
                    IsDescendant(existing, lastNode, sortedPaths));
                result.Add(lastNode);
            }
        }

        return result.ToArray();
    }

    // Check if potentialDescendant is a descendant of node in any path
    private static bool IsDescendant(string potentialDescendant, string node, string[][] paths)
    {
        foreach (var path in paths)
        {
            int nodeIndex = Array.IndexOf(path, node);
            int descendantIndex = Array.IndexOf(path, potentialDescendant);

            if (nodeIndex != -1 && descendantIndex != -1 &&
                nodeIndex < descendantIndex)
            {
                return true;
            }
        }
        return false;
    }
}