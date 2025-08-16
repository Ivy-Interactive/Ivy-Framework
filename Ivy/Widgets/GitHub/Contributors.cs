using Ivy.Core;
using Ivy.Core.Hooks;
using Ivy.Services;
using Microsoft.Extensions.Configuration;

// ReSharper disable once CheckNamespace
namespace Ivy;

public class ContributorsView : ViewBase
{
    private readonly string? _filePath;
    private readonly int _maxContributors;
    private readonly bool _showOnMobile;

    public ContributorsView(string? filePath, int maxContributors = 5, bool showOnMobile = false)
    {
        _filePath = filePath;
        _maxContributors = maxContributors;
        _showOnMobile = showOnMobile;
    }

    public override object? Build()
    {
        Console.WriteLine($"ContributorsView.Build() called with filePath: {_filePath}");

        try
        {
            Console.WriteLine("Step 1: Checking filePath");
            if (string.IsNullOrEmpty(_filePath))
            {
                // No file path provided, show placeholder
                Console.WriteLine("No file path provided for contributors, showing placeholder");
                return new Contributors
                {
                    ContributorsData = new List<Contributor>(),
                    IsLoading = false,
                    HasError = true,
                    ShowOnMobile = _showOnMobile
                };
            }

            Console.WriteLine("Step 2: Getting GitHub service");

            // Debug: Try to get other services to see if DI is working
            var config = UseService<IConfiguration>();
            Console.WriteLine($"Step 2.1: IConfiguration available: {config != null}");

            var httpClientFactory = UseService<IHttpClientFactory>();
            Console.WriteLine($"Step 2.2: IHttpClientFactory available: {httpClientFactory != null}");

            var cacheService = UseService<ICacheService>();
            Console.WriteLine($"Step 2.3: ICacheService available: {cacheService != null}");

            var gitHubService = UseService<IGitHubService>();
            Console.WriteLine($"Step 3: GitHub service result: {gitHubService?.GetType().Name ?? "null"}");
            if (gitHubService == null)
            {
                // GitHub service not available, show placeholder instead of null
                Console.WriteLine("GitHub service not available, showing placeholder contributors");
                var placeholderWidget = new Contributors
                {
                    ContributorsData = new List<Contributor>(),
                    IsLoading = false,
                    HasError = true,
                    ShowOnMobile = _showOnMobile
                };
                Console.WriteLine("Returning placeholder Contributors widget");
                return placeholderWidget;
            }

            var contributors = UseState<List<Contributor>>(new List<Contributor>());
            var isLoading = UseState(true);
            var hasError = UseState(false);

            UseEffect(async () =>
            {
                try
                {
                    isLoading.Set(true);
                    hasError.Set(false);

                    var contributorsData = await gitHubService.GetFileContributorsAsync(_filePath);
                    var limitedContributors = contributorsData.Take(_maxContributors).ToList();

                    contributors.Set(limitedContributors);
                }
                catch (Exception)
                {
                    hasError.Set(true);
                }
                finally
                {
                    isLoading.Set(false);
                }
            }, []);

            var contributorsWidget = new Contributors
            {
                ContributorsData = contributors.Value,
                IsLoading = isLoading.Value,
                HasError = hasError.Value,
                ShowOnMobile = _showOnMobile
            };
            Console.WriteLine($"Returning Contributors widget with {contributors.Value.Count} contributors, isLoading: {isLoading.Value}, hasError: {hasError.Value}");
            return contributorsWidget;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR in ContributorsView.Build(): {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            return new Contributors
            {
                ContributorsData = new List<Contributor>(),
                IsLoading = false,
                HasError = true,
                ShowOnMobile = _showOnMobile
            };
        }
    }
}

public record Contributors : WidgetBase<Contributors>
{
    public Contributors(params IEnumerable<object> content) : base(content)
    {
    }

    [Prop] public List<Contributor> ContributorsData { get; set; } = new();

    [Prop] public bool IsLoading { get; set; } = false;

    [Prop] public bool HasError { get; set; } = false;

    [Prop] public bool ShowOnMobile { get; set; } = false;
}

public static class ContributorsExtensions
{
    public static ContributorsView CreateContributorsView(string? filePath, int maxContributors = 5, bool showOnMobile = false)
    {
        return new ContributorsView(filePath, maxContributors, showOnMobile);
    }

    public static Contributors ShowOnMobile(this Contributors contributors, bool showOnMobile = true) => contributors with { ShowOnMobile = showOnMobile };
}
