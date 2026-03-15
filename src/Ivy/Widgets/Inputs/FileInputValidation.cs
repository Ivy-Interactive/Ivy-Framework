using System.Text.RegularExpressions;

// ReSharper disable once CheckNamespace
namespace Ivy;

public static class FileInputValidation
{
    public static ValidationResult ValidateFileCount(IEnumerable<IFileUpload> files, int? maxFiles)
    {
        if (maxFiles == null) return ValidationResult.Success();

        var fileCount = files.Count();
        if (fileCount > maxFiles.Value)
        {
            return ValidationResult.Error($"Maximum {maxFiles.Value} file{(maxFiles.Value == 1 ? "" : "s")} allowed. {fileCount} file{(fileCount == 1 ? "" : "s")} selected.");
        }

        return ValidationResult.Success();
    }

    public static ValidationResult ValidateFileTypes(IEnumerable<IFileUpload> files, string? accept)
    {
        if (string.IsNullOrWhiteSpace(accept)) return ValidationResult.Success();

        var allowedPatterns = ParseAcceptPattern(accept);
        var invalidFiles = new List<string>();

        foreach (var file in files)
        {
            if (!IsFileTypeAllowed(file, allowedPatterns))
            {
                invalidFiles.Add(file.FileName ?? "unknown");
            }
        }

        if (invalidFiles.Any())
        {
            var fileList = string.Join(", ", invalidFiles);
            return ValidationResult.Error($"Invalid file type(s): {fileList}. Allowed types: {accept}");
        }

        return ValidationResult.Success();
    }

    public static ValidationResult ValidateFileType(IFileUpload file, string? accept)
    {
        if (string.IsNullOrWhiteSpace(accept)) return ValidationResult.Success();

        var allowedPatterns = ParseAcceptPattern(accept);

        if (!IsFileTypeAllowed(file, allowedPatterns))
        {
            return ValidationResult.Error($"Invalid file type: {file.FileName}. Allowed types: {accept}");
        }

        return ValidationResult.Success();
    }

    public static ValidationResult ValidateFileSize(IFileUpload file, long? maxFileSize)
    {
        if (maxFileSize == null) return ValidationResult.Success();

        if (file.Length > maxFileSize.Value)
        {
            var maxSizeFormatted = StringHelper.FormatBytes(maxFileSize.Value);
            var fileSizeFormatted = StringHelper.FormatBytes(file.Length);
            return ValidationResult.Error($"File '{file.FileName}' is too large ({fileSizeFormatted}). Maximum allowed size is {maxSizeFormatted}.");
        }

        return ValidationResult.Success();
    }

    public static ValidationResult ValidateMinFileSize(IFileUpload file, long? minFileSize)
    {
        if (minFileSize == null) return ValidationResult.Success();

        if (file.Length < minFileSize.Value)
        {
            var minSizeFormatted = StringHelper.FormatBytes(minFileSize.Value);
            var fileSizeFormatted = StringHelper.FormatBytes(file.Length);
            return ValidationResult.Error($"File '{file.FileName}' is too small ({fileSizeFormatted}). Minimum required size is {minSizeFormatted}.");
        }

        return ValidationResult.Success();
    }

    public static ValidationResult ValidateFileTypeWithMagicBytes(IFileUpload file, string? accept, Stream? stream)
    {
        if (string.IsNullOrWhiteSpace(accept)) return ValidationResult.Success();

        // First validate with existing ContentType check
        var typeValidation = ValidateFileType(file, accept);
        if (!typeValidation.IsValid) return typeValidation;

        // If stream is provided and contentType is known, validate magic bytes
        if (stream != null && !string.IsNullOrWhiteSpace(file.ContentType))
        {
            if (!ValidateMagicBytes(stream, file.ContentType))
            {
                return ValidationResult.Error($"File '{file.FileName}' failed security validation. The file content doesn't match the declared type.");
            }
        }

        return ValidationResult.Success();
    }

