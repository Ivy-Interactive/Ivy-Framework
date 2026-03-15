using Ivy.Core.Hooks;

namespace Ivy.Test;

public class FileInputValidationTests
{
    [Fact]
    public void ValidateFileCount_WithNullMaxFiles_ReturnsSuccess()
    {
        // Arrange
        var files = new List<FileUpload>
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
        var files = new List<FileUpload>
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
        var files = new List<FileUpload>
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
        var files = new List<FileUpload>
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
        var files = new List<FileUpload>
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
        var files = new List<FileUpload>
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
        var files = new List<FileUpload>
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
        var files = new List<FileUpload>
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
        var files = new List<FileUpload>
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
        var files = new List<FileUpload>
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
        var files = new List<FileUpload>
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
        var files = new List<FileUpload>
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
        var files = new List<FileUpload>
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

    private static FileUpload CreateTestFile(string name, string type = "text/plain", long length = 12345)
    {
        return new FileUpload { FileName = name, ContentType = type, Length = length };
    }

    [Fact]
    public void ValidateMinFileSize_WithNullMinFileSize_ReturnsSuccess()
    {
        // Arrange
        var file = CreateTestFile("test.txt", "text/plain", 100);

        // Act
        var result = FileInputValidation.ValidateMinFileSize(file, null);

        // Assert
        Assert.True(result.IsValid);
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public void ValidateMinFileSize_WithFileLargerThanMin_ReturnsSuccess()
    {
        // Arrange
        var file = CreateTestFile("test.txt", "text/plain", 2048);

        // Act
        var result = FileInputValidation.ValidateMinFileSize(file, 1024);

        // Assert
        Assert.True(result.IsValid);
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public void ValidateMinFileSize_WithFileEqualToMin_ReturnsSuccess()
    {
        // Arrange
        var file = CreateTestFile("test.txt", "text/plain", 1024);

        // Act
        var result = FileInputValidation.ValidateMinFileSize(file, 1024);

        // Assert
        Assert.True(result.IsValid);
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public void ValidateMinFileSize_WithFileSmallerThanMin_ReturnsError()
    {
        // Arrange
        var file = CreateTestFile("test.txt", "text/plain", 512);

        // Act
        var result = FileInputValidation.ValidateMinFileSize(file, 1024);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("too small", result.ErrorMessage);
        Assert.Contains("test.txt", result.ErrorMessage);
    }

    [Fact]
    public void ValidateMinFileSize_WithEmptyFile_ReturnsError()
    {
        // Arrange
        var file = CreateTestFile("empty.txt", "text/plain", 0);

        // Act
        var result = FileInputValidation.ValidateMinFileSize(file, 1);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("too small", result.ErrorMessage);
    }

    [Fact]
    public void FileInput_ValidateValue_WithNullValue_ReturnsSuccess()
    {
        // Arrange
        var fileInput = new FileInput<FileUpload?>((FileUpload?)null, "Test");

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
        var fileInput = new FileInput<FileUpload?>((FileUpload?)null, "Test") with { Accept = ".txt" };

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
        var fileInput = new FileInput<FileUpload?>((FileUpload?)null, "Test") with { Accept = ".txt" };

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
        var files = new List<FileUpload>
        {
            CreateTestFile("test1.txt", "text/plain"),
            CreateTestFile("test2.txt", "text/plain")
        };
        var fileInput = new FileInput<IEnumerable<FileUpload>?>((IEnumerable<FileUpload>?)null, "Test") with { Accept = ".txt", MaxFiles = 3 };

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
        var files = new List<FileUpload>
        {
            CreateTestFile("test1.txt", "text/plain"),
            CreateTestFile("test2.txt", "text/plain"),
            CreateTestFile("test3.txt", "text/plain")
        };
        var fileInput = new FileInput<IEnumerable<FileUpload>?>((IEnumerable<FileUpload>?)null, "Test") with { Accept = ".txt", MaxFiles = 2 };

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
        var files = new List<FileUpload>
        {
            CreateTestFile("test1.txt", "text/plain"),
            CreateTestFile("test2.pdf", "application/pdf")
        };
        var fileInput = new FileInput<IEnumerable<FileUpload>?>((IEnumerable<FileUpload>?)null, "Test") with { Accept = ".txt", MaxFiles = 3 };

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
        var files = new List<FileUpload>
        {
            CreateTestFile("test1.jpg", "image/jpeg"),
            CreateTestFile("test2.png", "image/png")
        };
        var fileInput = new FileInput<IEnumerable<FileUpload>?>((IEnumerable<FileUpload>?)null, "Test") with { Accept = "image/*" };

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
        var files = new List<FileUpload>
        {
            CreateTestFile("test1.txt", "text/plain"),
            CreateTestFile("test2.pdf", "application/pdf")
        };
        var fileInput = new FileInput<IEnumerable<FileUpload>?>((IEnumerable<FileUpload>?)null, "Test");

        // Act
        var result = fileInput.ValidateValue(files);

        // Assert
        Assert.True(result.IsValid);
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public void FileInput_ValidateValue_WithValidMinFileSize_ReturnsSuccess()
    {
        // Arrange
        var file = CreateTestFile("test.txt", "text/plain", 2048);
        var fileInput = new FileInput<FileUpload?>((FileUpload?)null, "Test") with { MinFileSize = 1024 };

        // Act
        var result = fileInput.ValidateValue(file);

        // Assert
        Assert.True(result.IsValid);
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public void FileInput_ValidateValue_WithFileTooSmall_ReturnsError()
    {
        // Arrange
        var file = CreateTestFile("test.txt", "text/plain", 512);
        var fileInput = new FileInput<FileUpload?>((FileUpload?)null, "Test") with { MinFileSize = 1024 };

        // Act
        var result = fileInput.ValidateValue(file);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("too small", result.ErrorMessage);
    }

    [Fact]
    public void FileInput_ValidateValue_WithMultipleFilesAndMinFileSize_ReturnsSuccess()
    {
        // Arrange
        var files = new List<FileUpload>
        {
            CreateTestFile("test1.txt", "text/plain", 2048),
            CreateTestFile("test2.txt", "text/plain", 1536)
        };
        var fileInput = new FileInput<IEnumerable<FileUpload>?>((IEnumerable<FileUpload>?)null, "Test") with { MinFileSize = 1024 };

        // Act
        var result = fileInput.ValidateValue(files);

        // Assert
        Assert.True(result.IsValid);
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public void FileInput_ValidateValue_WithMultipleFilesOneFileTooSmall_ReturnsError()
    {
        // Arrange
        var files = new List<FileUpload>
        {
            CreateTestFile("test1.txt", "text/plain", 2048),
            CreateTestFile("test2.txt", "text/plain", 256)
        };
        var fileInput = new FileInput<IEnumerable<FileUpload>?>((IEnumerable<FileUpload>?)null, "Test") with { MinFileSize = 1024 };

        // Act
        var result = fileInput.ValidateValue(files);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("test2.txt", result.ErrorMessage);
        Assert.Contains("too small", result.ErrorMessage);
    }

    [Fact]
    public void FileInput_ValidateValue_WithBothMinAndMaxFileSize_ReturnsSuccess()
    {
        // Arrange
        var file = CreateTestFile("test.txt", "text/plain", 2048);
        var fileInput = new FileInput<FileUpload?>((FileUpload?)null, "Test") with { MinFileSize = 1024, MaxFileSize = 5000 };

        // Act
        var result = fileInput.ValidateValue(file);

        // Assert
        Assert.True(result.IsValid);
        Assert.Null(result.ErrorMessage);
    }

    // Magic Byte Validation Tests

    [Fact]
    public void ValidateFileTypeWithMagicBytes_ValidJpegWithMatchingContentType_ReturnsSuccess()
    {
        // Arrange
        var file = CreateTestFile("test.jpg", "image/jpeg");
        var stream = CreateJpegStream();

        // Act
        var result = FileInputValidation.ValidateFileTypeWithMagicBytes(file, "image/*", stream);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void ValidateFileTypeWithMagicBytes_SpoofedContentType_ReturnsError()
    {
        // Arrange
        var file = CreateTestFile("malicious.jpg", "image/jpeg"); // Claims to be JPEG
        var stream = CreatePdfStream(); // But is actually a PDF

        // Act
        var result = FileInputValidation.ValidateFileTypeWithMagicBytes(file, "image/*", stream);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("security validation", result.ErrorMessage);
    }

    [Fact]
    public void ValidateFileTypeWithMagicBytes_NullStream_FallsBackToContentTypeOnly()
    {
        // Arrange
        var file = CreateTestFile("test.jpg", "image/jpeg");

        // Act
        var result = FileInputValidation.ValidateFileTypeWithMagicBytes(file, "image/*", null);

        // Assert
        Assert.True(result.IsValid); // Should pass with ContentType validation only
    }

    [Fact]
    public void ValidateFileTypeWithMagicBytes_AlternativeMimeType_ValidatesPng()
    {
        // Arrange
        var file = CreateTestFile("test.png", "image/x-png"); // Alternative MIME type
        var stream = CreatePngStream();

        // Act
        var result = FileInputValidation.ValidateFileTypeWithMagicBytes(file, "image/*", stream);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void ValidateFileTypeWithMagicBytes_OfficeDocument_ValidatesDocx()
    {
        // Arrange
        var file = CreateTestFile("document.docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document");
        var stream = CreateZipBasedStream(); // Office formats are ZIP-based

        // Act
        var result = FileInputValidation.ValidateFileTypeWithMagicBytes(file, ".docx", stream);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void ValidateFileTypeWithMagicBytes_ValidWebP_ReturnsSuccess()
    {
        // Arrange
        var file = CreateTestFile("test.webp", "image/webp");
        var stream = CreateWebPStream();

        // Act
        var result = FileInputValidation.ValidateFileTypeWithMagicBytes(file, "image/*", stream);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void ValidateFileTypeWithMagicBytes_InvalidContentType_ReturnsError()
    {
        // Arrange
        var file = CreateTestFile("test.jpg", "image/jpeg");
        var stream = CreateJpegStream();

        // Act - try to validate against wrong accept pattern
        var result = FileInputValidation.ValidateFileTypeWithMagicBytes(file, ".pdf", stream);

        // Assert
        Assert.False(result.IsValid);
    }

    [Fact]
    public void ValidateFileTypeWithMagicBytes_TextFile_SkipsMagicByteCheck()
    {
        // Arrange - text files don't have magic bytes, so they should pass
        var file = CreateTestFile("test.txt", "text/plain");
        var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("Hello World"));
        stream.Position = 0;

        // Act
        var result = FileInputValidation.ValidateFileTypeWithMagicBytes(file, "text/plain", stream);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void ValidateFileTypesWithMagicBytes_AllValid_ReturnsSuccess()
    {
        // Arrange
        var files = new List<FileUpload>
        {
            CreateTestFile("test1.jpg", "image/jpeg"),
            CreateTestFile("test2.png", "image/png")
        };

        Stream? StreamProvider(IFileUpload file)
        {
            return file.FileName switch
            {
                "test1.jpg" => CreateJpegStream(),
                "test2.png" => CreatePngStream(),
                _ => null
            };
        }

        // Act
        var result = FileInputValidation.ValidateFileTypesWithMagicBytes(files, "image/*", StreamProvider);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void ValidateFileTypesWithMagicBytes_OneSpoofed_ReturnsError()
    {
        // Arrange
        var files = new List<FileUpload>
        {
            CreateTestFile("test1.jpg", "image/jpeg"),
            CreateTestFile("malicious.jpg", "image/jpeg") // Spoofed
        };

        Stream? StreamProvider(IFileUpload file)
        {
            return file.FileName switch
            {
                "test1.jpg" => CreateJpegStream(),
                "malicious.jpg" => CreatePdfStream(), // Actually a PDF
                _ => null
            };
        }

        // Act
        var result = FileInputValidation.ValidateFileTypesWithMagicBytes(files, "image/*", StreamProvider);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("malicious.jpg", result.ErrorMessage);
    }

    // Helper methods to create test streams

    private static MemoryStream CreateJpegStream()
    {
        var stream = new MemoryStream();
        stream.Write(new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 }, 0, 4);
        stream.Position = 0;
        return stream;
    }

    private static MemoryStream CreatePdfStream()
    {
        var stream = new MemoryStream();
        stream.Write(new byte[] { 0x25, 0x50, 0x44, 0x46 }, 0, 4);
        stream.Position = 0;
        return stream;
    }

    private static MemoryStream CreatePngStream()
    {
        var stream = new MemoryStream();
        stream.Write(new byte[] { 0x89, 0x50, 0x4E, 0x47 }, 0, 4);
        stream.Position = 0;
        return stream;
    }

    private static MemoryStream CreateZipBasedStream()
    {
        var stream = new MemoryStream();
        stream.Write(new byte[] { 0x50, 0x4B, 0x03, 0x04 }, 0, 4);
        stream.Position = 0;
        return stream;
    }

    private static MemoryStream CreateWebPStream()
    {
        var stream = new MemoryStream();
        stream.Write(new byte[] { 0x52, 0x49, 0x46, 0x46, 0x00, 0x00, 0x00, 0x00, 0x57, 0x45, 0x42, 0x50 }, 0, 12);
        stream.Position = 0;
        return stream;
    }
}
