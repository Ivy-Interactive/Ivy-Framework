---
name: ivy-create-any-connection
description: >
  Create an ad-hoc API connection or service integration in an Ivy project. Supports
  NuGet package, OpenAPI/Swagger with Refitter, REST API, and custom HTTP client
  approaches. Use when the user wants to connect to an external API, add a service
  integration, or create a typed client for a REST endpoint.
allowed-tools: Bash(dotnet:*) Bash(refitter:*) Read Write Edit Glob Grep
effort: high
argument-hint: "[name or URL of the API/service to connect to]"
---

# ivy-create-any-connection

Create a connection to an external API or service inside an existing Ivy project. This produces connection files under `Connections/[ConnectionName]/` -- it does NOT create a separate project or catalog entry. Only use this skill when the user wants to connect to an API or service that does not already have a pre-built reference connection in the Ivy catalog.

## Pre-flight: Read Learnings

If the file `.ivy/learnings/ivy-create-any-connection.md` exists in the project directory, read it first and apply any lessons learned from previous runs of this skill.

## Reference Files

Read before implementing:
- [references/AGENTS.md](references/AGENTS.md) -- Ivy framework API reference (widgets, hooks, layouts, inputs, colors)

## Step 1: Validate the Project

1. Verify this is a valid Ivy project. Check for a `.csproj` file and `Program.cs` in the working directory. If this is not an Ivy project, tell the user and stop.

2. **LLM endpoint detection:** If the user's request mentions an OpenAI-compatible LLM endpoint (URL contains `openai`, `litellm`, `chat/completions`, or `v1`; or keywords like "OpenAI-compatible", "LLM proxy", "chat completions"), do NOT create an ad-hoc connection. Instead, tell the user to use the **OpenAI** reference connection, which already supports custom endpoints via the `OpenAI:Endpoint` secret. Stop and suggest using the `/ivy-create-using-reference-connection` skill with "OpenAI" as the provider.

## Step 2: Clarify the Target API

3. Is it clear what API or service the connection is for? If not, ask the user to clarify. Ask for a link to the API documentation or the name of the service. Some services have multiple APIs (e.g. Stripe has payments, billing, customers). If so, clarify which API and reflect this in the connection name.

4. Determine a name for the connection. This is usually the service name in PascalCase, like "Stripe" or "GitHub". Ask the user to confirm the name.

5. Create the connection directory using the pattern `Connections/[ConnectionName]/` inside the user's project directory.

Expected directory structure after this skill completes:

```
[UserProject]/
├── [UserProject].csproj
├── Program.cs
├── Connections/
│   └── [ConnectionName]/
│       ├── [ConnectionName]Connection.cs
│       ├── [ConnectionName]ConnectionTests.cs
│       └── (OpenAPI only: ClientFactory, .refitter, Refresh.ps1, generated client)
│       └── (Custom only: [ConnectionName]Client.cs)
└── Apps/
    └── [ConnectionName]App.cs
```

## Step 3: Choose an Approach

6. Try to find a good NuGet package that implements the API.

**GOOD indicators:**
- Lots of downloads
- Recently updated
- Permissive license (MIT, Apache-2.0, BSD, ISC, etc.)
- Good documentation
- Official, created by the service itself

**BAD indicators:**
- Few downloads
- Not updated in a long time
- Restrictive license (GPL, etc.)
- No documentation
- Not official, created by a third party

**Notes:**
- For anything related to LLM model inference, prefer packages that provide an `IChatClient` adapter via `Microsoft.Extensions.AI`. It is fine to also register the provider-specific client (e.g. `AnthropicClient`) -- but always register `IChatClient` as well if possible, since it gives a unified chat interface across all LLM connections.

Given these parameters, present the user a list of potential NuGets. The strategy for choosing the connection approach (in priority order):

