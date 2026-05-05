---
name: ivy-create-soap-connection
description: >
  Add a SOAP/WSDL web service connection to an Ivy project. Generates a typed
  client from a WSDL URL using dotnet-svcutil. Supports no auth, HTTP Basic,
  Bearer token, and custom header authentication. Use when the user wants to
  connect to a SOAP service, WSDL endpoint, or WCF service.
allowed-tools: Bash(dotnet:*) Bash(svcutil:*) Bash(ivy:*) Read Write Edit Glob Grep
effort: high
argument-hint: "[WSDL URL of the SOAP service]"
---

# ivy-create-soap-connection

Add a SOAP web service connection to an existing Ivy project. This skill guides you through collecting the WSDL URL, authentication details, and generating a typed client using `dotnet-svcutil`.

## Pre-flight: Read Learnings

If the file `.ivy/learnings/ivy-create-soap-connection.md` exists in the project directory, read it first and apply any lessons learned from previous runs of this skill.

## Step 1: Validate the Project

1. Verify this is a valid Ivy project. Check for a `.csproj` file and `Program.cs` in the working directory. If this is not an Ivy project, tell the user and stop.

2. Clean up any empty connection folders under `Connections/` if they exist.

## Step 2: Collect the WSDL URL

3. Ask the user for the WSDL URL if not already provided. The URL must be a valid HTTP or HTTPS URL (matching `^https?://.+$`). If the URL is invalid, ask the user to provide a valid one.

4. Validate that the URL is accessible and points to a WSDL document.

## Step 3: Collect Connection Details

5. Ask the user for a connection name in **PascalCase** (e.g. "PaymentService", "WeatherApi"). The name must match the regex `^[A-Z][a-zA-Z0-9]*$`. A name is suggested based on the WSDL URL hostname. If the name is invalid or already in use, ask the user to provide a different one.

6. Ask the user for the SOAP service endpoint URL. This is the actual service URL that the client will call at runtime (not the WSDL URL). A suggestion is derived from the WSDL URL hostname.

7. Ask the user to select the authentication type:

| Auth Type | Description | Required Credentials |
|---|---|---|
| **none** | No authentication | -- |
| **basic** | HTTP Basic (username + password) | Username, Password |
| **bearer** | Bearer token | Token |
| **custom-header** | Custom header (name + value) | Header Name, Token/Value |

8. Based on the chosen auth type, collect the required credentials:
   - **basic**: Ask for username and password (password is a secret)
   - **bearer**: Ask for the bearer token (secret)
   - **custom-header**: Ask for the header name and the header value (value is a secret)
   - **none**: No credentials needed

## Step 4: Generate the Connection

9. Create the connection directory at `Connections/[ConnectionName]/`.

10. Ensure dotnet tools are installed. The `dotnet-svcutil` tool is required:

```bash
dotnet tool install --global dotnet-svcutil
```

11. Initialize user secrets and store credentials:

```bash
dotnet user-secrets init
dotnet user-secrets set "[ConnectionName]:EndpointUrl" "[endpoint-url]"
```

For auth credentials, also store them in user secrets:
- **basic**: `[ConnectionName]:Username` and `[ConnectionName]:Password`
- **bearer** or **custom-header**: `[ConnectionName]:Token`

12. Add the required WCF NuGet packages:

```bash
dotnet add package System.ServiceModel.Http
dotnet add package System.ServiceModel.Primitives
```

13. Run `dotnet-svcutil` to generate the client from the WSDL:

```bash
dotnet-svcutil [WsdlUrl] --outputDir Connections/[ConnectionName] --namespace "*,[ProjectNamespace].Connections.[ConnectionName]" --noLogo
```

This generates a `Reference.cs` file containing the service client class (inheriting from `System.ServiceModel.ClientBase<T>`).

14. Verify that `Reference.cs` was generated in the connection directory. If generation fails:
    - Verify that the WSDL URL is accessible and valid
    - Check that .NET SDK is installed and `dotnet-svcutil` is available
    - Verify the service uses SOAP 1.1 or 1.2

