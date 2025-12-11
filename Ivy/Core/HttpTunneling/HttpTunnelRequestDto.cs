namespace Ivy.Core.HttpTunneling;

public class HttpTunnelRequestDto
{
    public string RequestId { get; set; } = null!;
    public string Method { get; set; } = null!;
    public string Url { get; set; } = null!;
    public Dictionary<string, string[]>? Headers { get; set; }
    public string? Body { get; set; }
    public string? ContentType { get; set; }
}
