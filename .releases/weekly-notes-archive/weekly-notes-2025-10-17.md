# Ivy Framework Weekly Notes - Week of 2025-10-17

## Powerful Data Tables with Apache Arrow

A new `DataTable` widget has been introduced for displaying and interacting with large datasets. Built on Apache Arrow technology, it provides exceptional performance with big data while offering modern data table features like sorting, filtering, pagination, and real-time updates.

**Basic usage:**

```csharp
// Convert any IQueryable<T> to a DataTable
sampleUsers.ToDataTable()
    .Header(u => u.Name, "Full Name")
    .Header(u => u.Email, "Email Address") 
    .Header(u => u.Salary, "Salary")
    .Header(u => u.Status, "Status");
```

The DataTable provides automatic scaffolding.

### MCP Documentation Tool

A new documentation tool has been added to the CLI's MCP (Model Context Protocol) server, giving you instant access to the framework's context.

Example usage in MCP-enabled environments:

```json
// List all available topics
{ "topic": "list" }

// Browse widgets documentation
{ "category": "widgets" }

// Search for deployment information
{ "search": "deploy" }

// Get specific topic
{ "topic": "getting-started/introduction" }
```

## Authentication Improvements

### Auth0 Configuration Update - Breaking Change

The Auth0 authentication provider now **requires** the `Auth0:Audience` parameter to be configured.

**Required configuration:**

```json
{
  "Auth0": {
    "Domain": "your-domain.auth0.com",
    "ClientId": "your-client-id", 
    "ClientSecret": "your-client-secret",
    "Audience": "https://your-domain.auth0.com/api/v2",
    "Namespace": "https://ivy.app/"
  }
}
```

Renamed `JWT`-related fields/methods that can sometimes have non-JWT tokens (as in `Authelia`’s case), to be more technically accurate (e.g., `AuthToken.Jwt` becomes `AuthToken.AccessToken`)

Added prompt for a new `Supabase:LegacyJwtSecret` config parameter to the `CLI`, which will now be required if the user hasn’t migrated to Supabase’s newer asymmetric signing key system

## New search in your apps sidebar

### Comprehensive Search Hints Implementation

A new `searchHints` property has been added that allows you to add additional search terms for better discoverability.

**In C# App attributes:**
```csharp
[App(path: ["Widgets", "Inputs"], 
     searchHints: ["password", "textarea", "email"])]
```

**In Markdown files:**
```yaml
---
searchHints:
  - charts
  - visualization
---
```

## Forms System Overhaul

### Simplified Form Submission

**New streamlined approach:**

```csharp
var contact = UseState(() => new ContactModel("", "", ""));

// React to form submission with UseEffect
UseEffect(() => {
    client.Toast($"Message from {contact.Value.Name} sent!");
}, contact);

// Forms now handle submission automatically
return contact.ToForm().Required(m => m.Name, m => m.Email);
```

Fields will now also validate as soon as user blurs out of them.

### Enhanced Form Validation

**Automatic email validation**: Fields ending with "Email" now automatically get proper email validation using .NET's built-in `EmailAddressAttribute`.

**DataAnnotations support**: Forms now automatically respect standard .NET validation attributes:

```csharp
public record UserModel(
    [Required, MinLength(3)] string Username,
    [EmailAddress] string Email,
    [Required, MinLength(8)] string Password
);

// Validation rules are automatically applied - no manual setup needed!
var form = userModel.ToForm();
```

## CLI & Tooling Improvements

- Fixed `ivy init` command to properly handle directories with common repository files (`.gitignore`, `README.md`)
- Database generator now automatically ensures required .NET tools (`dotnet-ef`) are installed
- CLI updater displays loading messages with improved typography (H2 heading format)
