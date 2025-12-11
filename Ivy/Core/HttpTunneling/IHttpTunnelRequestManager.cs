namespace Ivy.Core.HttpTunneling;

public interface IHttpTunnelRequestManager
{
    bool TryCompleteRequest(string requestId, HttpTunnelResponseDto response);
    void CancelAllPendingRequests(string reason);
}