1. **NuGet package** -- if a good NuGet exists, proceed to the "NuGet Approach" section
2. **OpenAPI/Swagger spec** -- if no good NuGet exists but the API has an official OpenAPI spec, use Refitter to generate a typed client. Proceed to the "OpenAPI / Refitter Approach" section
3. **Custom client** -- as a last resort, if there is no NuGet and no OpenAPI spec, generate a custom HTTP client from the API documentation. Proceed to the "Custom HTTP Client Approach" section

Consult with the user on which approach to take if not clear.

---

## NuGet Approach

### Add the NuGet package

7. Add the selected NuGet package(s) to the project using the dotnet CLI:

```bash
dotnet add package [PackageName]
```

### Create the Connection class

8. Create `Connections/[ConnectionName]/[ConnectionName]Connection.cs` implementing `IConnection` and `IHaveSecrets`.

The connection class must follow this pattern:

```csharp
using Ivy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace [ProjectNamespace].Connections.[ConnectionName];

public class [ConnectionName]Connection : IConnection, IHaveSecrets
{
    public string GetContext(string connectionPath) => """
        # [ConnectionName] Connection

        ## Getting the service

        ```csharp
        var client = UseService<[ServiceType]>();
        ```

        ## Common Usage

        // Include 2-3 of the most common usage examples with code snippets
        // Show how to call the most important methods
        // Include information about response types and error handling
        """;

    public string GetNamespace() => typeof([ConnectionName]Connection).Namespace!;

    public string GetName() => "[ConnectionName]";

    public string GetConnectionType() => "Nuget:[PackageId]";

    public ConnectionEntity[] GetEntities() =>
    [
        // List the main entities this API exposes
        // e.g. new("Message", "Messages"),
    ];

    public void RegisterServices(Server server)
    {
        server.Services.AddSingleton(sp =>
        {
            var config = sp.GetRequiredService<IConfiguration>();
            var apiKey = config["[ConnectionName]:ApiKey"] ?? "";
            // Create and return the API client
        });

        // For LLM connections, also register IChatClient:
        // server.Services.AddSingleton<IChatClient>(sp => { ... });
    }

    public async Task<(bool ok, string? message)> TestConnection(IConfiguration config)
    {
        try
        {
            var apiKey = config["[ConnectionName]:ApiKey"];
            if (string.IsNullOrEmpty(apiKey))
                return (false, "[ConnectionName]:ApiKey is not configured. Please set your API key in user secrets.");

            // Create a client and call a lightweight read-only endpoint
            return (true, "Connected successfully.");
        }
        catch (Exception ex)
        {
            return (false, $"Connection test failed: {ex.Message}");
        }
    }

    public Secret[] GetSecrets() => [new Secret("[ConnectionName]:ApiKey")];
    // If the user provided a value, use: new Secret("[ConnectionName]:ApiKey", "user-provided-value")
}
```

- Some connections need multiple secrets. Use appropriate names.

### Set up secrets

9. Determine what secrets are needed to connect to the API. For example: API key, bearer token, client ID/secret, endpoint URL, etc. Ask the user what credentials the API requires if not clear from documentation.

10. Initialize user secrets for the project (idempotent if already initialized):

```bash
dotnet user-secrets init
```

> **Note:** If the user provided specific credential values in their prompt (API keys, tokens, endpoints), set them as `Preset` values in `GetSecrets()`: `new Secret("Key", "user-provided-value")`. This ensures the connection works immediately without manual secret configuration.

11. Make sure the connection class's `GetSecrets()` method returns all secret keys, and `RegisterServices()` reads them from `IConfiguration`.

After completing the NuGet approach, proceed to the Verification section.

---

## OpenAPI / Refitter Approach

No suitable NuGet package was found, but the API has an official OpenAPI/Swagger specification. Use **Refitter** to generate a strongly-typed C# client from the spec.

### Find the OpenAPI spec

7. Search for the official OpenAPI/Swagger spec URL for the API. Common locations:
   - `https://api.example.com/openapi.json`
   - `https://api.example.com/swagger.json`
   - `https://api.example.com/v1/openapi.yaml`
   - The API documentation site often links to it
   - GitHub repositories may contain the spec file

