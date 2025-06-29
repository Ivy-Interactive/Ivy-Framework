using System.Diagnostics;

namespace Ivy.Test;

public class HashingPerformanceBenchmarkTests
{
    private readonly Random _random = new(42); // Fixed seed for consistent results

    [Fact]
    public void StableHash_ShortStrings_Performance_Benchmark()
    {
        var testStrings = GenerateTestStrings(1000, 10, 50);
        
        // Warm up
        _ = StableHashBaseline("test");
        
        var results = new Dictionary<string, (long ticks, int hash)>();
        
        // Baseline implementation (original algorithm)
        var stopwatch = Stopwatch.StartNew();
        var baselineHash = StableHashBaseline(testStrings[0]);
        stopwatch.Stop();
        results["Baseline"] = (stopwatch.ElapsedTicks, baselineHash);
        
        // LINQ Aggregate approach
        stopwatch.Restart();
        var linqAggregateHash = StableHashLinqAggregate(testStrings[0]);
        stopwatch.Stop();
        results["LINQ_Aggregate"] = (stopwatch.ElapsedTicks, linqAggregateHash);
        
        // LINQ Select + Sum approach
        stopwatch.Restart();
        var linqSelectHash = StableHashLinqSelect(testStrings[0]);
        stopwatch.Stop();
        results["LINQ_Select"] = (stopwatch.ElapsedTicks, linqSelectHash);
        
        // LINQ ForEach approach
        stopwatch.Restart();
        var linqForEachHash = StableHashLinqForEach(testStrings[0]);
        stopwatch.Stop();
        results["LINQ_ForEach"] = (stopwatch.ElapsedTicks, linqForEachHash);
        
        // Verify all hashes are identical
        var expectedHash = results["Baseline"].hash;
        Assert.All(results.Values, result => Assert.Equal(expectedHash, result.hash));
        
        // Output performance comparison
        Console.WriteLine($"Short String Hashing Performance:");
        foreach (var (method, (ticks, hash)) in results.OrderBy(r => r.Value.ticks))
        {
            Console.WriteLine($"  {method}: {ticks} ticks");
        }
        
        // Baseline implementation should be fastest
        var fastest = results.OrderBy(r => r.Value.ticks).First();
        Assert.Equal("Baseline", fastest.Key);
    }

    [Fact]
    public void StableHash_LongStrings_Performance_Benchmark()
    {
        var testStrings = GenerateTestStrings(100, 1000, 5000);
        
        var results = new Dictionary<string, (long ticks, int hash)>();
        
        // Baseline implementation
        var stopwatch = Stopwatch.StartNew();
        var baselineHash = StableHashBaseline(testStrings[0]);
        stopwatch.Stop();
        results["Baseline"] = (stopwatch.ElapsedTicks, baselineHash);
        
        // LINQ Aggregate approach
        stopwatch.Restart();
        var linqAggregateHash = StableHashLinqAggregate(testStrings[0]);
        stopwatch.Stop();
        results["LINQ_Aggregate"] = (stopwatch.ElapsedTicks, linqAggregateHash);
        
        // LINQ Select + Sum approach
        stopwatch.Restart();
        var linqSelectHash = StableHashLinqSelect(testStrings[0]);
        stopwatch.Stop();
        results["LINQ_Select"] = (stopwatch.ElapsedTicks, linqSelectHash);
        
        // Verify all hashes are identical
        var expectedHash = results["Baseline"].hash;
        Assert.All(results.Values, result => Assert.Equal(expectedHash, result.hash));
        
        // Output performance comparison
        Console.WriteLine($"Long String Hashing Performance ({testStrings[0].Length} chars):");
        foreach (var (method, (ticks, hash)) in results.OrderBy(r => r.Value.ticks))
        {
            Console.WriteLine($"  {method}: {ticks} ticks");
        }
        
        // Baseline implementation should be fastest
        var fastest = results.OrderBy(r => r.Value.ticks).First();
        Assert.Equal("Baseline", fastest.Key);
    }

    [Fact]
    public void StableHash_UnicodeStrings_Performance_Benchmark()
    {
        var unicodeStrings = GenerateUnicodeStrings(100);
        
        var results = new Dictionary<string, (long ticks, int hash)>();
        
        // Baseline implementation
        var stopwatch = Stopwatch.StartNew();
        var baselineHash = StableHashBaseline(unicodeStrings[0]);
        stopwatch.Stop();
        results["Baseline"] = (stopwatch.ElapsedTicks, baselineHash);
        
        // LINQ Aggregate approach
        stopwatch.Restart();
        var linqAggregateHash = StableHashLinqAggregate(unicodeStrings[0]);
        stopwatch.Stop();
        results["LINQ_Aggregate"] = (stopwatch.ElapsedTicks, linqAggregateHash);
        
        // LINQ Select + Sum approach
        stopwatch.Restart();
        var linqSelectHash = StableHashLinqSelect(unicodeStrings[0]);
        stopwatch.Stop();
        results["LINQ_Select"] = (stopwatch.ElapsedTicks, linqSelectHash);
        
        // Verify all hashes are identical
        var expectedHash = results["Baseline"].hash;
        Assert.All(results.Values, result => Assert.Equal(expectedHash, result.hash));
        
        // Output performance comparison
        Console.WriteLine($"Unicode String Hashing Performance ({unicodeStrings[0].Length} chars):");
        foreach (var (method, (ticks, hash)) in results.OrderBy(r => r.Value.ticks))
        {
            Console.WriteLine($"  {method}: {ticks} ticks");
        }
    }

