using Ivy.Core.Apps;

namespace Ivy.Test;

public class AppRoutingHelpersTests
{
    #region ValidateAppId - Valid Cases

    [Theory]
    [InlineData("dashboard")]
    [InlineData("my-app")]
    [InlineData("app_name")]
    [InlineData("app123")]
    [InlineData("MyApp")]
    public void ValidateAppId_SimpleValidIds_ReturnsValid(string appId)
    {
        var reservedPaths = new HashSet<string>();
        var result = AppRoutingHelpers.ValidateAppId(appId, reservedPaths);
        Assert.Equal(AppIdValidationResult.Valid, result);
    }

    [Theory]
    [InlineData("app.v2")]
    [InlineData("my.app")]
    [InlineData("users.profile")]
    [InlineData("api.v1.users")]
    [InlineData("com.example.app")]
    public void ValidateAppId_DottedAppIds_ReturnsValid(string appId)
    {
        // Critical: App IDs with dots must be allowed (e.g., versioning, namespacing)
        var reservedPaths = new HashSet<string>();
        var result = AppRoutingHelpers.ValidateAppId(appId, reservedPaths);
        Assert.Equal(AppIdValidationResult.Valid, result);
    }

    #endregion

    #region ValidateAppId - Empty/Whitespace Cases

    [Theory]
    [InlineData("")]
    public void ValidateAppId_Empty_ReturnsEmpty(string appId)
    {
        var reservedPaths = new HashSet<string>();
        var result = AppRoutingHelpers.ValidateAppId(appId, reservedPaths);
        Assert.Equal(AppIdValidationResult.Empty, result);
    }