    public static ValidationResult ValidateFileTypesWithMagicBytes(IEnumerable<IFileUpload> files, string? accept, Func<IFileUpload, Stream?>? streamProvider)
    {
        if (string.IsNullOrWhiteSpace(accept)) return ValidationResult.Success();

        var invalidFiles = new List<string>();

        foreach (var file in files)
        {
            var stream = streamProvider?.Invoke(file);
            var validation = ValidateFileTypeWithMagicBytes(file, accept, stream);
            if (!validation.IsValid)
            {
                invalidFiles.Add(file.FileName ?? "unknown");
            }
        }

        if (invalidFiles.Any())
        {
            var fileList = string.Join(", ", invalidFiles);
            return ValidationResult.Error($"Invalid file type(s): {fileList}. Allowed types: {accept}");
        }

        return ValidationResult.Success();
    }

    private static List<string> ParseAcceptPattern(string accept)
    {
        return accept.Split(',')
                    .Select(s => s.Trim())
                    .Where(s => !string.IsNullOrEmpty(s))
                    .ToList();
    }

    private static bool IsFileTypeAllowed(IFileUpload file, List<string> allowedPatterns)
    {
        foreach (var pattern in allowedPatterns)
        {
            if (IsFileTypeMatch(file, pattern))
            {
                return true;
            }
        }
        return false;
    }

    private static bool IsFileTypeMatch(IFileUpload file, string pattern)
    {
        if (pattern == "*/*" || pattern == "*")
        {
            return true;
        }

        if (pattern.Contains("/"))
        {
            if (pattern.EndsWith("/*"))
            {
                var baseType = pattern[..^2];
                return file.ContentType?.StartsWith(baseType, StringComparison.OrdinalIgnoreCase) ?? false;
            }
            else
            {
                return string.Equals(file.ContentType, pattern, StringComparison.OrdinalIgnoreCase);
            }
        }

        if (pattern.StartsWith("."))
        {
            var fileExtension = Path.GetExtension(file.FileName);
            return string.Equals(fileExtension, pattern, StringComparison.OrdinalIgnoreCase);
        }

        var extension = Path.GetExtension(file.FileName);
        if (!string.IsNullOrEmpty(extension))
        {
            extension = extension[1..];
            return string.Equals(extension, pattern, StringComparison.OrdinalIgnoreCase);
        }

        return false;
    }

