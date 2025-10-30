namespace Ivy.Core.Models;

public abstract record FileBase
{
    protected FileBase()
    {
    }

    protected FileBase(string FileName, string ContentType, long Length)
    {
        this.FileName = FileName;
        this.ContentType = ContentType;
        this.Length = Length;
    }

    /// <summary>Gets the name of the uploaded file including its extension.</summary>
    public string? FileName { get; set; }

    /// <summary>Gets the MIME type of the uploaded file.</summary>
    public string? ContentType { get; set; }

    /// <summary>Gets the size of the uploaded file in bytes.</summary>
    public long Length { get; set; }
}