### Create the ClientFactory

15. Create `Connections/[ConnectionName]/[ConnectionName]ClientFactory.cs`. This class reads credentials from configuration and creates the SOAP client with the appropriate binding and authentication:

```csharp
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using Microsoft.Extensions.Configuration;

namespace [ProjectNamespace].Connections.[ConnectionName];

public static class [ConnectionName]ClientFactory
{
    public static [ServiceClientClassName] CreateClient()
    {
        var configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .AddUserSecrets(Assembly.GetEntryAssembly()!)
            .Build();

        var endpointUrl = configuration.GetValue<string>("[ConnectionName]:EndpointUrl")
            ?? throw new Exception("[ConnectionName]:EndpointUrl is required");

        var endpointUri = new Uri(endpointUrl);
        Binding binding;

        if (endpointUri.Scheme == "https")
        {
            var httpsBinding = new BasicHttpsBinding();
            httpsBinding.Security.Mode = BasicHttpsSecurityMode.Transport;
            binding = httpsBinding;
        }
        else
        {
            binding = new BasicHttpBinding();
        }

        var endpoint = new EndpointAddress(endpointUrl);
        var client = new [ServiceClientClassName](binding, endpoint);

        // For basic auth:
        // var username = configuration.GetValue<string>("[ConnectionName]:Username")
        //     ?? throw new Exception("[ConnectionName]:Username is required");
        // var password = configuration.GetValue<string>("[ConnectionName]:Password")
        //     ?? throw new Exception("[ConnectionName]:Password is required");
        // client.ClientCredentials.UserName.UserName = username;
        // client.ClientCredentials.UserName.Password = password;

        // For bearer auth:
        // var token = configuration.GetValue<string>("[ConnectionName]:Token")
        //     ?? throw new Exception("[ConnectionName]:Token is required");
        // client.Endpoint.EndpointBehaviors.Add(
        //     new AuthHeaderBehavior("Authorization", $"Bearer {token}"));

        // For custom-header auth:
        // var token = configuration.GetValue<string>("[ConnectionName]:Token")
        //     ?? throw new Exception("[ConnectionName]:Token is required");
        // client.Endpoint.EndpointBehaviors.Add(
        //     new AuthHeaderBehavior("[HeaderName]", token));

        return client;
    }

    // Include these classes when using bearer or custom-header auth:

    private class AuthHeaderBehavior(string headerName, string headerValue) : IEndpointBehavior
    {
        public void Validate(ServiceEndpoint endpoint) { }
        public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters) { }
        public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher) { }

        public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
            clientRuntime.ClientMessageInspectors.Add(new AuthHeaderInspector(headerName, headerValue));
        }
    }

    private class AuthHeaderInspector(string headerName, string headerValue) : IClientMessageInspector
    {
        public object? BeforeSendRequest(ref Message request, IClientChannel channel)
        {
            var httpRequestProperty = new System.ServiceModel.Channels.HttpRequestMessageProperty();
            httpRequestProperty.Headers[headerName] = headerValue;

            if (request.Properties.ContainsKey(HttpRequestMessageProperty.Name))
            {
                request.Properties[HttpRequestMessageProperty.Name] = httpRequestProperty;
            }
            else
            {
                request.Properties.Add(HttpRequestMessageProperty.Name, httpRequestProperty);
            }

            return null;
        }

        public void AfterReceiveReply(ref Message reply, object correlationState) { }
    }
}
```

Uncomment and use only the auth section matching the chosen auth type. Remove the `AuthHeaderBehavior` and `AuthHeaderInspector` classes if auth type is `none` or `basic`.

The `[ServiceClientClassName]` is detected from the generated `Reference.cs` -- look for a class inheriting from `System.ServiceModel.ClientBase<T>` (e.g. `SomeServiceClient`). If detection fails, default to `[ConnectionName]Client`.

### Create the Connection class

16. Create `Connections/[ConnectionName]/[ConnectionName]Connection.cs` implementing `IConnection` and `IHaveSecrets`:

