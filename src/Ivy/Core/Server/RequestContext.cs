namespace Ivy.Core.Server;

public class RequestContext
{
    private string? _scheme;
    private string? _host;
    private readonly Lock _lock = new();

    public string? Scheme
    {
        get
        {
            lock (_lock)
            {
                return _scheme;
            }
        }
    }

    public string? Host
    {
        get
        {
            lock (_lock)
            {
                return _host;
            }
        }
    }

    public bool IsInitialized => _scheme != null && _host != null;

    public void Initialize(string scheme, string host)
    {
        lock (_lock)
        {
            if (_scheme == null && _host == null)
            {
                _scheme = scheme;
                _host = host;
            }
        }
    }

    public (string Scheme, string Host) GetRequired()
    {
        lock (_lock)
        {
            if (_scheme == null || _host == null)
            {
                throw new InvalidOperationException("RequestContext has not been initialized yet. Ensure the server has received at least one request.");
            }
            return (_scheme, _host);
        }
    }
}
