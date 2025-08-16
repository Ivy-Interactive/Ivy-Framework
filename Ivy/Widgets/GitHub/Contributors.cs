using Ivy.Core;
using Ivy.Core.Hooks;
using Ivy.Services;

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
        if (string.IsNullOrEmpty(_filePath))
        {
            return null;
        }

        var gitHubService = UseService<IGitHubService>();
        if (gitHubService == null)
        {
            // GitHub service not available, don't show contributors
            Console.WriteLine("GitHub service not available, don't show contributors");
            return null;
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

        return new Contributors

        {
            ContributorsData = contributors.Value,
            IsLoading = isLoading.Value,
            HasError = hasError.Value,
            ShowOnMobile = _showOnMobile
        };
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
