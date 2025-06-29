using System.Diagnostics;
using System.Linq.Expressions;
using Ivy.Charts;
using Ivy.Core.Helpers;

namespace Ivy.Test;

[Trait("Category", "Benchmark")]
public class PerformanceBenchmarkTests
{
    private readonly Random _random = new(42); // Fixed seed for consistent results

    [Fact]
    [Trait("Category", "Benchmark")]
    public void TreeRebuildSolver_Performance_Benchmark()
    {
        // Generate test data
        var testPaths = GenerateTestPaths(1000, 10);
        
        // Warm up
        _ = TreeRebuildSolver.FindMinimalRebuildNodes(testPaths);
        
        // Benchmark current implementation
        var stopwatch = Stopwatch.StartNew();
        var result1 = TreeRebuildSolver.FindMinimalRebuildNodes(testPaths);
        stopwatch.Stop();
        var currentTime = stopwatch.ElapsedTicks;
        
        // Benchmark LINQ-based alternative
        stopwatch.Restart();
        var result2 = FindMinimalRebuildNodesLinq(testPaths);
        stopwatch.Stop();
        var linqTime = stopwatch.ElapsedTicks;
        
        // Verify results are equivalent
        Assert.Equal(result1.Length, result2.Length);
        Assert.True(result1.All(r => result2.Contains(r)));
        Assert.True(result2.All(r => result1.Contains(r)));
        
        // Output performance comparison
        var ratio = (double)linqTime / currentTime;
        Console.WriteLine($"TreeRebuildSolver Performance:");
        Console.WriteLine($"  Current implementation: {currentTime} ticks");
        Console.WriteLine($"  LINQ implementation: {linqTime} ticks");
        Console.WriteLine($"  Ratio (LINQ/Current): {ratio:F2}x");
        
        // Assert that current implementation is not significantly slower
        Assert.True(ratio > 0.5, $"LINQ implementation is {ratio:F2}x slower than current implementation");
    }

    [Fact]
    [Trait("Category", "Benchmark")]
    public void PivotTable_Grouping_Performance_Benchmark()
    {
        var testData = GenerateTestData(10000);
        
        // Test different grouping approaches
        var results = new Dictionary<string, (long ticks, int resultCount)>();
        
        // Approach 1: Current PivotTable implementation
        var pivotTable = new PivotTable<TestRecord>(
            [new Dimension<TestRecord>("Category", r => r.Category)],
            [new Measure<TestRecord>("Sum", q => q.Sum(r => r.Value))]
        );
        
        var stopwatch = Stopwatch.StartNew();
        var result1 = pivotTable.ExecuteAsync(testData.AsQueryable()).Result;
        stopwatch.Stop();
        results["PivotTable"] = (stopwatch.ElapsedTicks, result1.Length);
        
        // Approach 2: Direct LINQ grouping
        stopwatch.Restart();
        var result2 = testData
            .GroupBy(r => r.Category)
            .Select(g => new Dictionary<string, object>
            {
                ["Category"] = g.Key,
                ["Sum"] = g.Sum(r => r.Value)
            })
            .ToArray();
        stopwatch.Stop();
        results["DirectLINQ"] = (stopwatch.ElapsedTicks, result2.Length);
        
        // Approach 3: Manual grouping with Dictionary
        stopwatch.Restart();
        var result3 = ManualGrouping(testData);
        stopwatch.Stop();
        results["ManualGrouping"] = (stopwatch.ElapsedTicks, result3.Length);
        
        // Output results
        Console.WriteLine($"PivotTable Grouping Performance (10,000 records):");
        foreach (var (method, (ticks, count)) in results.OrderBy(r => r.Value.ticks))
        {
            Console.WriteLine($"  {method}: {ticks} ticks, {count} results");
        }
        
        // Verify all approaches produce same number of results
        Assert.Equal(results["PivotTable"].resultCount, results["DirectLINQ"].resultCount);
        Assert.Equal(results["PivotTable"].resultCount, results["ManualGrouping"].resultCount);
    }

    [Fact]
    [Trait("Category", "Benchmark")]
    public void Expression_Compilation_Performance_Benchmark()
    {
        var testData = GenerateTestData(1000);
        
        // Test expression compilation vs direct delegates
        var results = new Dictionary<string, long>();
        
        // Approach 1: Expression compilation (current approach)
        Expression<Func<TestRecord, object>> expr = r => r.Category;
        var compiled = expr.Compile();
        
        var stopwatch = Stopwatch.StartNew();
        for (int i = 0; i < 1000; i++)
        {
            var result = testData.Select(compiled).ToArray();
        }
        stopwatch.Stop();
        results["ExpressionCompilation"] = stopwatch.ElapsedTicks;
        
        // Approach 2: Direct delegate
        Func<TestRecord, object> direct = r => r.Category;
        
        stopwatch.Restart();
        for (int i = 0; i < 1000; i++)
        {
            var result = testData.Select(direct).ToArray();
        }
        stopwatch.Stop();
        results["DirectDelegate"] = stopwatch.ElapsedTicks;
        
        // Approach 3: Inline lambda
        stopwatch.Restart();
        for (int i = 0; i < 1000; i++)
        {
            var result = testData.Select(r => r.Category).ToArray();
        }
        stopwatch.Stop();
        results["InlineLambda"] = stopwatch.ElapsedTicks;
        
        // Output results
        Console.WriteLine($"Expression Compilation Performance:");
        foreach (var (method, ticks) in results.OrderBy(r => r.Value))
        {
            Console.WriteLine($"  {method}: {ticks} ticks");
        }
        
        // Expression compilation should be reasonable (not orders of magnitude slower)
        var ratio = (double)results["ExpressionCompilation"] / results["DirectDelegate"];
        Assert.True(ratio < 10, $"Expression compilation is {ratio:F2}x slower than direct delegate");
    }

