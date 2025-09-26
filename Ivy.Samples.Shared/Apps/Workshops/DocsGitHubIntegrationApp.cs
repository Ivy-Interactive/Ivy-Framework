using Ivy.Shared;

namespace Ivy.Samples.Shared.Apps.Workshops;

[App(icon: Icons.Book, path: ["Workshops"], isVisible: true, title: "Docs GitHub Integration")]
public class DocsGitHubIntegrationApp : SampleBase
{
    protected override object? BuildSample()
    {
        return new Article()
            | new Markdown(
                """"
                # Project Idea: Enhanced Docs with GitHub Integration (11. January 2025)

                We want to enhance the documentation experience by integrating GitHub features directly into our docs pages, providing better context and collaboration tools.
                """"
            )
            | new Callout("This is a requirements sketch; it does not implement the feature.", icon: Icons.Info)
            | new Markdown(
                """"
                ## Goals

                * **Contributors Widget**: Display GitHub contributors for each documentation page beneath the TOC
                * **Copy for LLMs Component**: Add a "Copy for LLM" button to markdown content for easy AI tool integration
                * **Discussions Integration**: Show related GitHub discussions for each documentation page
                * **Enhanced Documentation Experience**: Provide better context about who maintains content and enable community engagement
                * **Seamless Integration**: Work with existing `Article`, `Markdown`, and docs generation pipeline
                """"
            )
            | new Markdown(
                """"
                ## Non-Goals (initial)

                * Real-time GitHub notifications or webhooks
                * GitHub issue creation from docs
                * Full GitHub project management integration
                * Analytics or tracking beyond basic contributor data
                """"
            )
            | new Markdown(
                """"
                ## User Stories

                ### Contributors Widget
                1. As a docs reader, I can see who has contributed to this specific documentation page
                2. As a contributor, I can click on contributor avatars to view their GitHub profiles
                3. As a maintainer, I want contributors to be automatically detected based on Git history for the specific markdown file

                ### Copy for LLMs Component
                1. As a developer, I can quickly copy documentation content in LLM-friendly format
                2. As a user, I can copy content with proper attribution and source links
                3. As a content consumer, I get clean markdown without navigation elements for AI tools

                ### GitHub Discussions
                1. As a reader, I can see if there are ongoing discussions about this documentation page
                2. As a community member, I can easily navigate to relevant discussions
                3. As a maintainer, I can see which docs pages generate the most questions
                """"
            )
            | new Markdown(
                """"
                ## Current Architecture Analysis

                Based on codebase exploration, the current docs system has these key components:

                ### Backend (C#)
                - `Ivy.Docs.Tools/MarkdownConverter.cs` - Converts MD files to C# apps
                - `Article` widget with `DocumentSource` property for GitHub source tracking
                - `MarkdownRenderer` already has copy functionality for code blocks
                
                ### Frontend (React/TypeScript)
                - `ArticleWidget.tsx` - Handles TOC rendering and article layout
                - `MarkdownRenderer.tsx` - Processes markdown with syntax highlighting
                - TOC is dynamically generated from headings with intersection observer
                
                ### Existing GitHub Integration
                - GitHub OAuth via Supabase and Auth0 providers
                - GitHub Actions for CI/CD workflows
                """"
            )
            | new Markdown(
                """"
                ## Proposed Implementation

                ### 1. GitHub Contributors Widget

                **Backend Implementation:**
                ```csharp
                // New service: Ivy/Services/GitHubService.cs
                public class GitHubService
                {
                    public async Task<List<Contributor>> GetFileContributorsAsync(string filePath);
                }
                
                // New widget: Ivy/Widgets/GitHub/ContributorsWidget.cs
                public record Contributors : WidgetBase<Contributors>
                {
                    [Prop] public string FilePath { get; set; }
                    [Prop] public int MaxContributors { get; set; } = 5;
                }
                ```
                
                **Frontend Implementation:**
                ```typescript
                // frontend/src/widgets/github/ContributorsWidget.tsx
                interface ContributorsWidgetProps {
                  filePath: string;
                  maxContributors: number;
                }
                
                // Integration in ArticleWidget.tsx after TOC
                {showContributors && documentSource && (
                  <Contributors filePath={documentSource} maxContributors={5} />
                )}
                ```

                ### 2. LLM Copy Component

                **Backend Enhancement:**
                ```csharp
                // Enhance Markdown widget
                public record Markdown : WidgetBase<Markdown>
                {
                    [Prop] public bool ShowLLMCopy { get; set; } = false;
                    [Prop] public string? DocumentSource { get; set; }
                }
                ```
                
                **Frontend Enhancement:**
                ```typescript
                // Add to MarkdownRenderer.tsx
                const LLMCopyButton = ({ content, source }: { content: string, source?: string }) => {
                  const copyForLLM = () => {
                    const cleanContent = stripNavigationElements(content);
                    const attribution = source ? `\n\nSource: ${source}` : '';
                    copyToClipboard(cleanContent + attribution);
                  };
                  // ... implementation
                };
                ```

                ### 3. GitHub Discussions Integration

                **Backend Implementation:**
                ```csharp
                // New widget: Ivy/Widgets/GitHub/DiscussionsWidget.cs
                public record GitHubDiscussions : WidgetBase<GitHubDiscussions>
                {
                    [Prop] public string Repository { get; set; }
                    [Prop] public string? SearchQuery { get; set; }
                    [Prop] public int MaxDiscussions { get; set; } = 3;
                }
                
                // Enhanced GitHubService
                public async Task<List<Discussion>> GetRelatedDiscussionsAsync(string repo, string query);
                ```
                
                **Frontend Implementation:**
                ```typescript
                // frontend/src/widgets/github/DiscussionsWidget.tsx
                // Integration in ArticleWidget.tsx footer
                {showDiscussions && (
                  <GitHubDiscussions 
                    repository="Ivy-Interactive/Ivy-Framework"
                    searchQuery={extractSearchTerms(content)}
                  />
                )}
                ```
                """"
            )
            | new Markdown(
                """"
                ## Technical Integration Points

                ### 1. Documentation Generation Pipeline
                - Extend `MarkdownConverter.cs` to extract file metadata for GitHub API calls
                - Add repository and file path information to generated C# apps
                - Ensure `DocumentSource` property is properly populated for all docs

                ### 2. Article Widget Enhancement
                - Modify `ArticleWidget.tsx` to include new sections:
                  - Contributors section after TOC (hidden on mobile)
                  - Discussions section in footer (before navigation)
                - Add props to control visibility: `showContributors`, `showDiscussions`

                ### 3. Backend Services
                - Create `GitHubService` with rate limiting and caching
                - Use GitHub GraphQL API for efficient data fetching
                - Store GitHub token in configuration/environment variables
                - Implement background refresh for contributor and discussion data

                ### 4. Frontend Components
                - Create reusable GitHub-themed components with Ivy styling
                - Ensure responsive design for mobile devices  
                - Add loading states and error handling
                - Use existing copy functionality patterns from `MarkdownRenderer`
                """"
            )
            | new Markdown(
                """"
                ## API Requirements

                ### GitHub REST/GraphQL APIs Needed:
                
                **Contributors API:**
                ```
                GET /repos/{owner}/{repo}/commits?path={file_path}
                ```
                
                **Discussions API:**
                ```graphql
                query {
                  repository(owner: "Ivy-Interactive", name: "Ivy-Framework") {
                    discussions(first: 10, orderBy: {field: UPDATED_AT, direction: DESC}) {
                      nodes {
                        title
                        url
                        createdAt
                        author { login, avatarUrl }
                        bodyText
                      }
                    }
                  }
                }
                ```
                
                **Rate Limiting Considerations:**
                - GitHub API: 5,000 requests/hour for authenticated requests
                - Implement caching with 1-hour TTL for contributors
                - Cache discussions for 15 minutes
                - Use conditional requests with ETags when possible
                """"
            )
            | new Markdown(
                """"
                ## UI/UX Design Requirements

                ### Contributors Section
                - Position: Between TOC and main content (desktop), hidden on mobile
                - Design: Horizontal list of contributor avatars with hover cards
                - Information: Avatar, name, contribution count, last contribution date
                - Interaction: Click avatar → open GitHub profile in new tab

                ### LLM Copy Button  
                - Position: Top-right corner of markdown content blocks
                - Design: Subtle button similar to existing code copy buttons
                - Functionality: Copy clean markdown + attribution
                - Feedback: Toast notification "Copied for LLM use"

                ### Discussions Section
                - Position: Article footer, before prev/next navigation
                - Design: Card-based layout with discussion previews
                - Content: Title, author, date, excerpt
                - Interaction: Click → open GitHub discussion in new tab
                - State: Loading skeleton, empty state, error handling
                """"
            )
            | new Markdown(
                """"
                ## Acceptance Criteria

                ### Contributors Widget
                - [ ] Shows up to 5 most recent contributors for each docs page
                - [ ] Displays contributor avatars, names, and GitHub profiles
                - [ ] Caches contributor data for 1 hour to respect rate limits
                - [ ] Handles cases where Git history is unavailable gracefully
                - [ ] Responsive design (hidden on mobile to save space)

                ### Copy for LLMs Feature
                - [ ] Adds "Copy for LLM" button to all markdown content
                - [ ] Strips navigation elements, leaving only content
                - [ ] Includes source attribution (GitHub file URL)
                - [ ] Provides user feedback on successful copy
                - [ ] Works consistently across all documentation pages

                ### GitHub Discussions
                - [ ] Shows 3 most relevant discussions based on page content
                - [ ] Displays discussion title, author, date, and preview
                - [ ] Links directly to GitHub discussions
                - [ ] Shows appropriate empty state when no discussions found
                - [ ] Updates discussion data every 15 minutes
                """"
            )
            | new Markdown(
                """"
                ## Risks & Considerations

                ### Technical Risks
                - **API Rate Limits**: GitHub API limits may affect user experience during high traffic
                - **Authentication**: Need to handle unauthenticated GitHub API calls gracefully
                - **Performance**: Additional API calls may slow down page loads
                - **File Path Mapping**: Need accurate mapping between docs URLs and GitHub file paths

                ### Content Risks
                - **Contributor Privacy**: Some contributors may not want their profiles displayed
                - **Discussion Relevance**: Automated discussion matching may show irrelevant content
                - **Content Licensing**: Need to ensure copied content includes appropriate attribution

                ### UX Risks
                - **Information Overload**: Too many GitHub elements might distract from documentation
                - **Mobile Experience**: Limited screen space for additional components
                - **Loading States**: Network delays could create jarring user experience
                """"
            )
            | new Markdown(
                """"
                ## Open Questions

                1. **Authentication Strategy**: Should we require GitHub authentication for enhanced features, or work with public API limits?

                2. **Caching Strategy**: Should we pre-generate contributor/discussion data during docs build, or fetch dynamically?

                3. **Search Algorithm**: How should we determine "related" discussions? By filename, content keywords, or manual tagging?

                4. **User Preferences**: Should users be able to toggle these features on/off, or make them always visible?

                5. **Analytics**: Do we want to track which docs pages generate the most GitHub engagement?

                6. **Fallback Strategy**: How should the system behave when GitHub is unavailable or rate-limited?
                """"
            )
            ;
    }
}