    [Fact]
    public void StableHash_EdgeCases_Performance_Benchmark()
    {
        var edgeCases = new[]
        {
            "",                    // Empty string
            "a",                   // Single character
            "ab",                  // Two characters
            "abc",                 // Three characters
            new string('a', 1000), // Repeated character
            new string('\0', 100), // Null characters
            "Hello, 世界! 🌍",      // Mixed ASCII and Unicode
            "🚀🎉🎊🎈🎁🎂🎄🎃🎗️🎟️", // Emoji only
        };
        
        var results = new Dictionary<string, Dictionary<string, (long ticks, int hash)>>();
        
        foreach (var testString in edgeCases)
        {
            var methodResults = new Dictionary<string, (long ticks, int hash)>();
            
            // Baseline implementation
            var stopwatch = Stopwatch.StartNew();
            var baselineHash = StableHashBaseline(testString);
            stopwatch.Stop();
            methodResults["Baseline"] = (stopwatch.ElapsedTicks, baselineHash);
            
            // LINQ Aggregate approach
            stopwatch.Restart();
            var linqAggregateHash = StableHashLinqAggregate(testString);
            stopwatch.Stop();
            methodResults["LINQ_Aggregate"] = (stopwatch.ElapsedTicks, linqAggregateHash);
            
            // Verify hashes are identical
            Assert.Equal(baselineHash, linqAggregateHash);
            
            results[testString.Length == 0 ? "Empty" : 
                   testString.Length == 1 ? "Single" :
                   testString.Length == 2 ? "Double" :
                   testString.Length == 3 ? "Triple" :
                   testString.All(c => c == 'a') ? "Repeated" :
                   testString.All(c => c == '\0') ? "NullChars" :
                   testString.Contains("世界") ? "MixedUnicode" :
                   testString.Contains("🚀") ? "Emoji" : "Other"] = methodResults;
        }
        
        // Output performance comparison
        Console.WriteLine($"Edge Cases Hashing Performance:");
        foreach (var (caseName, methodResults) in results)
        {
            Console.WriteLine($"  {caseName}:");
            foreach (var (method, (ticks, hash)) in methodResults.OrderBy(r => r.Value.ticks))
            {
                Console.WriteLine($"    {method}: {ticks} ticks");
            }
        }
    }

    [Fact]
    public void StableHash_BulkProcessing_Performance_Benchmark()
    {
        var testStrings = GenerateTestStrings(10000, 10, 100);
        
        var results = new Dictionary<string, (long ticks, int[] hashes)>();
        
        // Baseline implementation - bulk processing
        var stopwatch = Stopwatch.StartNew();
        var baselineHashes = testStrings.Select(s => StableHashBaseline(s)).ToArray();
        stopwatch.Stop();
        results["Baseline"] = (stopwatch.ElapsedTicks, baselineHashes);
        
        // LINQ Aggregate - bulk processing
        stopwatch.Restart();
        var linqAggregateHashes = testStrings.Select(s => StableHashLinqAggregate(s)).ToArray();
        stopwatch.Stop();
        results["LINQ_Aggregate"] = (stopwatch.ElapsedTicks, linqAggregateHashes);
        
        // LINQ Select - bulk processing
        stopwatch.Restart();
        var linqSelectHashes = testStrings.Select(s => StableHashLinqSelect(s)).ToArray();
        stopwatch.Stop();
        results["LINQ_Select"] = (stopwatch.ElapsedTicks, linqSelectHashes);
        
        // Verify all hashes are identical
        Assert.Equal(baselineHashes.Length, linqAggregateHashes.Length);
        Assert.Equal(baselineHashes.Length, linqSelectHashes.Length);
        for (int i = 0; i < baselineHashes.Length; i++)
        {
            Assert.Equal(baselineHashes[i], linqAggregateHashes[i]);
            Assert.Equal(baselineHashes[i], linqSelectHashes[i]);
        }
        
        // Output performance comparison
        Console.WriteLine($"Bulk Processing Performance ({testStrings.Length} strings):");
        foreach (var (method, (ticks, hashes)) in results.OrderBy(r => r.Value.ticks))
        {
            Console.WriteLine($"  {method}: {ticks} ticks ({hashes.Length} hashes)");
        }
        
        // Baseline implementation should be fastest
        var fastest = results.OrderBy(r => r.Value.ticks).First();
        Assert.Equal("Baseline", fastest.Key);
    }