Validate that the URL returns a valid OpenAPI 2.0/3.0/3.1 spec (JSON or YAML). Ask the user to confirm the spec URL.

### Determine authentication

8. What authentication scheme does the API use?
   - **Bearer token** (`Authorization: Bearer <token>`) -- most common for modern APIs
   - **API key header** (e.g. `X-Api-Key: <key>`) -- common for simpler APIs

Note the auth scheme type (`bearer` or `apikey`) and the header name (e.g. `Authorization` or `X-Api-Key`).

### Set up secrets

9. Initialize user secrets for the project (idempotent if already initialized):

```bash
dotnet user-secrets init
```

> **Note:** If the user provided specific credential values in their prompt (API keys, tokens, endpoints), set them as `Preset` values in `GetSecrets()`: `new Secret("Key", "user-provided-value")`. This ensures the connection works immediately without manual secret configuration.

### Install Refitter and Refit

10. Ensure the Refitter dotnet tool is installed globally:

```bash
dotnet tool install --global refitter
```

11. Add the Refit NuGet package to the project:

```bash
dotnet add package Refit
```

### Create the .refitter configuration

12. Create the file `Connections/[ConnectionName]/[ConnectionName].refitter` with smart defaults:

```json
{
    "openApiPath": "<OpenAPI spec URL>",
    "namespace": "[ProjectNamespace].Connections.[ConnectionName]",
    "outputFilename": "<absolute path to Connections/[ConnectionName]/[ConnectionName]Client.cs>",
    "naming": {
        "useOpenApiTitle": false,
        "interfaceName": "[ConnectionName]Client"
    },
    "immutableRecords": false,
    "operationNameGenerator": "SingleClientFromPathSegments",
    "optionalParameters": true,
    "addAutoGeneratedHeader": false,
    "generateXmlDocCodeComments": false,
    "generateStatusCodeComments": false,
    "codeGeneratorSettings": {
        "generateOptionalPropertiesAsNullable": true,
        "generateNullableReferenceTypes": true
    }
}
```

Key settings explained:
- `operationNameGenerator: "SingleClientFromPathSegments"` -- generates a single interface with method names derived from URL path segments instead of operationIds (cleaner names)
- `optionalParameters: true` -- optional query params become C# optional parameters with defaults
- `immutableRecords: false` -- generates mutable classes (safer for serialization)
- `generateNullableReferenceTypes: true` -- respects nullable reference types
- `interfaceName` -- omits the `I` prefix; Refitter adds it automatically, producing `I[ConnectionName]Client`

### Generate the client

13. Run refitter to generate the C# client from the OpenAPI spec:

```bash
refitter --settings-file "Connections/[ConnectionName]/[ConnectionName].refitter" --skip-validation --no-banner
```

This generates `[ConnectionName]Client.cs` containing the `I[ConnectionName]Client` Refit interface with all API methods.

14. Verify the generated file exists and contains the expected interface. If generation fails, check that:
   - The OpenAPI spec URL is accessible
   - The spec is valid (the `--skip-validation` flag is already used)
   - The output path is correct

### Create Refresh.ps1

15. Create `Connections/[ConnectionName]/Refresh.ps1` so the client can be regenerated when the API spec changes:

```powershell
# Regenerates the [ConnectionName] API client from the OpenAPI spec.
# Run this script when the API spec has been updated.
#
# Prerequisites: dotnet tool install --global refitter

$ErrorActionPreference = "Stop"
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$settingsFile = Join-Path $scriptDir "[ConnectionName].refitter"

Write-Host "Regenerating [ConnectionName] client from OpenAPI spec..." -ForegroundColor Cyan
refitter --settings-file $settingsFile --skip-validation --no-banner

if ($LASTEXITCODE -eq 0) {
    Write-Host "Client regenerated successfully." -ForegroundColor Green
} else {
    Write-Host "Refitter failed with exit code $LASTEXITCODE." -ForegroundColor Red
    exit $LASTEXITCODE
}
```

