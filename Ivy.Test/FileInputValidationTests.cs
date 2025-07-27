using Ivy.Widgets.Inputs;
using Ivy.Core.Hooks;

namespace Ivy.Test;

public class FileInputValidationTests
{
    [Fact]
    public void ValidateFileCount_WithNullMaxFiles_ReturnsSuccess()
    {
        // Arrange
        var files = new List<FileInput>
        {
            CreateTestFile("test1.txt"),
            CreateTestFile("test2.txt"),
            CreateTestFile("test3.txt")
        };

        // Act
        var result = FileInputValidation.ValidateFileCount(files, null);

        // Assert
        Assert.True(result.IsValid);
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public void ValidateFileCount_WithValidCount_ReturnsSuccess()
    {
        // Arrange
        var files = new List<FileInput>
        {
            CreateTestFile("test1.txt"),
            CreateTestFile("test2.txt")
        };

        // Act
        var result = FileInputValidation.ValidateFileCount(files, 3);

        // Assert
        Assert.True(result.IsValid);
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public void ValidateFileCount_WithExactCount_ReturnsSuccess()
    {
        // Arrange
        var files = new List<FileInput>
        {
            CreateTestFile("test1.txt"),
            CreateTestFile("test2.txt")
        };

        // Act
        var result = FileInputValidation.ValidateFileCount(files, 2);

        // Assert
        Assert.True(result.IsValid);
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public void ValidateFileCount_WithTooManyFiles_ReturnsError()
    {
        // Arrange
        var files = new List<FileInput>
        {
            CreateTestFile("test1.txt"),
            CreateTestFile("test2.txt"),
            CreateTestFile("test3.txt")
        };

        // Act
        var result = FileInputValidation.ValidateFileCount(files, 2);

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal("Maximum 2 files allowed. 3 files selected.", result.ErrorMessage);
    }

    [Fact]
    public void ValidateFileCount_WithSingleFileLimit_ReturnsError()
    {
        // Arrange
        var files = new List<FileInput>
        {
            CreateTestFile("test1.txt"),
            CreateTestFile("test2.txt")
        };

        // Act
        var result = FileInputValidation.ValidateFileCount(files, 1);

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal("Maximum 1 file allowed. 2 files selected.", result.ErrorMessage);
    }

    [Fact]
    public void ValidateFileTypes_WithNullAccept_ReturnsSuccess()
    {
        // Arrange
        var files = new List<FileInput>
        {
            CreateTestFile("test.txt", "text/plain"),
            CreateTestFile("test.pdf", "application/pdf")
        };

        // Act
        var result = FileInputValidation.ValidateFileTypes(files, null);

        // Assert
        Assert.True(result.IsValid);
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public void ValidateFileTypes_WithEmptyAccept_ReturnsSuccess()
    {
        // Arrange
        var files = new List<FileInput>
        {
            CreateTestFile("test.txt", "text/plain"),
            CreateTestFile("test.pdf", "application/pdf")
        };

        // Act
        var result = FileInputValidation.ValidateFileTypes(files, "");

        // Assert
        Assert.True(result.IsValid);
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public void ValidateFileTypes_WithValidExtension_ReturnsSuccess()
    {
        // Arrange
        var files = new List<FileInput>
        {
            CreateTestFile("test.txt", "text/plain"),
            CreateTestFile("test2.txt", "text/plain")
        };

        // Act
        var result = FileInputValidation.ValidateFileTypes(files, ".txt");

        // Assert
        Assert.True(result.IsValid);
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public void ValidateFileTypes_WithValidExtensions_ReturnsSuccess()
    {
        // Arrange
        var files = new List<FileInput>
        {
            CreateTestFile("test.txt", "text/plain"),
            CreateTestFile("test.pdf", "application/pdf")
        };

        // Act
        var result = FileInputValidation.ValidateFileTypes(files, ".txt,.pdf");

        // Assert
        Assert.True(result.IsValid);
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public void ValidateFileTypes_WithInvalidExtension_ReturnsError()
    {
        // Arrange
        var files = new List<FileInput>
        {
            CreateTestFile("test.txt", "text/plain"),
            CreateTestFile("test.pdf", "application/pdf")
        };

        // Act
        var result = FileInputValidation.ValidateFileTypes(files, ".txt");

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal("Invalid file type(s): test.pdf. Allowed types: .txt", result.ErrorMessage);
    }

    [Fact]
    public void ValidateFileTypes_WithMimeTypeWildcard_ReturnsSuccess()
    {
        // Arrange
        var files = new List<FileInput>
        {
            CreateTestFile("test.jpg", "image/jpeg"),
            CreateTestFile("test.png", "image/png")
        };

        // Act
        var result = FileInputValidation.ValidateFileTypes(files, "image/*");

        // Assert
        Assert.True(result.IsValid);
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public void ValidateFileTypes_WithExactMimeType_ReturnsSuccess()
    {
        // Arrange
        var files = new List<FileInput>
        {
            CreateTestFile("test.txt", "text/plain")
        };

        // Act
        var result = FileInputValidation.ValidateFileTypes(files, "text/plain");

        // Assert
        Assert.True(result.IsValid);
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public void ValidateFileTypes_WithInvalidMimeType_ReturnsError()
    {
        // Arrange
        var files = new List<FileInput>
        {
            CreateTestFile("test.txt", "text/plain"),
            CreateTestFile("test.pdf", "application/pdf")
        };

        // Act
        var result = FileInputValidation.ValidateFileTypes(files, "text/plain");

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal("Invalid file type(s): test.pdf. Allowed types: text/plain", result.ErrorMessage);
    }

    [Fact]
    public void ValidateFileType_WithValidExtension_ReturnsSuccess()
    {
        // Arrange
        var file = CreateTestFile("test.txt", "text/plain");

        // Act
        var result = FileInputValidation.ValidateFileType(file, ".txt");

        // Assert
        Assert.True(result.IsValid);
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public void ValidateFileType_WithInvalidExtension_ReturnsError()
    {
        // Arrange
        var file = CreateTestFile("test.pdf", "application/pdf");

        // Act
        var result = FileInputValidation.ValidateFileType(file, ".txt");

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal("Invalid file type: test.pdf. Allowed types: .txt", result.ErrorMessage);
    }

    [Fact]
    public void ValidateFileType_WithExtensionWithoutDot_ReturnsSuccess()
    {
        // Arrange
        var file = CreateTestFile("test.txt", "text/plain");

        // Act
        var result = FileInputValidation.ValidateFileType(file, "txt");

        // Assert
        Assert.True(result.IsValid);
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public void ValidateFileType_WithExtensionWithoutDot_ReturnsError()
    {
        // Arrange
        var file = CreateTestFile("test.pdf", "application/pdf");

        // Act
        var result = FileInputValidation.ValidateFileType(file, "txt");

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal("Invalid file type: test.pdf. Allowed types: txt", result.ErrorMessage);
    }

    [Fact]
    public void ValidateFileType_WithFileWithoutExtension_ReturnsError()
    {
        // Arrange
        var file = CreateTestFile("testfile", "text/plain");

        // Act
        var result = FileInputValidation.ValidateFileType(file, ".txt");

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal("Invalid file type: testfile. Allowed types: .txt", result.ErrorMessage);
    }

    private static FileInput CreateTestFile(string name, string type = "text/plain")
    {
        return new FileInput
        {
            Name = name,
            Type = type,
            Size = 1024,
            LastModified = DateTime.Now,
            Content = null
        };
    }

    [Fact]
    public void FileInput_ValidateValue_WithNullValue_ReturnsSuccess()
    {
        // Arrange
        var fileInput = new FileInput<FileInput?>(null, null, "Test");

        // Act
        var result = fileInput.ValidateValue(null);

        // Assert
        Assert.True(result.IsValid);
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public void FileInput_ValidateValue_WithValidSingleFile_ReturnsSuccess()
    {
        // Arrange
        var file = CreateTestFile("test.txt", "text/plain");
        var fileInput = new FileInput<FileInput?>(null, null, "Test") with { Accept = ".txt" };

        // Act
        var result = fileInput.ValidateValue(file);

        // Assert
        Assert.True(result.IsValid);
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public void FileInput_ValidateValue_WithInvalidSingleFile_ReturnsError()
    {
        // Arrange
        var file = CreateTestFile("test.pdf", "application/pdf");
        var fileInput = new FileInput<FileInput?>(null, null, "Test") with { Accept = ".txt" };

        // Act
        var result = fileInput.ValidateValue(file);

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal("Invalid file type: test.pdf. Allowed types: .txt", result.ErrorMessage);
    }

    [Fact]
    public void FileInput_ValidateValue_WithValidMultipleFiles_ReturnsSuccess()
    {
        // Arrange
        var files = new List<FileInput>
        {
            CreateTestFile("test1.txt", "text/plain"),
            CreateTestFile("test2.txt", "text/plain")
        };
        var fileInput = new FileInput<IEnumerable<FileInput>?>(null, null, "Test") with { Accept = ".txt", MaxFiles = 3 };

        // Act
        var result = fileInput.ValidateValue(files);

        // Assert
        Assert.True(result.IsValid);
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public void FileInput_ValidateValue_WithTooManyFiles_ReturnsError()
    {
        // Arrange
        var files = new List<FileInput>
        {
            CreateTestFile("test1.txt", "text/plain"),
            CreateTestFile("test2.txt", "text/plain"),
            CreateTestFile("test3.txt", "text/plain")
        };
        var fileInput = new FileInput<IEnumerable<FileInput>?>(null, null, "Test") with { Accept = ".txt", MaxFiles = 2 };

        // Act
        var result = fileInput.ValidateValue(files);

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal("Maximum 2 files allowed. 3 files selected.", result.ErrorMessage);
    }

    [Fact]
    public void FileInput_ValidateValue_WithInvalidFileTypes_ReturnsError()
    {
        // Arrange
        var files = new List<FileInput>
        {
            CreateTestFile("test1.txt", "text/plain"),
            CreateTestFile("test2.pdf", "application/pdf")
        };
        var fileInput = new FileInput<IEnumerable<FileInput>?>(null, null, "Test") with { Accept = ".txt", MaxFiles = 3 };

        // Act
        var result = fileInput.ValidateValue(files);

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal("Invalid file type(s): test2.pdf. Allowed types: .txt", result.ErrorMessage);
    }

    [Fact]
    public void FileInput_ValidateValue_WithMimeTypeWildcard_ReturnsSuccess()
    {
        // Arrange
        var files = new List<FileInput>
        {
            CreateTestFile("test1.jpg", "image/jpeg"),
            CreateTestFile("test2.png", "image/png")
        };
        var fileInput = new FileInput<IEnumerable<FileInput>?>(null, null, "Test") with { Accept = "image/*" };

        // Act
        var result = fileInput.ValidateValue(files);

        // Assert
        Assert.True(result.IsValid);
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public void FileInput_ValidateValue_WithNoAcceptOrMaxFiles_ReturnsSuccess()
    {
        // Arrange
        var files = new List<FileInput>
        {
            CreateTestFile("test1.txt", "text/plain"),
            CreateTestFile("test2.pdf", "application/pdf")
        };
        var fileInput = new FileInput<IEnumerable<FileInput>?>(null, null, "Test");

        // Act
        var result = fileInput.ValidateValue(files);

        // Assert
        Assert.True(result.IsValid);
        Assert.Null(result.ErrorMessage);
    }
}