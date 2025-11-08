using Ivy.Apps;
using Ivy.Auth;
using Ivy.Client;
using Ivy.Core;
using Ivy.Helpers;
using Ivy.Hooks;
using Ivy.Shared;
using Ivy.Views;
using Ivy.Widgets.Internal;
using System.Collections.Immutable;

namespace Ivy.Chrome;

[App(isVisible: false)]
public class DefaultSidebarChrome(ChromeSettings settings) : ViewBase
{
    private record TabState(string Id, string AppId, string Title, AppHost AppHost, Icons? Icon, string RefreshToken)
    {
        public Tab ToTab() => new Tab(Title, AppHost).Icon(Icon).Key(Utils.GetShortHash(Id + RefreshToken));
    }

    public override object? Build()
    {
        var tabs = UseState(ImmutableArray.Create<TabState>);
        var selectedIndex = UseState<int?>();
        var appRepository = UseService<IAppRepository>();
        var client = UseService<IClientProvider>();
        var auth = UseService<IAuthService?>();
        var user = UseState<UserInfo?>();
        var currentApp = UseState<AppHost?>();
        var search = UseState("");
        var menuItems = UseState(() => appRepository.GetMenuItems());
        var args = UseService<AppArgs>();
        var navigate = Context.UseSignal<NavigateSignal, NavigateArgs, Unit>();
        var navigator = this.UseNavigation();

        UseEffect(() =>
        {
            return navigate.Receive(navigateArgs =>
            {
                OpenApp(navigateArgs);
                return default!;
            });
        });

        UseEffect(() =>
        {
            if (string.IsNullOrWhiteSpace(search.Value))
            {
                menuItems.Set(appRepository.GetMenuItems());
            }
            else
            {
                var result = appRepository.GetMenuItems().Flatten()
                    .Where(item => item.Children == null || item.Children.Length == 0) // Only include leaf nodes (actual apps)
                    .Select(item => new { Item = item, Score = ChromeUtils.ItemMatchScore(item, search.Value) })
                    .Where(x => x.Score > 0)
                    .OrderByDescending(x => x.Score)
                    .ThenBy(x => x.Item.Label)
                    .Select(x => x.Item)
                    .ToArray();

                if (result.Length > 0)
                {
                    menuItems.Set([MenuItem.Default("Search Results").Children(result)]);
                }
                else
                {
                    menuItems.Set([]);
                }
            }
        }, [search]);

        void OpenApp(NavigateArgs navigateArgs, bool replaceHistory = false)
        {
            if (settings.Navigation == ChromeNavigation.Pages)
            {
                var previousApp = currentApp.Value?.AppId;

                if (navigateArgs.AppId == null)
                {
                    navigateArgs = navigateArgs with { AppId = settings.DefaultAppId };
                }

                var appHost = navigateArgs.AppId != null
                    ? navigateArgs.ToAppHost(args.ConnectionId)
                    : null;

                currentApp.Set(appHost);
                // Update browser URL for page navigation
                if (navigateArgs.Purpose is NavigationPurpose.NewDestination && previousApp != navigateArgs.AppId)
                {
                    client.Redirect(navigateArgs.GetUrl(), replaceHistory);
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(navigateArgs.TabId))
                {
                    // Try to find existing tab with the given TabId
                    var tabIndex = tabs.Value.ToList().FindIndex(t => t.Id == navigateArgs.TabId);
                    if (tabIndex >= 0)
                    {
                        selectedIndex.Set(tabIndex);

                        // Update browser URL when switching to existing tab
                        var tab = tabs.Value[tabIndex];
                        if (navigateArgs.Purpose is NavigationPurpose.NewDestination)
                        {
                            client.Redirect(navigateArgs.GetUrl(), replaceHistory, tabId: tab.Id);
                        }
                        return;
                    }

                    if (navigateArgs.Purpose is NavigationPurpose.HistoryTraversal)
                    {
                        client.Error(new InvalidOperationException("Tab no longer exists."));
                        return;
                    }
                }

                if (navigateArgs.AppId == null)
                {
                    // If there is no app ID or tab ID specified, do nothing.
                    return;
                }

                var tabId = Guid.NewGuid().ToString();
                var appHost = navigateArgs.ToAppHost(args.ConnectionId);

                if (settings.PreventTabDuplicates)
                {
                    var appId = navigateArgs.AppId;
                    int existingTabIndex = -1;
                    for (int i = 0; i < tabs.Value.Length; i++)
                    {
                        if (tabs.Value[i].AppId == appId)
                        {
                            existingTabIndex = i;
                            break;
                        }
                    }

                    if (existingTabIndex >= 0)
                    {
                        var previousSelectedIndex = selectedIndex.Value;
                        selectedIndex.Set(existingTabIndex);
                        tabId = tabs.Value[existingTabIndex].Id;
                        // Update browser URL when switching to existing tab
                        if (navigateArgs.Purpose is NavigationPurpose.NewDestination && previousSelectedIndex != existingTabIndex)
                        {
                            client.Redirect(navigateArgs.GetUrl(), replaceHistory, tabId: tabId);
                        }
                        return;
                    }
                }

                if (navigateArgs.Purpose is NavigationPurpose.NewDestination)
                {
                    var app = appRepository!.GetAppOrDefault(navigateArgs.AppId);
                    var newTabs = tabs.Value.Add(new TabState(tabId, app.Id, app.Title, appHost, app.Icon, Guid.NewGuid().ToString()));
                    tabs.Set(newTabs);
                    selectedIndex.Set(newTabs.Length - 1);

                    // Update browser URL when new tab is opened
                    client.Redirect(navigateArgs.GetUrl(), replaceHistory, tabId: tabId);
                }
            }
        }

        UseEffect(async () =>
        {
            if (auth != null)
            {
                var userInfo = await TimeoutHelper.WithTimeoutAsync(auth.GetUserInfoAsync);
                user.Set(userInfo);
            }

            var initialAppId = args.NavigationAppId ?? settings.DefaultAppId;
            if (!string.IsNullOrWhiteSpace(initialAppId))
            {
                OpenApp(new NavigateArgs(initialAppId), replaceHistory: true);
            }
            else
            {
                client.Redirect("/", replaceHistory: true);
            }
        });

        bool CheckTabExists(int tabId)
        {
            return tabId >= 0 && tabId < tabs.Value.Length;
        }

        void OnMenuSelect(Event<SidebarMenu, object> @event)
        {
            if (@event.Value is string appId)
            {
                OpenApp(new NavigateArgs(appId));
            }
        }

        ValueTask OnCtrlRightClickSelect(Event<SidebarMenu, object> @event)
        {
            if (@event.Value is string appId)
            {
                client.OpenUrl(new NavigateArgs(appId, Chrome: false).GetUrl());
            }
            return ValueTask.CompletedTask;
        }

        void OnTabSelect(Event<TabsLayout, int> @event)
        {
            if (!CheckTabExists(@event.Value))
            {
                return;
            }

            // Only update and redirect if the selected index actually changes
            if (selectedIndex.Value != @event.Value)
            {
                selectedIndex.Set(@event.Value);

                // Update browser URL when tab is selected
                var tab = tabs.Value[@event.Value];
                var navigateArgs = new NavigateArgs(tab.AppId);
                client.Redirect(navigateArgs.GetUrl(), tabId: tab.Id);
            }
        }

        void OnTabClose(Event<TabsLayout, int> @event)
        {
            if (!CheckTabExists(@event.Value))
            {
                return;
            }

            var closedIndex = @event.Value;
            var wasSelected = selectedIndex.Value == closedIndex;
            var newTabs = tabs.Value.RemoveAt(closedIndex);
            int? newIndex = null;
            if (newTabs.Length > 0)
            {
                if (wasSelected)
                {
                    newIndex = Math.Min(closedIndex, newTabs.Length - 1);
                }
                else if (selectedIndex.Value > closedIndex)
                {
                    newIndex = selectedIndex.Value - 1;
                }
                else
                {
                    newIndex = selectedIndex.Value;
                }
            }
            selectedIndex.Set(newIndex);

            // Update browser URL when current tab was closed
            if (wasSelected)
            {
                if (newIndex != null)
                {
                    var tab = newTabs[newIndex.Value];
                    var navigateArgs = new NavigateArgs(tab.AppId);
                    client.Redirect(navigateArgs.GetUrl(), tabId: tab.Id);
                }
                else
                {
                    client.Redirect("/");
                }
            }

            tabs.Set(newTabs);
        }

        void OnTabRefresh(Event<TabsLayout, int> @event)
        {
            if (!CheckTabExists(@event.Value))
            {
                return;
            }

            var tab = tabs.Value[@event.Value];
            tabs.Set(tabs.Value.RemoveAt(@event.Value).Insert(@event.Value, tab with { RefreshToken = Guid.NewGuid().ToString() }));
            selectedIndex.Set(@event.Value);
        }

        void OnTabReorder(Event<TabsLayout, int[]> @event)
        {
            var newOrder = @event.Value;
            // Reorder tabs according to the new indices
            var reorderedTabs = newOrder.Select(index => tabs.Value[index]).ToArray();
            tabs.Set([.. reorderedTabs]);

            // Update selected index to match the new position of the currently selected tab
            if (selectedIndex.Value.HasValue)
            {
                var oldSelectedIndex = selectedIndex.Value.Value;
                var newSelectedIndex = Array.IndexOf(newOrder, oldSelectedIndex);
                if (newSelectedIndex >= 0)
                {
                    selectedIndex.Set(newSelectedIndex);
                }
            }
        }

        object? body;

        if (settings.Navigation == ChromeNavigation.Pages)
        {
            body = currentApp.Value;
        }
        else
        {
            if (tabs.Value.Length == 0)
            {
                body = null;
                if (settings.WallpaperAppId != null)
                {
                    body = new AppHost(settings.WallpaperAppId, null, args.ConnectionId);
                }
            }
            else
            {
                body = new TabsLayout(OnTabSelect, OnTabClose, OnTabRefresh, OnTabReorder, selectedIndex.Value,
                    tabs.Value.ToArray().Select(e => e.ToTab()).ToArray()
                ).RemoveParentPadding().Variant(TabsVariant.Tabs).Padding(0);
            }
        }

        var searchInput = search.ToSearchInput().ShortcutKey("CTRL+K").TestId("sidebar-search");
        var sidebarMenu = new SidebarMenu(
            OnMenuSelect,
            menuItems.Value
        )
        {
            OnCtrlRightClickSelect = OnCtrlRightClickSelect,
            SearchActive = !string.IsNullOrWhiteSpace(search.Value)
        };

        var commonMenuItems = new[]
        {
            MenuItem.Default("Star on Github").Tag("$github").Icon(Icons.Github)
                .HandleSelect(() => client.OpenUrl(Resources.IvyGitHubUrl)),
            MenuItem.Default("Theme")
                .Tag("$theme")
                .Icon(Icons.SunMoon)
                .Children(
                    MenuItem.Checkbox("Light").Icon(Icons.Sun).HandleSelect(() => client.SetThemeMode(ThemeMode.Light)),
                    MenuItem.Checkbox("Dark").Icon(Icons.Moon).HandleSelect(() => client.SetThemeMode(ThemeMode.Dark)),
                    MenuItem.Checkbox("System").Icon(Icons.SunMoon).HandleSelect(() => client.SetThemeMode(ThemeMode.System))
                )
        };

        DropDownMenu? footer;
        if (user.Value != null)
        {
            var trigger = new Button().Variant(ButtonVariant.Ghost)
                .Content(
                    Layout.Horizontal().Align(Align.Left).Width(Size.Full())
                        | new Avatar(user.Value.Initials, user.Value.AvatarUrl)
                        | (Layout.Vertical().Gap(1)
                           | (user.Value.FullName != null
                               ? Text.Muted(user.Value.FullName!).Overflow(Overflow.Ellipsis)
                               : null!)
                           | Text.Label(user.Value.Email).Overflow(Overflow.Ellipsis))
                        .Grow()
                        .Size(Size.Full().Min(0))
                ).Width(Size.Full());

            footer = new DropDownMenu(
                    DropDownMenu.DefaultSelectHandler(),
                    trigger)
                .Top();

            var onLogout = new Action(async () =>
            {
                try
                {
                    if (auth == null) return;

                    await TimeoutHelper.WithTimeoutAsync(auth.LogoutAsync);
                    client.SetAuthToken(null!);
                }
                catch (Exception)
                {
                    //ignore
                }
            });

            footer = footer.Items(settings.FooterMenuItemsTransformer([
                ..commonMenuItems, MenuItem.Default("Logout").Tag("$logout").Icon(Icons.LogOut).HandleSelect(onLogout)
            ], navigator));
        }
        else
        {
            var trigger = new Button("Settings")
                .Content(
                    Layout.Horizontal().Align(Align.Left)
                        | Icons.Settings.ToIcon()
                        | Text.Muted("Settings")
                    )
                    .Variant(ButtonVariant.Ghost).Width(Size.Full());

            footer = new DropDownMenu(
                    DropDownMenu.DefaultSelectHandler(),
                    trigger)
                .Top()
                .Items(
                    settings.FooterMenuItemsTransformer(commonMenuItems, navigator)
                );
        }

        return new SidebarLayout(
            body ?? null!,
            sidebarMenu,
            Layout.Vertical().Gap(2)
                | settings.Header
                | searchInput
            ,
            Layout.Vertical(
                new SidebarNews("https://ivy.app/news.json"),
                settings.Footer,
                footer
            )
        ).MainAppSidebar(true);
    }
}