    [Fact]
    [Trait("Category", "Benchmark")]
    public void Large_Dataset_PivotTable_Performance()
    {
        var largeDataset = GenerateTestData(100000);
        
        var stopwatch = Stopwatch.StartNew();
        var pivotTable = new PivotTable<TestRecord>(
            [new Dimension<TestRecord>("Category", r => r.Category)],
            [
                new Measure<TestRecord>("Count", q => q.Count()),
                new Measure<TestRecord>("Sum", q => q.Sum(r => r.Value)),
                new Measure<TestRecord>("Average", q => q.Average(r => r.Value))
            ]
        );
        
        var result = pivotTable.ExecuteAsync(largeDataset.AsQueryable()).Result;
        stopwatch.Stop();
        
        Console.WriteLine($"Large Dataset PivotTable Performance:");
        Console.WriteLine($"  100,000 records processed in {stopwatch.ElapsedMilliseconds}ms");
        Console.WriteLine($"  Generated {result.Length} pivot rows");
        
        // Performance assertion - should complete within reasonable time
        Assert.True(stopwatch.ElapsedMilliseconds < 5000, $"PivotTable took {stopwatch.ElapsedMilliseconds}ms, expected < 5000ms");
    }

    // Helper methods for LINQ-based alternatives
    private static string[] FindMinimalRebuildNodesLinq(string[][] paths)
    {
        if (paths.Length == 0) return Array.Empty<string>();
        if (paths.Length == 1) return [paths[0].Last()];

        var sortedPaths = paths
            .Where(p => p.Length > 0)
            .OrderBy(p => p.Length)
            .ToArray();

        var result = new HashSet<string>();

        foreach (var path in sortedPaths)
        {
            string lastNode = path[^1];

            // Check if any ancestor is already in result using LINQ
            bool hasAncestor = path.Take(path.Length - 1).Any(ancestor => result.Contains(ancestor));

            if (!hasAncestor)
            {
                // Remove descendants using LINQ
                var descendantsToRemove = result.Where(existing => 
                    IsDescendantLinq(existing, lastNode, sortedPaths)).ToArray();
                
                foreach (var descendant in descendantsToRemove)
                {
                    result.Remove(descendant);
                }
                
                result.Add(lastNode);
            }
        }

        return result.ToArray();
    }

    private static bool IsDescendantLinq(string potentialDescendant, string node, string[][] paths)
    {
        return paths.Any(path =>
        {
            int nodeIndex = Array.IndexOf(path, node);
            int descendantIndex = Array.IndexOf(path, potentialDescendant);
            return nodeIndex != -1 && descendantIndex != -1 && nodeIndex < descendantIndex;
        });
    }

    private static Dictionary<string, object>[] ManualGrouping(IEnumerable<TestRecord> data)
    {
        var groups = new Dictionary<string, List<TestRecord>>();
        
        foreach (var record in data)
        {
            if (!groups.ContainsKey(record.Category))
            {
                groups[record.Category] = new List<TestRecord>();
            }
            groups[record.Category].Add(record);
        }
        
        return groups.Select(g => new Dictionary<string, object>
        {
            ["Category"] = g.Key,
            ["Sum"] = g.Value.Sum(r => r.Value)
        }).ToArray();
    }

    // Test data generation
    private string[][] GenerateTestPaths(int count, int maxDepth)
    {
        var paths = new List<string[]>();
        var categories = new[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J" };
        
        for (int i = 0; i < count; i++)
        {
            var depth = _random.Next(1, maxDepth + 1);
            var path = new string[depth];
            
            for (int j = 0; j < depth; j++)
            {
                path[j] = $"{categories[_random.Next(categories.Length)]}{j}";
            }
            
            paths.Add(path);
        }
        
        return paths.ToArray();
    }

    private List<TestRecord> GenerateTestData(int count)
    {
        var categories = new[] { "Electronics", "Clothing", "Books", "Home", "Sports", "Food", "Automotive", "Health", "Beauty", "Toys" };
        var data = new List<TestRecord>();
        
        for (int i = 0; i < count; i++)
        {
            data.Add(new TestRecord
            {
                Id = i,
                Name = $"Product {i}",
                Category = categories[_random.Next(categories.Length)],
                Value = _random.Next(1, 1000),
                Price = _random.NextDouble() * 1000
            });
        }
        
        return data;
    }

    // Test data model
    private class TestRecord
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Category { get; set; } = "";
        public int Value { get; set; }
        public double Price { get; set; }
    }
} 