### Create the ClientFactory

16. Create `Connections/[ConnectionName]/[ConnectionName]ClientFactory.cs`. This class reads credentials from configuration and creates an authenticated Refit client:

```csharp
using System.Text.Json;
using System.Text.Json.Serialization;
using Refit;

namespace [ProjectNamespace].Connections.[ConnectionName];

public static class [ConnectionName]ClientFactory
{
    private class [ConnectionName]AuthHandler : DelegatingHandler
    {
        private readonly string _token;
        private readonly string _headerName;
        private readonly bool _isBearer;

        public [ConnectionName]AuthHandler(string token, string headerName, bool isBearer)
        {
            _token = token;
            _headerName = headerName;
            _isBearer = isBearer;
            InnerHandler = new HttpClientHandler();
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (_isBearer)
                request.Headers.Add(_headerName, $"Bearer {_token}");
            else
                request.Headers.Add(_headerName, _token);
            return base.SendAsync(request, cancellationToken);
        }
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: true) },
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        NumberHandling = JsonNumberHandling.AllowReadingFromString,
    };

    public static I[ConnectionName]Client CreateClient()
    {
        var configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .AddUserSecrets(typeof([ConnectionName]ClientFactory).Assembly)
            .Build();

        var endpointUrl = configuration.GetValue<string>("[ConnectionName]:EndpointUrl")
            ?? throw new Exception("[ConnectionName]:EndpointUrl is required");
        var token = configuration.GetValue<string>("[ConnectionName]:BearerToken")  // or [ConnectionName]:ApiKey
            ?? throw new Exception("[ConnectionName]:BearerToken is required");

        return CreateClient(endpointUrl, token);
    }

    public static I[ConnectionName]Client CreateClient(string endpointUrl, string token)
    {
        return RestService.For<I[ConnectionName]Client>(endpointUrl, new RefitSettings
        {
            HttpMessageHandlerFactory = () => new [ConnectionName]AuthHandler(token, "Authorization", true),
            // For API key auth: new [ConnectionName]AuthHandler(token, "X-Api-Key", false)
            ContentSerializer = new SystemTextJsonContentSerializer(JsonOptions)
        });
    }
}
```

### Create the Connection class

17. Create `Connections/[ConnectionName]/[ConnectionName]Connection.cs` implementing `IConnection` and `IHaveSecrets`:

```csharp
using System.Reflection;
using Ivy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace [ProjectNamespace].Connections.[ConnectionName];

public class [ConnectionName]Connection : IConnection, IHaveSecrets
{
    public string GetContext(string connectionPath)
    {
        var connectionFile = nameof([ConnectionName]Connection) + ".cs";
        var clientFactoryFile = nameof([ConnectionName]ClientFactory) + ".cs";
        var files = Directory.GetFiles(connectionPath, "*.*", SearchOption.TopDirectoryOnly)
            .Where(f => !f.EndsWith(connectionFile) && !f.EndsWith(clientFactoryFile))
            .Select(File.ReadAllText)
            .ToArray();
        return string.Join(Environment.NewLine, files);
    }

    public string GetNamespace() => typeof([ConnectionName]Connection).Namespace!;

    public string GetName() => "[ConnectionName]";

    public string GetConnectionType() => "OpenApi.Rest";

    public ConnectionEntity[] GetEntities()
    {
        var clientType = typeof(I[ConnectionName]Client);
        var methods = clientType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        return methods.Select(m => new ConnectionEntity(m.Name, m.Name)).ToArray();
    }

    public void RegisterServices(Server server)
    {
        server.Services.AddTransient<I[ConnectionName]Client>(_ => [ConnectionName]ClientFactory.CreateClient());
    }

    public Secret[] GetSecrets() =>
    [
        new Secret("[ConnectionName]:EndpointUrl"),
        new Secret("[ConnectionName]:BearerToken"),  // or [ConnectionName]:ApiKey
    ];

    public async Task<(bool ok, string? message)> TestConnection(IConfiguration config)
    {
        try
        {
            var token = config["[ConnectionName]:BearerToken"];  // or [ConnectionName]:ApiKey
            if (string.IsNullOrEmpty(token))
                return (false, "[ConnectionName]:BearerToken is not configured. Please set your API token in user secrets.");

            var client = [ConnectionName]ClientFactory.CreateClient();
            // Call a lightweight read-only endpoint to verify connectivity
            return (true, "Connected successfully.");
        }
        catch (Exception ex)
        {
            return (false, $"Connection test failed: {ex.Message}");
        }
    }
}
```

