using System.Collections.Immutable;
using Ivy.Apps;
using Ivy.Auth;
using Ivy.Client;
using Ivy.Core;
using Ivy.Hooks;
using Ivy.Shared;
using Ivy.Views;
using Ivy.Widgets.Internal;

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
                var result = appRepository.GetMenuItems().Flatten().Where(e =>
                    (e.Label ?? "").StartsWith(search.Value, StringComparison.OrdinalIgnoreCase)).ToArray();

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

        void OpenApp(NavigateArgs navigateArgs)
        {
            var app = appRepository!.GetAppOrDefault(navigateArgs.AppId);
            if (settings.Navigation == ChromeNavigation.Pages)
            {
                currentApp.Set(navigateArgs.ToAppHost(args.ConnectionId));
            }
            else
            {
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
                        selectedIndex.Set(existingTabIndex);
                        return;
                    }
                }

                var newTabs = tabs.Value.Add(new TabState(tabId, app.Id, app.Title, appHost, app.Icon, Guid.NewGuid().ToString()));
                tabs.Set(newTabs);
                selectedIndex.Set(newTabs.Length - 1);
            }
        }

        UseEffect(async () =>
        {
            if (auth != null)
            {
                user.Set(await auth.GetUserInfoAsync());
            }
            if (!string.IsNullOrEmpty(settings.DefaultAppId))
            {
                OpenApp(new NavigateArgs(settings.DefaultAppId));
            }
        });

        void OnMenuSelect(Event<SidebarMenu, object> @event)
        {
            if (@event.Value is string appId)
            {
                OpenApp(new NavigateArgs(appId));
            }
        }

        void OnCtrlRightClickSelect(Event<SidebarMenu, object> @event)
        {
            if (@event.Value is string appId)
            {
                client.OpenUrl(new NavigateArgs(appId).GetUrl());
            }
        }

        void OnTabSelect(Event<TabsLayout, int> @event)
        {
            selectedIndex.Set(@event.Value);
        }

        void OnTabClose(Event<TabsLayout, int> @event)
        {
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
            tabs.Set(newTabs);
        }

        void OnTabRefresh(Event<TabsLayout, int> @event)
        {
            var tab = tabs.Value[@event.Value];
            tabs.Set(tabs.Value.RemoveAt(@event.Value).Insert(@event.Value, tab with { RefreshToken = Guid.NewGuid().ToString() }));
            selectedIndex.Set(@event.Value);
        }

        object? body;

        if (settings.Navigation == ChromeNavigation.Pages)
        {
            body = currentApp.Value;
        }
        else
        {
            body = new TabsLayout(OnTabSelect, OnTabClose, OnTabRefresh, selectedIndex.Value,
                tabs.Value.ToArray().Select(e => e.ToTab()).ToArray()
            ).RemoveParentPadding().Variant(TabsVariant.Tabs).Padding(0);
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
            MenuItem.Default("Star on Github").Icon(Icons.Github)
                .HandleSelect(() => client.OpenUrl(Resources.IvyGitHubUrl)),
            MenuItem.Default("Theme")
                .Icon(Icons.SunMoon)
                .Children(
                    MenuItem.Checkbox("Light").Icon(Icons.Sun).HandleSelect(() => client.SetTheme(Theme.Light)),
                    MenuItem.Checkbox("Dark").Icon(Icons.Moon).HandleSelect(() => client.SetTheme(Theme.Dark)),
                    MenuItem.Checkbox("System").Icon(Icons.SunMoon).HandleSelect(() => client.SetTheme(Theme.System))
                )
        };

        DropDownMenu? footer;
        if (user.Value != null)
        {
            var trigger = new Button().Variant(ButtonVariant.Ghost)
                .Content(
                    Layout.Horizontal().Align(Align.Left).Width(Size.Full())
                        | new Avatar(user.Value.Initials)
                        | (Layout.Vertical().Gap(1)
                           | (user.Value.FullName != null
                               ? Text.Muted(user.Value.FullName!).Overflow(Overflow.Ellipsis)
                               : null!)
                           | Text.Small(user.Value.Email).Overflow(Overflow.Ellipsis))
                        .Grow()
                        .Size(Size.Full().Min(0))
                        | Icons.ChevronsUpDown
                ).Width(Size.Full());

            footer = new DropDownMenu(
                    DropDownMenu.DefaultSelectHandler(),
                    trigger)
                .Top();

            var onLogout = new Action(async void () =>
            {
                try
                {
                    if (auth == null) return;
                    await auth.LogoutAsync();
                    client.SetJwt(null!);
                }
                catch (Exception)
                {
                    //ignore
                }
            });

            footer = footer.Items([
                ..commonMenuItems, MenuItem.Default("Logout").Icon(Icons.LogOut).HandleSelect(onLogout)
            ]);
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
                    commonMenuItems
                );
        }

        return new SidebarLayout(
            body ?? null!,
            sidebarMenu,
            Layout.Vertical()
                | settings.Header
                | searchInput
            ,
            Layout.Vertical(
                new SidebarNews("https://ivy.app/news.json"),
                settings.Footer,
                footer
            )
        );
    }
}