using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Ivy.Test;

public class LocalFileControllerTests : IDisposable
{
    private readonly string _tempDirectory;
    private readonly List<string> _tempFiles = [];

    public LocalFileControllerTests()
    {
        _tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDirectory);
    }

    public void Dispose()
    {
        foreach (var file in _tempFiles)
        {
            if (File.Exists(file))
            {
                File.Delete(file);
            }
        }

        if (Directory.Exists(_tempDirectory))
        {
            Directory.Delete(_tempDirectory, true);
        }
    }

    [Fact]
    public void GetFile_ShouldSetContentDispositionWithFilename()
    {
        // Arrange
        var fileName = "test-document.pdf";
        var filePath = CreateTempFile(fileName, "Test content");
        var controller = CreateController(allowLocalFiles: true);

        // Act
        var result = controller.GetFile(filePath) as PhysicalFileResult;

        // Assert
        Assert.NotNull(result);
        var contentDisposition = controller.Response.Headers.ContentDisposition.ToString();
        Assert.StartsWith("inline", contentDisposition);
        Assert.Contains($"filename=\"{fileName}\"", contentDisposition);
    }

    [Fact]
    public void GetFile_WithImageFile_ShouldSetInlineDisposition()
    {
        // Arrange
        var fileName = "image.png";
        var filePath = CreateTempFile(fileName, "Fake PNG content");
        var controller = CreateController(allowLocalFiles: true);

        // Act
        var result = controller.GetFile(filePath) as PhysicalFileResult;

        // Assert
        Assert.NotNull(result);
        var contentDisposition = controller.Response.Headers.ContentDisposition.ToString();
        Assert.StartsWith("inline", contentDisposition);
        Assert.Contains($"filename=\"{fileName}\"", contentDisposition);
    }

    [Fact]
    public void GetFile_WithMarkdownFile_ShouldReturnCorrectContentType()
    {
        // Arrange
        var fileName = "document.md";
        var filePath = CreateTempFile(fileName, "# Markdown content");
        var controller = CreateController(allowLocalFiles: true);

        // Act
        var result = controller.GetFile(filePath) as PhysicalFileResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal("text/markdown", result.ContentType);
    }

    [Fact]
    public void GetFile_WithWebPFile_ShouldReturnCorrectContentType()
    {
        // Arrange
        var fileName = "image.webp";
        var filePath = CreateTempFile(fileName, "Fake WebP content");
        var controller = CreateController(allowLocalFiles: true);

        // Act
        var result = controller.GetFile(filePath) as PhysicalFileResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal("image/webp", result.ContentType);
    }

    [Fact]
    public void GetFile_WithAvifFile_ShouldReturnCorrectContentType()
    {
        // Arrange
        var fileName = "image.avif";
        var filePath = CreateTempFile(fileName, "Fake AVIF content");
        var controller = CreateController(allowLocalFiles: true);

        // Act
        var result = controller.GetFile(filePath) as PhysicalFileResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal("image/avif", result.ContentType);
    }

    [Fact]
    public void GetFile_WithWebMFile_ShouldReturnCorrectContentType()
    {
        // Arrange
        var fileName = "video.webm";
        var filePath = CreateTempFile(fileName, "Fake WebM content");
        var controller = CreateController(allowLocalFiles: true);

        // Act
        var result = controller.GetFile(filePath) as PhysicalFileResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal("video/webm", result.ContentType);
    }

    [Fact]
    public void GetFile_WithJsonlFile_ShouldReturnCorrectContentType()
    {
        // Arrange
        var fileName = "data.jsonl";
        var filePath = CreateTempFile(fileName, "{\"key\":\"value\"}");
        var controller = CreateController(allowLocalFiles: true);

        // Act
        var result = controller.GetFile(filePath) as PhysicalFileResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal("application/jsonl", result.ContentType);
    }

    [Fact]
    public void GetFile_WhenLocalFilesNotAllowed_ShouldReturnNotFound()
    {
        // Arrange
        var fileName = "test.txt";
        var filePath = CreateTempFile(fileName, "Content");
        var controller = CreateController(allowLocalFiles: false);

        // Act
        var result = controller.GetFile(filePath);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public void GetFile_WhenPathIsNull_ShouldReturnBadRequest()
    {
        // Arrange
        var controller = CreateController(allowLocalFiles: true);

        // Act
        var result = controller.GetFile(null);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Path is required", badRequestResult.Value);
    }

    [Fact]
    public void GetFile_WhenPathIsEmpty_ShouldReturnBadRequest()
    {
        // Arrange
        var controller = CreateController(allowLocalFiles: true);

        // Act
        var result = controller.GetFile("");

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Path is required", badRequestResult.Value);
    }

    [Fact]
    public void GetFile_WhenFileDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        var nonExistentPath = Path.Combine(_tempDirectory, "nonexistent.txt");
        var controller = CreateController(allowLocalFiles: true);

        // Act
        var result = controller.GetFile(nonExistentPath);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public void GetFile_WithFilenameContainingSpaces_ShouldSetCorrectContentDisposition()
    {
        // Arrange
        var fileName = "my document with spaces.pdf";
        var filePath = CreateTempFile(fileName, "Content");
        var controller = CreateController(allowLocalFiles: true);

        // Act
        var result = controller.GetFile(filePath) as PhysicalFileResult;

        // Assert
        Assert.NotNull(result);
        var contentDisposition = controller.Response.Headers.ContentDisposition.ToString();
        Assert.Contains($"filename=\"{fileName}\"", contentDisposition);
    }

    private string CreateTempFile(string fileName, string content)
    {
        var filePath = Path.Combine(_tempDirectory, fileName);
        File.WriteAllText(filePath, content);
        _tempFiles.Add(filePath);
        return filePath;
    }

    private LocalFileController CreateController(bool allowLocalFiles)
    {
        var serverArgs = new ServerArgs
        {
            DangerouslyAllowLocalFiles = allowLocalFiles
        };
        var server = new Server(serverArgs);
        var controller = new LocalFileController(server);

        // Set up HTTP context for the controller
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        return controller;
    }
}