After completing the OpenAPI approach, proceed to the Verification section.

---

## Custom HTTP Client Approach

No suitable NuGet package was found and there is no official OpenAPI/Swagger specification available. As a last resort, generate a custom C# HTTP client based on the API documentation.

### Research the API

7. Find and read the official API documentation. Identify:
   - Base URL and versioning scheme (e.g. `https://api.example.com/v1`)
   - Authentication method (Bearer token, API key header, query parameter, etc.)
   - The most important endpoints (focus on 5-10 core read-only endpoints first)
   - Request/response formats (usually JSON)
   - Rate limiting and pagination patterns
   - Error response format

8. Ask the user to confirm which endpoints are most important for their use case.

### Set up secrets

9. Determine what authentication the API requires. Note the auth type and header name.

10. Initialize user secrets for the project (idempotent if already initialized):

```bash
dotnet user-secrets init
```

> **Note:** If the user provided specific credential values in their prompt (API keys, tokens, endpoints), set them as `Preset` values in `GetSecrets()`: `new Secret("Key", "user-provided-value")`. This ensures the connection works immediately without manual secret configuration.

### Generate the custom client

11. Create `Connections/[ConnectionName]/[ConnectionName]Client.cs` with a clean, typed HTTP client. Follow these guidelines:

**Client structure:**

```csharp
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace [ProjectNamespace].Connections.[ConnectionName];

public interface I[ConnectionName]Client
{
    // One method per endpoint, grouped logically
    Task<ListResponse<Item>> GetItemsAsync(int? page = null, int? pageSize = null, CancellationToken ct = default);
    Task<Item> GetItemByIdAsync(string id, CancellationToken ct = default);
    // ... more endpoints
}

public class [ConnectionName]Client : I[ConnectionName]Client
{
    private readonly HttpClient _http;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public [ConnectionName]Client(HttpClient http)
    {
        _http = http;
    }

    public async Task<ListResponse<Item>> GetItemsAsync(int? page = null, int? pageSize = null, CancellationToken ct = default)
    {
        var query = new List<string>();
        if (page.HasValue) query.Add($"page={page}");
        if (pageSize.HasValue) query.Add($"page_size={pageSize}");
        var qs = query.Count > 0 ? "?" + string.Join("&", query) : "";

        var response = await _http.GetAsync($"/items{qs}", ct);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<ListResponse<Item>>(JsonOptions, ct))!;
    }

    // ... more endpoint implementations
}
```

**Guidelines for a good custom client:**
- Define an `I[ConnectionName]Client` interface so it can be mocked in tests and registered with DI
- Use `System.Net.Http.Json` for JSON serialization (no external dependencies)
- Use `System.Text.Json` with `CamelCase` naming policy and `WhenWritingNull` for clean payloads
- Make all methods async with `CancellationToken` support
- Use optional parameters for query string params (pagination, filters, sorting)
- Create typed C# records/classes for request/response models -- match the API's JSON structure
- Use `record` types for response models (immutable, concise)
- Handle pagination patterns (offset/limit, cursor-based, page-based) consistently
- Only implement read-only endpoints first; add write endpoints if specifically needed

**Response model patterns:**

```csharp
// For paginated list responses
public record ListResponse<T>(
    [property: JsonPropertyName("data")] T[] Data,
    [property: JsonPropertyName("total")] int Total,
    [property: JsonPropertyName("has_more")] bool HasMore
);

// For individual resource models
public record Item(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("created_at")] DateTimeOffset CreatedAt
);
```

