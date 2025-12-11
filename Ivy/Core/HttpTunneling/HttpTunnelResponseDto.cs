namespace Ivy.Core.HttpTunneling;

public class HttpTunnelResponseDto
{
    public string RequestId { get; set; } = null!;
    public int StatusCode { get; set; }
    public Dictionary<string, string[]>? Headers { get; set; }
    public string? Body { get; set; }
    public string? ContentType { get; set; }
    public string? ErrorMessage { get; set; }
}
