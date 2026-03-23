using System.Text;
using Markdig;
using Xunit;

namespace Ivy.Docs.Tools.Test;

public class MarkdownConverterTests
{
    [Fact]
    public async Task Convert_NestedDetailsWithClosingBrace_DoesNotCrash()
    {
        // This test replicates a potential edge case where markdown content 
        // ends exactly at the end of a block, which might cause Substring issues
        // if Markdig spans are slightly off or if we don't check bounds.
        
        string markdown = @"# Test
<Details>
<Summary>Details</Summary>
<Body>
```csharp
var x = 1;
```
</Body>
</Details>";

        var tempFile = Path.GetTempFileName() + ".md";
        var outputFile = Path.GetTempFileName() + ".cs";
        try
        {
            await File.WriteAllTextAsync(tempFile, markdown);
            
            // Should not throw ArgumentOutOfRangeException
            await MarkdownConverter.ConvertAsync("Test", "test", tempFile, outputFile, "TestNamespace", false, 0);
            
            Assert.True(File.Exists(outputFile));
        }
        finally
        {
            if (File.Exists(tempFile)) File.Delete(tempFile);
            if (File.Exists(outputFile)) File.Delete(outputFile);
        }
    }

    [Fact]
    public async Task Convert_MarkdownWithEmoji_DoesNotCrash()
    {
        // Test with emojis to ensure surrogate pairs don't cause offset issues
        string markdown = @"# Test ❌
## Cause
```csharp
// ❌ Error here
```";

        var tempFile = Path.GetTempFileName() + ".md";
        var outputFile = Path.GetTempFileName() + ".cs";
        try
        {
            await File.WriteAllTextAsync(tempFile, markdown);
            await MarkdownConverter.ConvertAsync("Test", "test", tempFile, outputFile, "TestNamespace", false, 0);
            Assert.True(File.Exists(outputFile));
        }
        finally
        {
            if (File.Exists(tempFile)) File.Delete(tempFile);
            if (File.Exists(outputFile)) File.Delete(outputFile);
        }
    }
}