### Create the Connection class

12. Create `Connections/[ConnectionName]/[ConnectionName]Connection.cs` implementing `IConnection` and `IHaveSecrets`:

```csharp
using Ivy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace [ProjectNamespace].Connections.[ConnectionName];

public class [ConnectionName]Connection : IConnection, IHaveSecrets
{
    public string GetName() => "[ConnectionName]";
    public string GetNamespace() => typeof([ConnectionName]Connection).Namespace!;
    public string GetConnectionType() => "Custom";

    public void RegisterServices(Server server)
    {
        server.Services.AddTransient<I[ConnectionName]Client>(sp =>
        {
            var config = sp.GetRequiredService<IConfiguration>();
            var baseUrl = config["[ConnectionName]:BaseUrl"]
                ?? throw new Exception("[ConnectionName]:BaseUrl is required");
            var apiKey = config["[ConnectionName]:ApiKey"]
                ?? throw new Exception("[ConnectionName]:ApiKey is required");

            var http = new HttpClient
            {
                BaseAddress = new Uri(baseUrl)
            };
            http.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
            // OR: http.DefaultRequestHeaders.Add("X-Api-Key", apiKey);

            return new [ConnectionName]Client(http);
        });
    }

    public Secret[] GetSecrets() =>
    [
        new Secret("[ConnectionName]:BaseUrl"),
        new Secret("[ConnectionName]:ApiKey"),
        // If the user provided values, use: new Secret("[ConnectionName]:ApiKey", "user-provided-value")
    ];

    public string GetContext(string connectionPath)
    {
        return """
            ## [ConnectionName] API Client

            Get the client:
            ```csharp
            var client = UseService<I[ConnectionName]Client>();
            ```

            // Include 2-3 of the most common usage examples
            // Show how to call the most important methods
            """;
    }

    public ConnectionEntity[] GetEntities() =>
    [
        // Fill based on API resources
    ];

    public async Task<(bool ok, string? message)> TestConnection(IConfiguration config)
    {
        try
        {
            var baseUrl = config["[ConnectionName]:BaseUrl"];
            var apiKey = config["[ConnectionName]:ApiKey"];
            if (string.IsNullOrEmpty(baseUrl) || string.IsNullOrEmpty(apiKey))
                return (false, "[ConnectionName] secrets are not configured. Please set BaseUrl and ApiKey in user secrets.");

            // Create a client and call a lightweight read-only endpoint
            return (true, "Connected successfully.");
        }
        catch (Exception ex)
        {
            return (false, $"Connection test failed: {ex.Message}");
        }
    }
}
```

After completing the Custom approach, proceed to the Verification section.

---

## Verification

This section applies regardless of which approach was used above.

18. Run `dotnet build` to verify everything compiles. Fix any errors.

19. Test the connection by running `dotnet build` to verify compilation, then manually test by running the project with `dotnet run`. If the test fails, investigate and fix the issue. Common causes: missing or incorrect secrets, wrong API endpoint, network issues.

20. If the project is in a git repository, create a commit with a descriptive message summarizing what was added. For example: "Add [ConnectionName] connection with [approach] client".

21. Present a summary to the user:
    - Connection name and approach used (NuGet / OpenAPI / Custom)
    - Files created (list each file)
    - Secrets configured (list each secret key, not the values)
    - Demo apps created
    - Next steps: how to use the connection in their Ivy apps via `UseService<T>()`

## Post-run: Evaluate and Improve

After completing the task:

1. **Evaluate**: Did the build succeed? Were there compilation errors, unexpected behavior, or manual corrections needed during this run?
2. **Update learnings**: If anything required correction or was surprising, append a concise entry to `.ivy/learnings/ivy-create-any-connection.md` (create the file and `.ivy/learnings/` directory if they don't exist). Each entry should note: the date, what went wrong, why, and what to do differently next time.
3. **Skip if clean**: If everything succeeded without issues, do not update the learnings file.
