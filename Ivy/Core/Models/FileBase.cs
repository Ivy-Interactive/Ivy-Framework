namespace Ivy.Core.Models;

public abstract record FileBase(string FileName, string ContentType, long Length, DateTime? LastModified = null)
{
    /// <summary>Gets the name of the uploaded file including its extension.</summary>
    public string FileName { get; init; } = FileName;

    /// <summary>Gets the MIME type of the uploaded file.</summary>
    public string ContentType { get; init; } = ContentType;

    /// <summary>Gets the size of the uploaded file in bytes.</summary>
    public long Length { get; init; } = Length;

    /// <summary>Gets the date and time when the file was last modified, if available.</summary>
    public DateTime? LastModified { get; init; } = LastModified;
}