    private static bool ValidateMagicBytes(Stream stream, string contentType)
    {
        // Store original position
        var originalPosition = stream.Position;
        stream.Position = 0;

        try
        {
            // Read first 12 bytes (enough for most signatures)
            var buffer = new byte[12];
            var bytesRead = stream.Read(buffer, 0, buffer.Length);

            if (bytesRead < 2) return false; // Need at least 2 bytes

            // Check magic bytes based on contentType
            return contentType.ToLowerInvariant() switch
            {
                // Images
                "image/jpeg" or "image/jpg" or "image/pjpeg" =>
                    buffer[0] == 0xFF && buffer[1] == 0xD8 && buffer[2] == 0xFF,

                "image/png" or "image/x-png" =>
                    buffer[0] == 0x89 && buffer[1] == 0x50 && buffer[2] == 0x4E && buffer[3] == 0x47,

                "image/gif" =>
                    buffer[0] == 0x47 && buffer[1] == 0x49 && buffer[2] == 0x46 && buffer[3] == 0x38,

                "image/bmp" or "image/x-bmp" or "image/x-ms-bmp" =>
                    buffer[0] == 0x42 && buffer[1] == 0x4D,

                "image/webp" =>
                    buffer[0] == 0x52 && buffer[1] == 0x49 && buffer[2] == 0x46 && buffer[3] == 0x46 &&
                    buffer[8] == 0x57 && buffer[9] == 0x45 && buffer[10] == 0x42 && buffer[11] == 0x50,

                "image/tiff" or "image/x-tiff" =>
                    (buffer[0] == 0x49 && buffer[1] == 0x49 && buffer[2] == 0x2A && buffer[3] == 0x00) || // Little-endian
                    (buffer[0] == 0x4D && buffer[1] == 0x4D && buffer[2] == 0x00 && buffer[3] == 0x2A),   // Big-endian

                "image/vnd.microsoft.icon" or "image/x-icon" =>
                    buffer[0] == 0x00 && buffer[1] == 0x00 && buffer[2] == 0x01 && buffer[3] == 0x00,

                // Documents
                "application/pdf" =>
                    buffer[0] == 0x25 && buffer[1] == 0x50 && buffer[2] == 0x44 && buffer[3] == 0x46,

                "application/vnd.openxmlformats-officedocument.wordprocessingml.document" or // .docx
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" or       // .xlsx
                "application/vnd.openxmlformats-officedocument.presentationml.presentation"  // .pptx
                    => buffer[0] == 0x50 && buffer[1] == 0x4B && buffer[2] == 0x03 && buffer[3] == 0x04, // ZIP-based Office formats

                "application/msword" => // .doc
                    buffer[0] == 0xD0 && buffer[1] == 0xCF && buffer[2] == 0x11 && buffer[3] == 0xE0,

                "application/vnd.ms-excel" => // .xls
                    buffer[0] == 0xD0 && buffer[1] == 0xCF && buffer[2] == 0x11 && buffer[3] == 0xE0,

                // Archives
                "application/zip" or "application/x-zip-compressed" or "application/x-zip" =>
                    buffer[0] == 0x50 && buffer[1] == 0x4B && buffer[2] == 0x03 && buffer[3] == 0x04,

                "application/x-rar-compressed" or "application/vnd.rar" =>
                    buffer[0] == 0x52 && buffer[1] == 0x61 && buffer[2] == 0x72 && buffer[3] == 0x21,

                "application/x-7z-compressed" =>
                    buffer[0] == 0x37 && buffer[1] == 0x7A && buffer[2] == 0xBC && buffer[3] == 0xAF,

                "application/gzip" or "application/x-gzip" =>
                    buffer[0] == 0x1F && buffer[1] == 0x8B,

                "application/x-tar" =>
                    bytesRead >= 262 && // Need at least 262 bytes for tar header
                    buffer[257] == 0x75 && buffer[258] == 0x73 && buffer[259] == 0x74 && buffer[260] == 0x61 && buffer[261] == 0x72, // "ustar" at offset 257

                // Audio
                "audio/mpeg" or "audio/mp3" =>
                    (buffer[0] == 0xFF && (buffer[1] & 0xE0) == 0xE0) || // MP3 frame header
                    (buffer[0] == 0x49 && buffer[1] == 0x44 && buffer[2] == 0x33), // ID3 tag

                "audio/wav" or "audio/x-wav" or "audio/vnd.wave" =>
                    buffer[0] == 0x52 && buffer[1] == 0x49 && buffer[2] == 0x46 && buffer[3] == 0x46 &&
                    buffer[8] == 0x57 && buffer[9] == 0x41 && buffer[10] == 0x56 && buffer[11] == 0x45,

                "audio/ogg" or "application/ogg" =>
                    buffer[0] == 0x4F && buffer[1] == 0x67 && buffer[2] == 0x67 && buffer[3] == 0x53,

                // Video
                "video/mp4" or "video/quicktime" =>
                    buffer[4] == 0x66 && buffer[5] == 0x74 && buffer[6] == 0x79 && buffer[7] == 0x70, // "ftyp" at offset 4

                "video/webm" =>
                    buffer[0] == 0x1A && buffer[1] == 0x45 && buffer[2] == 0xDF && buffer[3] == 0xA3,

                "video/x-msvideo" or "video/avi" =>
                    buffer[0] == 0x52 && buffer[1] == 0x49 && buffer[2] == 0x46 && buffer[3] == 0x46 &&
                    buffer[8] == 0x41 && buffer[9] == 0x56 && buffer[10] == 0x49 && buffer[11] == 0x20,

                // Text formats (most don't have magic bytes, so we allow them)
                "text/plain" or "text/csv" or "application/csv" or
                "application/json" or "text/json" or
                "application/xml" or "text/xml" or
                "image/svg+xml" => true,

                // Allow unknown types (don't validate magic bytes if we don't know the signature)
                _ => true
            };
        }
        finally
        {
            // Restore original position
            stream.Position = originalPosition;
        }
    }
}

public record ValidationResult
{
    public bool IsValid { get; }
    public string? ErrorMessage { get; }

    private ValidationResult(bool isValid, string? errorMessage = null)
    {
        IsValid = isValid;
        ErrorMessage = errorMessage;
    }

    public static ValidationResult Success() => new(true);
    public static ValidationResult Error(string message) => new(false, message);
}