```csharp
using Ivy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

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

    public string GetName() => "[ConnectionName]";

    public string GetNamespace() => typeof([ConnectionName]Connection).Namespace!;

    public string GetConnectionType() => "Soap.Wsdl";

    public ConnectionEntity[] GetEntities()
    {
        var clientType = typeof([ServiceClientClassName]);
        var methods = clientType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Where(m => m.Name.EndsWith("Async"))
            .ToArray();

        return methods
            .Select(m => new ConnectionEntity(m.Name, m.Name))
            .ToArray();
    }

    public void RegisterServices(Server server)
    {
        server.Services.AddTransient(_ => [ConnectionName]ClientFactory.CreateClient());
    }

    public Secret[] GetSecrets()
    {
        return
        [
            new Secret("[ConnectionName]:EndpointUrl"),
            // For basic auth:
            // new Secret("[ConnectionName]:Username"),
            // new Secret("[ConnectionName]:Password"),
            // For bearer or custom-header auth:
            // new Secret("[ConnectionName]:Token"),
        ];
    }

    public async Task<(bool ok, string? message)> TestConnection(IConfiguration config)
    {
        try
        {
            var client = [ConnectionName]ClientFactory.CreateClient();
            return (true, null);
        }
        catch (Exception ex)
        {
            return (false, $"Connection test failed: {ex.Message}");
        }
    }
}
```

Adjust the `GetSecrets()` method to include only the secrets relevant to the chosen auth type.

Expected directory structure after completion:

```
[UserProject]/
├── [UserProject].csproj
├── Program.cs
├── Connections/
│   └── [ConnectionName]/
│       ├── [ConnectionName]Connection.cs
│       ├── [ConnectionName]ClientFactory.cs
│       └── Reference.cs (generated by svcutil)
```

## Step 5: Verify

17. Run `dotnet build` to verify everything compiles. Fix any errors.

18. Run `dotnet build` to verify the project compiles. Then test the connection by running `dotnet run` and verifying the app starts without errors. If the test fails, investigate and fix the issue.

19. If the project is in a git repository, create a commit with a descriptive message, for example: "Added SOAP connection '[ConnectionName]'."

20. Tell the user the connection is ready and show them how to use it. Example usage:

```csharp
public class SomeView : ViewBase
{
    public object? Build()
    {
        var client = UseService<[ServiceClientClassName]>();
        // Call SOAP operations, e.g.:
        // var result = await client.SomeOperationAsync(request);
        ...
    }
}
```

## Recovery

If the setup fails:

1. Diagnose the root cause (invalid WSDL URL, svcutil failure, incompatible SOAP version, auth issues):
   - For invalid WSDL location: verify the WSDL URL is accessible or the file path exists
   - For svcutil failure: check that .NET SDK is installed and `dotnet-svcutil` is available
   - For SOAP version issues: verify the service uses SOAP 1.1 or 1.2 (WS-* extensions may not be fully supported)
   - For auth issues: check if the SOAP service requires WS-Security or HTTP auth

2. After fixing the underlying issue, either retry using the `/ivy-create-soap-connection` skill from scratch, or manually create the connection:
   - Run svcutil manually: `dotnet-svcutil [wsdl-url] --outputDir Connections/[ConnectionName]/`
   - Add the generated classes to your project
   - Configure the SOAP client endpoint and binding in code
   - Implement authentication if required (WS-Security, HTTP Basic, etc.)
   - Test the connection with a sample SOAP operation

3. Use `ivy ask "How do I create a SOAP connection?"` or `ivy ask "How do I configure SOAP authentication?"` for API guidance.

## Post-run: Evaluate and Improve

After completing the task:

1. **Evaluate**: Did the build succeed? Were there compilation errors, unexpected behavior, or manual corrections needed during this run?
2. **Update learnings**: If anything required correction or was surprising, append a concise entry to `.ivy/learnings/ivy-create-soap-connection.md` (create the file and `.ivy/learnings/` directory if they don't exist). Each entry should note: the date, what went wrong, why, and what to do differently next time.
3. **Skip if clean**: If everything succeeded without issues, do not update the learnings file.
