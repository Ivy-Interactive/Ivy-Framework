using System.Text.RegularExpressions;

namespace Ivy.Widgets.Inputs;

public static class FileInputValidation
{
    /// <summary>
    /// Validates file count for multiple file inputs
    /// </summary>
    /// <param name="files">The files to validate</param>
    /// <param name="maxFiles">Maximum number of files allowed</param>
    /// <returns>Validation result with error message if validation fails</returns>
    public static ValidationResult ValidateFileCount(IEnumerable<FileInput> files, int? maxFiles)
    {
        if (maxFiles == null) return ValidationResult.Success();

        var fileCount = files.Count();
        if (fileCount > maxFiles.Value)
        {
            return ValidationResult.Error($"Maximum {maxFiles.Value} file{(maxFiles.Value == 1 ? "" : "s")} allowed. {fileCount} file{(fileCount == 1 ? "" : "s")} selected.");
        }

        return ValidationResult.Success();
    }

    /// <summary>
    /// Validates file types based on the accept pattern
    /// </summary>
    /// <param name="files">The files to validate</param>
    /// <param name="accept">The accept pattern (e.g., ".txt,.pdf" or "image/*")</param>
    /// <returns>Validation result with error message if validation fails</returns>
    public static ValidationResult ValidateFileTypes(IEnumerable<FileInput> files, string? accept)
    {
        if (string.IsNullOrWhiteSpace(accept)) return ValidationResult.Success();

        var allowedPatterns = ParseAcceptPattern(accept);
        var invalidFiles = new List<string>();

        foreach (var file in files)
        {
            if (!IsFileTypeAllowed(file, allowedPatterns))
            {
                invalidFiles.Add(file.Name);
            }
        }

        if (invalidFiles.Any())
        {
            var fileList = string.Join(", ", invalidFiles);
            return ValidationResult.Error($"Invalid file type(s): {fileList}. Allowed types: {accept}");
        }

        return ValidationResult.Success();
    }

    /// <summary>
    /// Validates a single file's type
    /// </summary>
    /// <param name="file">The file to validate</param>
    /// <param name="accept">The accept pattern</param>
    /// <returns>Validation result with error message if validation fails</returns>
    public static ValidationResult ValidateFileType(FileInput file, string? accept)
    {
        if (string.IsNullOrWhiteSpace(accept)) return ValidationResult.Success();

        var allowedPatterns = ParseAcceptPattern(accept);

        if (!IsFileTypeAllowed(file, allowedPatterns))
        {
            return ValidationResult.Error($"Invalid file type: {file.Name}. Allowed types: {accept}");
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

    private static bool IsFileTypeAllowed(FileInput file, List<string> allowedPatterns)
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

    private static bool IsFileTypeMatch(FileInput file, string pattern)
    {
        // Handle MIME type patterns (e.g., "image/*", "text/plain")
        if (pattern.Contains("/"))
        {
            if (pattern.EndsWith("/*"))
            {
                // Wildcard MIME type (e.g., "image/*")
                var baseType = pattern[..^2];
                return file.Type.StartsWith(baseType, StringComparison.OrdinalIgnoreCase);
            }
            else
            {
                // Exact MIME type (e.g., "text/plain")
                return string.Equals(file.Type, pattern, StringComparison.OrdinalIgnoreCase);
            }
        }

        // Handle file extension patterns (e.g., ".txt", ".pdf")
        if (pattern.StartsWith("."))
        {
            var fileExtension = Path.GetExtension(file.Name);
            return string.Equals(fileExtension, pattern, StringComparison.OrdinalIgnoreCase);
        }

        // Handle extension without dot (e.g., "txt", "pdf")
        var extension = Path.GetExtension(file.Name);
        if (!string.IsNullOrEmpty(extension))
        {
            extension = extension[1..]; // Remove the dot
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