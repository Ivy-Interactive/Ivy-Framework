namespace Ivy.Core.Models;

public abstract record FileBase
{
    protected FileBase()
    {
    }

    protected FileBase(string FileName, string ContentType, long Length, DateTime? LastModified = null)
    {
        this.FileName = FileName;
        this.ContentType = ContentType;
        this.Length = Length;
        this.LastModified = LastModified;
    }

    /// <summary>Gets the name of the uploaded file including its extension.</summary>
    public string? FileName { get; set; }

    /// <summary>Gets the MIME type of the uploaded file.</summary>
    public string? ContentType { get; set; }

    /// <summary>Gets the size of the uploaded file in bytes.</summary>
    public long Length { get; set; }

    /// <summary>Gets the date and time when the file was last modified, if available.</summary>
    public DateTime? LastModified { get; set; }
}
