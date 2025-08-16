# Services

<Ingress>
Services in Ivy provide dependency injection and service management for clean application architecture.
</Ingress>

## Overview

The service system in Ivy supports:

- Dependency injection
- Service registration and configuration
- Service lifecycle management
- Scoped and singleton services
- Service interfaces and implementations
- Service middleware

## Basic Usage

Here's a simple example of using a service:

```csharp
public class MyView : ViewBase
{
    public override object? Build()
    {
        var myService = this.UseService<IMyService>();
        return myService.GetData();
    }
}
```

### Service Registration

Register services in your application startup:

```csharp
public class Program
{
    public static void Main()
    {
        var server = new Server()
            .UseService<IMyService, MyService>()
            .UseService<IDataService, DataService>(ServiceLifetime.Singleton)
            .UseService<IAuthService, AuthService>();
    }
}
```

### Service Interfaces

Define service interfaces for better abstraction:

```csharp
public interface IDataService
{
    Task<IEnumerable<Data>> GetDataAsync();
    Task<Data> GetDataByIdAsync(string id);
    Task SaveDataAsync(Data data);
}

public class DataService : IDataService
{
    private readonly ILogger<DataService> _logger;
    
    public DataService(ILogger<DataService> logger)
    {
        _logger = logger;
    }

    public async Task<IEnumerable<Data>> GetDataAsync()
    {
        _logger.LogInformation("Fetching data");
        // Implementation
    }

    public async Task<Data> GetDataByIdAsync(string id)
    {
        _logger.LogInformation("Fetching data for id: {Id}", id);
        // Implementation
    }

    public async Task SaveDataAsync(Data data)
    {
        _logger.LogInformation("Saving data");
        // Implementation
    }
}
```

### Service Lifetime

Ivy supports different service lifetimes:

```csharp
// Singleton - Created once for the entire application
.UseService<ICacheService, CacheService>(ServiceLifetime.Singleton)

// Scoped - Created once per request
.UseService<IDbContext, DbContext>(ServiceLifetime.Scoped)

// Transient - Created each time requested
.UseService<ILogger, Logger>(ServiceLifetime.Transient)
```

### Service Middleware

Add middleware to services for cross-cutting concerns:

```csharp
public class LoggingServiceMiddleware : IServiceMiddleware
{
    private readonly ILogger _logger;

    public LoggingServiceMiddleware(ILogger logger)
    {
        _logger = logger;
    }

    public async Task<T> ExecuteAsync<T>(Func<Task<T>> next)
    {
        _logger.LogInformation("Service method called");
        try
        {
            return await next();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Service method failed");
            throw;
        }
    }
}

// Register middleware
.UseServiceMiddleware<LoggingServiceMiddleware>()
```

### Best Practices

1. **Interface-based Design**: Always define interfaces for your services
2. **Single Responsibility**: Each service should have a single, well-defined purpose
3. **Dependency Injection**: Use constructor injection for dependencies
4. **Service Lifetime**: Choose appropriate lifetimes for your services
5. **Error Handling**: Implement proper error handling in services
6. **Logging**: Use logging for important operations and errors
7. **Testing**: Make services easily testable through interfaces

## Examples

### Authentication Service

```csharp
public interface IAuthService
{
    Task<bool> ValidateCredentialsAsync(string username, string password);
    Task<string> GenerateTokenAsync(User user);
    Task<User?> GetCurrentUserAsync();
}

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthService(
        IUserRepository userRepository,
        ITokenService tokenService,
        IHttpContextAccessor httpContextAccessor)
    {
        _userRepository = userRepository;
        _tokenService = tokenService;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<bool> ValidateCredentialsAsync(string username, string password)
    {
        var user = await _userRepository.GetByUsernameAsync(username);
        if (user == null) return false;
        
        return await _userRepository.ValidatePasswordAsync(user, password);
    }

    public async Task<string> GenerateTokenAsync(User user)
    {
        return await _tokenService.GenerateTokenAsync(user);
    }

    public async Task<User?> GetCurrentUserAsync()
    {
        var userId = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null) return null;
        
        return await _userRepository.GetByIdAsync(userId);
    }
}
```

### Caching Service

```csharp
public interface ICacheService
{
    Task<T?> GetAsync<T>(string key);
    Task SetAsync<T>(string key, T value, TimeSpan? expiry = null);
    Task RemoveAsync(string key);
}

public class CacheService : ICacheService
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<CacheService> _logger;

    public CacheService(
        IDistributedCache cache,
        ILogger<CacheService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        try
        {
            var data = await _cache.GetAsync(key);
            if (data == null) return default;
            
            return JsonSerializer.Deserialize<T>(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cache for key: {Key}", key);
            return default;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
    {
        try
        {
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiry
            };
            
            var data = JsonSerializer.SerializeToUtf8Bytes(value);
            await _cache.SetAsync(key, data, options);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting cache for key: {Key}", key);
        }
    }

    public async Task RemoveAsync(string key)
    {
        try
        {
            await _cache.RemoveAsync(key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing cache for key: {Key}", key);
        }
    }
}
```