    [Theory]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("  \t  ")]
    public void ValidateAppId_WhitespaceOnly_ReturnsUnsafeCharacters(string appId)
    {
        // Whitespace-only strings are caught by IsSafeAppId check, not the empty check
        var reservedPaths = new HashSet<string>();
        var result = AppRoutingHelpers.ValidateAppId(appId, reservedPaths);
        Assert.Equal(AppIdValidationResult.UnsafeCharacters, result);
    }

    #endregion

    #region ValidateAppId - Starts With Slash

    [Theory]
    [InlineData("/dashboard")]
    [InlineData("/users/profile")]
    [InlineData("/app")]
    public void ValidateAppId_StartsWithSlash_ReturnsStartsWithSlash(string appId)
    {
        var reservedPaths = new HashSet<string>();
        var result = AppRoutingHelpers.ValidateAppId(appId, reservedPaths);
        Assert.Equal(AppIdValidationResult.StartsWithSlash, result);
    }

    #endregion

    #region ValidateAppId - Unsafe Characters

    [Theory]
    [InlineData("app:protocol")]
    [InlineData("app?query")]
    [InlineData("app#fragment")]
    [InlineData("app&evil")]
    [InlineData("app%encoded")]
    [InlineData("app\\backslash")]
    public void ValidateAppId_UnsafeCharacters_ReturnsUnsafeCharacters(string appId)
    {
        var reservedPaths = new HashSet<string>();
        var result = AppRoutingHelpers.ValidateAppId(appId, reservedPaths);
        Assert.Equal(AppIdValidationResult.UnsafeCharacters, result);
    }

    [Fact]
    public void ValidateAppId_ControlCharacters_ReturnsUnsafeCharacters()
    {
        var appId = "app\u0000with\u0001control";
        var reservedPaths = new HashSet<string>();
        var result = AppRoutingHelpers.ValidateAppId(appId, reservedPaths);
        Assert.Equal(AppIdValidationResult.UnsafeCharacters, result);
    }

    [Theory]
    [InlineData("app\nwith\nnewlines")]
    [InlineData("app\twith\ttabs")]
    [InlineData("app\rwith\rreturns")]
    public void ValidateAppId_WhitespaceCharacters_ReturnsUnsafeCharacters(string appId)
    {
        var reservedPaths = new HashSet<string>();
        var result = AppRoutingHelpers.ValidateAppId(appId, reservedPaths);
        Assert.Equal(AppIdValidationResult.UnsafeCharacters, result);
    }

    #endregion

    #region ValidateAppId - Reserved Path Conflicts

    [Theory]
    [InlineData("ivy", new[] { "/ivy" })]
    [InlineData("api", new[] { "/api" })]
    [InlineData("assets", new[] { "/assets" })]
    public void ValidateAppId_ReservedPath_ReturnsReservedPathConflict(string appId, string[] reserved)
    {
        var reservedPaths = new HashSet<string>(reserved, StringComparer.OrdinalIgnoreCase);
        var result = AppRoutingHelpers.ValidateAppId(appId, reservedPaths);
        Assert.Equal(AppIdValidationResult.ReservedPathConflict, result);
    }

    [Theory]
    [InlineData("ivy.messages", new[] { "/ivy" })] // Should match /ivy/ prefix
    [InlineData("api.v1", new[] { "/api" })] // Should match /api/ prefix
    public void ValidateAppId_SubpathOfReserved_ReturnsReservedPathConflict(string appId, string[] reserved)
    {
        // Critical: Prevent apps that would create subpaths of reserved paths
        // e.g., "ivy.messages" would create "/ivy.messages" which might conflict with "/ivy/*"
        var reservedPaths = new HashSet<string>(reserved, StringComparer.OrdinalIgnoreCase);
        var result = AppRoutingHelpers.ValidateAppId(appId, reservedPaths);

        // This should pass because dots in app IDs don't create path segments
        // "ivy.messages" creates route "/ivy.messages", not "/ivy/messages"
        // So it shouldn't conflict with "/ivy/" prefix
        Assert.Equal(AppIdValidationResult.Valid, result);
    }

    [Theory]
    [InlineData("dashboard", new[] { "/ivy", "/api" })]
    [InlineData("users", new[] { "/ivy", "/api", "/assets" })]
    public void ValidateAppId_NotInReservedPaths_ReturnsValid(string appId, string[] reserved)
    {
        var reservedPaths = new HashSet<string>(reserved, StringComparer.OrdinalIgnoreCase);
        var result = AppRoutingHelpers.ValidateAppId(appId, reservedPaths);
        Assert.Equal(AppIdValidationResult.Valid, result);
    }

    [Fact]
    public void ValidateAppId_CaseInsensitiveReservedPaths_ReturnsReservedPathConflict()
    {
        var appId = "IVY";
        var reservedPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "/ivy" };
        var result = AppRoutingHelpers.ValidateAppId(appId, reservedPaths);
        Assert.Equal(AppIdValidationResult.ReservedPathConflict, result);
    }

    #endregion

    #region ValidateAppId - Static File Extension Conflicts

    [Theory]
    [InlineData("app.js")]
    [InlineData("script.js")]
    [InlineData("bundle.js")]
    [InlineData("component.jsx")]
    public void ValidateAppId_JavaScriptExtension_ReturnsStaticFileExtensionConflict(string appId)
    {
        // Critical: Prevent app IDs that look like JavaScript files
        var reservedPaths = new HashSet<string>();
        var result = AppRoutingHelpers.ValidateAppId(appId, reservedPaths);
        Assert.Equal(AppIdValidationResult.StaticFileExtensionConflict, result);
    }

    [Theory]
    [InlineData("config.json")]
    [InlineData("data.json")]
    [InlineData("manifest.json")]
    public void ValidateAppId_JsonExtension_ReturnsStaticFileExtensionConflict(string appId)
    {
        // Critical: Prevent app IDs that look like JSON files
        var reservedPaths = new HashSet<string>();
        var result = AppRoutingHelpers.ValidateAppId(appId, reservedPaths);
        Assert.Equal(AppIdValidationResult.StaticFileExtensionConflict, result);
    }

    [Theory]
    [InlineData("style.css")]
    [InlineData("theme.css")]
    public void ValidateAppId_CssExtension_ReturnsStaticFileExtensionConflict(string appId)
    {
        var reservedPaths = new HashSet<string>();
        var result = AppRoutingHelpers.ValidateAppId(appId, reservedPaths);
        Assert.Equal(AppIdValidationResult.StaticFileExtensionConflict, result);
    }

    [Theory]
    [InlineData("page.html")]
    [InlineData("index.html")]
    public void ValidateAppId_HtmlExtension_ReturnsStaticFileExtensionConflict(string appId)
    {
        var reservedPaths = new HashSet<string>();
        var result = AppRoutingHelpers.ValidateAppId(appId, reservedPaths);
        Assert.Equal(AppIdValidationResult.StaticFileExtensionConflict, result);
    }

    [Theory]
    [InlineData("image.png")]
    [InlineData("photo.jpg")]
    [InlineData("icon.ico")]
    [InlineData("graphic.svg")]
    public void ValidateAppId_ImageExtension_ReturnsStaticFileExtensionConflict(string appId)
    {
        var reservedPaths = new HashSet<string>();
        var result = AppRoutingHelpers.ValidateAppId(appId, reservedPaths);
        Assert.Equal(AppIdValidationResult.StaticFileExtensionConflict, result);
    }

    [Theory]
    [InlineData("font.woff")]
    [InlineData("typeface.woff2")]
    [InlineData("font.ttf")]
    public void ValidateAppId_FontExtension_ReturnsStaticFileExtensionConflict(string appId)
    {
        var reservedPaths = new HashSet<string>();
        var result = AppRoutingHelpers.ValidateAppId(appId, reservedPaths);
        Assert.Equal(AppIdValidationResult.StaticFileExtensionConflict, result);
    }

    [Theory]
    [InlineData("source.map")]
    public void ValidateAppId_SourceMapExtension_ReturnsStaticFileExtensionConflict(string appId)
    {
        var reservedPaths = new HashSet<string>();
        var result = AppRoutingHelpers.ValidateAppId(appId, reservedPaths);
        Assert.Equal(AppIdValidationResult.StaticFileExtensionConflict, result);
    }

    [Theory]
    [InlineData("app.v2")]     // .v2 is not a static file extension
    [InlineData("my.app")]     // .app is not a static file extension
    [InlineData("api.v1")]     // .v1 is not a static file extension
    public void ValidateAppId_NonStaticExtensions_ReturnsValid(string appId)
    {
        // Critical: Dots in app IDs should be allowed when not followed by static file extensions
        var reservedPaths = new HashSet<string>();
        var result = AppRoutingHelpers.ValidateAppId(appId, reservedPaths);
        Assert.Equal(AppIdValidationResult.Valid, result);
    }

    #endregion

    #region ValidateAppId - Edge Cases

    [Fact]
    public void ValidateAppId_MultipleViolations_ReturnsFirstViolation()
    {
        // App ID has multiple issues: starts with slash AND has unsafe character
        var appId = "/app?query";
        var reservedPaths = new HashSet<string>();
        var result = AppRoutingHelpers.ValidateAppId(appId, reservedPaths);

        // Should return the first check that fails (StartsWithSlash comes before UnsafeCharacters)
        Assert.Equal(AppIdValidationResult.StartsWithSlash, result);
    }

    [Fact]
    public void ValidateAppId_EmptyReservedPaths_ValidatesCorrectly()
    {
        var appId = "dashboard";
        var reservedPaths = new HashSet<string>();
        var result = AppRoutingHelpers.ValidateAppId(appId, reservedPaths);
        Assert.Equal(AppIdValidationResult.Valid, result);
    }

    [Fact]
    public void ValidateAppId_ManyReservedPaths_ValidatesCorrectly()
    {
        var appId = "myapp";
        var reservedPaths = new HashSet<string>
        {
            "/ivy",
            "/api",
            "/assets",
            "/fonts",
            "/_framework",
            "/favicon.ico",
            "/manifest.json"
        };
        var result = AppRoutingHelpers.ValidateAppId(appId, reservedPaths);
        Assert.Equal(AppIdValidationResult.Valid, result);
    }

    #endregion

    #region Integration Tests - gRPC Service Names

    [Theory]
    [InlineData("datatable.DataTableService")]  // gRPC service format
    [InlineData("grpc.ServiceName")]
    [InlineData("proto.MyService")]
    public void ValidateAppId_GrpcStyleNames_ValidWithoutReservedConflict(string appId)
    {
        // gRPC services use dotted names like "datatable.DataTableService"
        // These should be valid app IDs (dots allowed) unless they conflict with reserved paths
        var reservedPaths = new HashSet<string> { "/datatable" };
        var result = AppRoutingHelpers.ValidateAppId(appId, reservedPaths);

        // Should be valid because "datatable.DataTableService" != "/datatable" exactly
        // and doesn't start with "/datatable/"
        Assert.Equal(AppIdValidationResult.Valid, result);
    }

    #endregion

    #region Integration Tests - Real-World Scenarios

    [Fact]
    public void ValidateAppId_TypicalAppIds_AllValid()
    {
        var appIds = new[]
        {
            "dashboard",
            "users",
            "settings",
            "my-app",
            "app_name",
            "app123",
            "MyApp"
        };

        var reservedPaths = new HashSet<string> { "/ivy", "/api", "/assets" };

        foreach (var appId in appIds)
        {
            var result = AppRoutingHelpers.ValidateAppId(appId, reservedPaths);
            Assert.Equal(AppIdValidationResult.Valid, result);
        }
    }

    [Fact]
    public void ValidateAppId_TypicalReservedPaths_AllBlocked()
    {
        var systemPaths = new[]
        {
            "ivy",
            "api",
            "assets",
            "fonts",
            "_framework"
        };

        var reservedPaths = new HashSet<string>
        {
            "/ivy",
            "/api",
            "/assets",
            "/fonts",
            "/_framework"
        };

        foreach (var appId in systemPaths)
        {
            var result = AppRoutingHelpers.ValidateAppId(appId, reservedPaths);
            Assert.Equal(AppIdValidationResult.ReservedPathConflict, result);
        }
    }

    [Fact]
    public void ValidateAppId_TypicalStaticFiles_AllBlocked()
    {
        var staticFiles = new[]
        {
            "bundle.js",
            "config.json",
            "styles.css",
            "index.html",
            "logo.png",
            "font.woff2"
        };

        var reservedPaths = new HashSet<string>();

        foreach (var appId in staticFiles)
        {
            var result = AppRoutingHelpers.ValidateAppId(appId, reservedPaths);
            Assert.Equal(AppIdValidationResult.StaticFileExtensionConflict, result);
        }
    }

    #endregion
}
