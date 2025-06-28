using System.Collections.Immutable;
using Ivy.Apps;
using Ivy.Auth;
using Ivy.Client;
using Ivy.Core;
using Ivy.Helpers;
using Ivy.Hooks;
using Ivy.Shared;
using Ivy.Widgets.Internal;

namespace Ivy.Chrome;

[App(isVisible:false, removeIvyBranding:true)]
public class DefaultSidebarChrome(ChromeSettings settings) : ViewBase
{
    private record TabState(string Title, string Url, Icons? Icon, long RefreshToken)
    {
        public Tab ToTab() => new Tab(Title, new Iframe(Url, refreshToken:RefreshToken)).Icon(Icon);
    }
    
    public override object? Build()
    {
        var tabs = UseState(ImmutableArray.Create<TabState>);
        var selectedIndex = UseState<int?>();
        var appRepository = UseService<IAppRepository>();
        var client = UseService<IClientProvider>();
        var auth = UseService<IAuthService?>();
        var user = UseState<UserInfo?>();
        var currentPage = UseState<string?>();
        var search = UseState<string>("");
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
                
                menuItems.Set([MenuItem.Default("Search Results").Children(result)]);
            }
        }, [ search ]);
            
        //////////////////////////////////////////////////////////
        // var isIvyAgentStarting = UseState(false);
        // var isIvyAgentRunning = UseState(false);
        // //todo: remove the ivy agent server -> replace with something else
        // UseEffect(async () =>
        // {
        //     var cleanup = await IvyAgentServer.StartAsync(); 
        //     client?.SetChatPanelUrl("http://localhost:5001");
        //     isIvyAgentRunning.Set(true);
        //     return cleanup;
        // }, [ isIvyAgentStarting ]);
        //////////////////////////////////////////////////////////
        
        void OpenApp(NavigateArgs navigateArgs)
        {
            var app = appRepository!.GetAppOrDefault(navigateArgs.AppId);
            if(settings.Navigation == ChromeNavigation.Pages)
            {
                currentPage.Set(navigateArgs.GetUrl(args.ConnectionId));
            }
            else
            {
                tabs.Set(tabs.Value.Add(new TabState(app.Title, navigateArgs.GetUrl(args.ConnectionId), app.Icon, DateTime.UtcNow.Ticks)));
                selectedIndex.Set(tabs.Value.Length - 1);
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
                client?.OpenUrl(new NavigateArgs(appId).GetUrl());
            }
        }
        
        void OnTabSelect(Event<TabsLayout, int> @event)
        {
            selectedIndex.Set(@event.Value);
        }
        
        void OnTabClose(Event<TabsLayout, int> @event)
        {
            //[0,1,|2|,3] -> 2
            //[0,1,|2|] -> 1
            //[0,|1|] -> 0
            //[|0|] -> null
            var newIndex = Math.Min(@event.Value, tabs.Value.Length - 2);
            selectedIndex.Set(newIndex >= 0 ? newIndex : (int?)null);
            tabs.Set(tabs.Value.RemoveAt(@event.Value));
        }
        
        void OnTabRefresh(Event<TabsLayout, int> @event)
        {
            var tab = tabs.Value[@event.Value];
            tabs.Set(tabs.Value.RemoveAt(@event.Value).Insert(@event.Value, tab with {RefreshToken = DateTime.UtcNow.Ticks}));
            selectedIndex.Set(@event.Value);
        }

        object? body = null;
        
        if(settings.Navigation == ChromeNavigation.Pages)
        {
            body = new Iframe(currentPage.Value!, 0);
        }
        else
        {
            body = new TabsLayout(OnTabSelect, OnTabClose, OnTabRefresh, selectedIndex.Value,
                tabs.Value.ToArray().Select(e => e.ToTab()).ToArray()
            ).RemoveParentPadding().Variant(TabsVariant.Tabs).Padding(0);
        }

        var searchInput = search.ToSearchInput().ShortcutKey("CTRL+K");
        var sidebarMenu = new SidebarMenu(
            OnMenuSelect,
            menuItems.Value
        )
        {
            OnCtrlRightClickSelect = OnCtrlRightClickSelect
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
            body,
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