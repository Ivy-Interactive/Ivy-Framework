using Ivy.Shared;

namespace Ivy.Samples.Shared.Apps.Workshops;

[App(icon: Icons.Users, path: ["Workshops"], isVisible: true, title: "GitHub Contributors Demo")]
public class GitHubContributorsDemo : SampleBase
{
    protected override object? BuildSample()
    {
        return new Article()
            .DocumentSource("https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/README.md")
            .ShowContributors(true)
            | new Markdown(
                """"
                # GitHub Contributors Integration Demo

                This demonstrates the new GitHub Contributors feature integrated with the Article widget.
                
                ## How It Works

                The Contributors widget automatically:
                - Fetches contributors for the specific file from GitHub API
                - Caches the results for 1 hour to respect rate limits
                - Shows the top 5 contributors with their avatars and contribution counts
                - Links to their GitHub profiles when clicked
                - Displays gracefully in the sidebar after the Table of Contents

                ## Implementation

                To add contributors to any Article:

                ```csharp
                return new Article()
                    .DocumentSource("https://github.com/your-org/your-repo/blob/main/path/to/file.md")
                    .ShowContributors(true)
                    | content;
                ```

                ## Features

                - **Caching**: 1-hour TTL to respect GitHub API rate limits
                - **Responsive**: Hidden on mobile to save space
                - **Error Handling**: Graceful fallback when GitHub is unavailable
                - **Performance**: Async loading with loading states
                - **Accessible**: Proper alt text and keyboard navigation

                ## Configuration

                Set these environment variables for enhanced functionality:

                - `GitHub__Token`: Personal access token for higher rate limits
                - `GitHub__RepositoryOwner`: Repository owner (defaults to "Ivy-Interactive")  
                - `GitHub__RepositoryName`: Repository name (defaults to "Ivy-Framework")
                """"
            )
            | ContributorsExtensions.CreateContributorsView(
                "README.md",
                maxContributors: 5,
                showOnMobile: false
            )
            ;
    }
}
