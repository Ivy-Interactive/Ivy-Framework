using System.Text;
using Ivy.Tendril.Services;

namespace Ivy.Tendril.Test;

public class PlanPdfServiceTests
{
    [Fact]
    public void GeneratePdf_ShouldReturnNonEmptyBytes()
    {
        // Arrange
        var service = new PlanPdfService();
        var title = "Test Plan";
        var planId = 1;
        var markdown = "# Test\n\nThis is a test plan.";

        // Act
        var result = service.GeneratePdf(title, planId, markdown);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public void GeneratePdf_ShouldReturnValidPdfHeader()
    {
        // Arrange
        var service = new PlanPdfService();
        var title = "Test Plan";
        var planId = 1;
        var markdown = "# Test\n\nThis is a test plan.";

        // Act
        var result = service.GeneratePdf(title, planId, markdown);

        // Assert - PDF files start with "%PDF"
        Assert.True(result.Length >= 4, "PDF should be at least 4 bytes");
        var header = Encoding.ASCII.GetString(result, 0, 4);
        Assert.Equal("%PDF", header);
    }

    [Fact]
    public void GeneratePdf_ShouldHandleEmptyMarkdown()
    {
        // Arrange
        var service = new PlanPdfService();
        var title = "Empty Plan";
        var planId = 1;
        var markdown = "";

        // Act
        var result = service.GeneratePdf(title, planId, markdown);

        // Assert - Should not throw and should produce valid PDF
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        var header = Encoding.ASCII.GetString(result, 0, 4);
        Assert.Equal("%PDF", header);
    }
}