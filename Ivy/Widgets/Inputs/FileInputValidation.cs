using Ivy.Services;
using System.Text.RegularExpressions;

namespace Ivy.Widgets.Inputs;

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
            var maxSizeFormatted = Utils.FormatBytes(maxFileSize.Value);
            var fileSizeFormatted = Utils.FormatBytes(file.Length);
            return ValidationResult.Error($"File '{file.FileName}' is too large ({fileSizeFormatted}). Maximum allowed size is {maxSizeFormatted}.");
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