    [Fact]
    public void StableHash_Collision_Test()
    {
        // Test for hash collisions with different strings
        var testStrings = GenerateTestStrings(1000, 5, 20);
        var hashes = new HashSet<int>();
        var collisions = new List<(string s1, string s2, int hash)>();
        
        foreach (var str in testStrings)
        {
            var hash = StableHashBaseline(str);
            if (!hashes.Add(hash))
            {
                // Find the collision
                var existingString = testStrings.First(s => StableHashBaseline(s) == hash && s != str);
                collisions.Add((existingString, str, hash));
            }
        }
        
        Console.WriteLine($"Hash Collision Test:");
        Console.WriteLine($"  Total strings: {testStrings.Length}");
        Console.WriteLine($"  Unique hashes: {hashes.Count}");
        Console.WriteLine($"  Collisions: {collisions.Count}");
        Console.WriteLine($"  Collision rate: {(double)collisions.Count / testStrings.Length * 100:F2}%");
        
        if (collisions.Any())
        {
            Console.WriteLine($"  Sample collisions:");
            foreach (var collision in collisions.Take(5))
            {
                Console.WriteLine($"    '{collision.s1}' and '{collision.s2}' -> {collision.hash}");
            }
        }
        
        // Collision rate should be reasonable (not too high)
        var collisionRate = (double)collisions.Count / testStrings.Length;
        Assert.True(collisionRate < 0.1, $"Collision rate {collisionRate:P} is too high");
    }

    [Fact]
    public void StableHash_Consistency_Test()
    {
        // Test that the same string always produces the same hash
        var testStrings = new[] { "hello", "world", "test", "123", "🚀", "世界" };
        
        foreach (var str in testStrings)
        {
            var hash1 = StableHashBaseline(str);
            var hash2 = StableHashBaseline(str);
            var hash3 = StableHashBaseline(str);
            
            Assert.Equal(hash1, hash2);
            Assert.Equal(hash2, hash3);
            
            // Test LINQ alternatives produce same result
            var linqHash = StableHashLinqAggregate(str);
            Assert.Equal(hash1, linqHash);
        }
        
        Console.WriteLine("Hash Consistency Test: All hashes are consistent across multiple calls");
    }

    // Baseline Implementation (original algorithm)
    private static int StableHashBaseline(string text)
    {
        unchecked
        {
            int hash = 23;
            foreach (char c in text)
                hash = (hash * 31) + c;
            return hash;
        }
    }

    // LINQ Alternative Implementations
    private static int StableHashLinqAggregate(string text)
    {
        unchecked
        {
            return text.Aggregate(23, (hash, c) => (hash * 31) + c);
        }
    }

    private static int StableHashLinqSelect(string text)
    {
        unchecked
        {
            var hash = 23;
            var chars = text.Select(c => c).ToArray();
            foreach (var c in chars)
            {
                hash = (hash * 31) + c;
            }
            return hash;
        }
    }

    private static int StableHashLinqForEach(string text)
    {
        unchecked
        {
            var hash = 23;
            text.ToList().ForEach(c => hash = (hash * 31) + c);
            return hash;
        }
    }

    // Test Data Generation
    private string[] GenerateTestStrings(int count, int minLength, int maxLength)
    {
        var strings = new string[count];
        var chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789 ";
        
        for (int i = 0; i < count; i++)
        {
            var length = _random.Next(minLength, maxLength + 1);
            var charsArray = new char[length];
            for (int j = 0; j < length; j++)
            {
                charsArray[j] = chars[_random.Next(chars.Length)];
            }
            strings[i] = new string(charsArray);
        }
        
        return strings;
    }

    private string[] GenerateUnicodeStrings(int count)
    {
        var strings = new string[count];
        var unicodeRanges = new[]
        {
            (0x0041, 0x005A), // Basic Latin
            (0x0061, 0x007A), // Basic Latin lowercase
            (0x0030, 0x0039), // Digits
            (0x4E00, 0x9FFF), // CJK Unified Ideographs
            (0x1F600, 0x1F64F), // Emoticons
            (0x1F300, 0x1F5FF), // Miscellaneous Symbols and Pictographs
        };
        
        for (int i = 0; i < count; i++)
        {
            var length = _random.Next(10, 100);
            var chars = new char[length];
            
            for (int j = 0; j < length; j++)
            {
                var range = unicodeRanges[_random.Next(unicodeRanges.Length)];
                var codePoint = _random.Next(range.Item1, range.Item2 + 1);
                chars[j] = (char)codePoint;
            }
            
            strings[i] = new string(chars);
        }
        
        return strings;
    }
} 