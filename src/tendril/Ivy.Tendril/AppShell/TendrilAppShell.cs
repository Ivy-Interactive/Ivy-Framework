using System.Collections.Immutable;
using System.Reactive.Disposables;
using Ivy.Core;
using Ivy.Core.Apps;
using Ivy.Tendril.AppShell.Dialogs;
using Ivy.Tendril.Apps;
using Ivy.Tendril.Services;
using Ivy.Tendril.Views;
using Ivy.Widgets.Internal;

namespace Ivy.Tendril.AppShell;

#pragma warning disable IVYAPP001
#pragma warning disable IVYHOOK005

public class TendrilAppShell(AppShellSettings settings) : ViewBase
{
    internal AppShellSettings Settings => settings;

    private static MenuItem AddBadge(MenuItem item, Dictionary<string, int> badges)
    {
        if (item.Tag is string tag && badges.TryGetValue(tag, out var count) && count > 0)
            item = item.Badge(count.ToString());
        if (item.Children is { Length: > 0 })
            item = item with { Children = item.Children.Select(c => AddBadge(c, badges)).ToArray() };
        return item;
    }

    private static MenuItem[] BuildMenuItems(IAppRepository repo, PlanCounts planCounts)
    {
        var badges = new Dictionary<string, int>
        {
            ["plans"] = planCounts.Drafts,
            ["review"] = planCounts.Reviews,
            ["jobs"] = planCounts.ActiveJobs,
            ["icebox"] = planCounts.Icebox,
            ["recommendations"] = planCounts.Recommendations
        };
        return repo.GetMenuItems().Select(m => AddBadge(m, badges)).ToArray();
    }

    public override object Build()
    {
        // All hooks must be at the top level of Build()
        var config = UseService<IConfigService>();
        var tabs = UseState(ImmutableArray.Create<TabState>);
        var selectedIndex = UseState<int?>();
        var appRepository = UseService<IAppRepository>();
        var client = UseService<IClientProvider>();
        Context.TryUseService<IAuthService>(out var auth);
        var user = UseState<UserInfo?>();
        var currentApp = UseState<AppHost?>();
        var countsService = UseService<IPlanCountsService>();
        var menuItems = UseState(() => BuildMenuItems(appRepository, countsService.Current));
        var counts = UseState(() => countsService.Current);
        var sidebarOpen = UseState(settings.SidebarOpen);
        var args = UseService<AppContext>();
        var serverArgs = UseService<ServerArgs>();
        var navigate = Context.UseSignal<NavigateSignal, NavigateArgs, Unit>();
        var navigator = UseNavigation();
        var importIssuesDialogOpen = UseState(false);
        UseEffect(() =>
        {
            void OnChanged()
            {
                counts.Set(countsService.Current);
            }

            countsService.CountsChanged += OnChanged;
            return Disposable.Create(() => countsService.CountsChanged -= OnChanged);
        });

        UseEffect(() =>
        {
            return navigate.Receive(navigateArgs =>
            {
                OpenApp(navigateArgs);
                return default!;
            });
        });

        UseEffect(() => { menuItems.Set(BuildMenuItems(appRepository, counts.Value)); },
            appRepository.Reloaded.ToTrigger(), counts);

        UseEffect(async () =>
        {
            if (config.NeedsOnboarding) return;

            if (auth != null)
            {
                var userInfo = await auth.GetUserInfoAsync();
                user.Set(userInfo);
            }

            var initialAppId = args.NavigationAppId ?? settings.DefaultAppId;
            var targetAppId = initialAppId;
            if (!string.IsNullOrWhiteSpace(targetAppId))
            {
                // Force redirect from onboarding if it's already done
                if (!config.NeedsOnboarding && (targetAppId.Equals("onboarding", StringComparison.OrdinalIgnoreCase) ||
                                                targetAppId.Equals("OnboardingApp",
                                                    StringComparison.OrdinalIgnoreCase) ||
                                                targetAppId.Equals("onboarding-app",
                                                    StringComparison.OrdinalIgnoreCase)))
                    targetAppId = settings.DefaultAppId;

                var appArgs = args.GetArgs<object>();
                OpenApp(new NavigateArgs(targetAppId, appArgs), true);
            }
            else
            {
                client.Redirect("/", true);
            }
        });

        // Auto-default: if there's exactly one visible app, select it and close sidebar
        var visibleApps = appRepository.GetMenuItems().FlattenWithPath().ToArray();
        if (visibleApps is [{ Item.Tag: string singleAppId } _])
            settings = settings with
            {
                DefaultAppId = settings.DefaultAppId ?? singleAppId,
                SidebarOpen = false
            };

        void SetAppTitle(string appId)
        {
            var app = appRepository.GetAppOrDefault(appId);
            if (app.Title is { } title) client.SetTitle(title, serverArgs.Metadata.Title);
        }

        bool IsErrorApp(string? appId)
        {
            return appId != null && appRepository.GetAppOrDefault(appId).Id == AppIds.ErrorNotFound;
        }

        void RedirectToAppIfNotError(NavigateArgs navigateArgs, bool replaceHistory = false, string? tabId = null)
        {
            if (IsErrorApp(navigateArgs.AppId)) return;
            client.Redirect(navigateArgs.GetUrl(), replaceHistory, tabId);
        }

        void OpenApp(NavigateArgs navigateArgs, bool replaceHistory = false)
        {
            try
            {
                if (settings.Navigation == AppShellNavigation.Pages)
                {
                    var previousApp = currentApp.Value?.AppId;
                    var effectiveNavigateArgs = navigateArgs.AppId == null
                        ? navigateArgs with { AppId = settings.DefaultAppId }
                        : navigateArgs;

                    var appHost = effectiveNavigateArgs.AppId != null
                        ? effectiveNavigateArgs.ToAppHost(args.ConnectionId)
                        : null;

                    currentApp.Set(appHost);

                    if (effectiveNavigateArgs.AppId != null) SetAppTitle(effectiveNavigateArgs.AppId);

                    if (effectiveNavigateArgs.HistoryOp is HistoryOp.Push && previousApp != effectiveNavigateArgs.AppId)
                        RedirectToAppIfNotError(effectiveNavigateArgs, replaceHistory);
                }
                else
                {
                    if (!string.IsNullOrEmpty(navigateArgs.TabId))
                    {
                        var tabIndex = tabs.Value.ToList().FindIndex(t => t.Id == navigateArgs.TabId);
                        if (tabIndex >= 0)
                        {
                            selectedIndex.Set(tabIndex);
                            var tab = tabs.Value[tabIndex];
                            SetAppTitle(tab.AppId);

                            if (navigateArgs.HistoryOp is HistoryOp.Push)
                                RedirectToAppIfNotError(navigateArgs, replaceHistory, tab.Id);
                            return;
                        }

                        if (navigateArgs.HistoryOp is HistoryOp.Pop)
                        {
                            client.Error(new InvalidOperationException("Tab no longer exists."));
                            return;
                        }
                    }

                    if (navigateArgs.AppId == null) return;

                    var tabId = Guid.NewGuid().ToString();
                    var appHost = navigateArgs.ToAppHost(args.ConnectionId);

                    if (settings.PreventTabDuplicates)
                    {
                        var targetAppId = navigateArgs.AppId;
                        var appDescriptor = appRepository.GetApp(targetAppId);
                        if (appDescriptor?.AllowDuplicateTabs != true)
                        {
                            var existingTabIndex = -1;
                            for (var i = 0; i < tabs.Value.Length; i++)
                                if (tabs.Value[i].AppId == targetAppId)
                                {
                                    existingTabIndex = i;
                                    break;
                                }

                            if (existingTabIndex >= 0)
                            {
                                var previousSelectedIndex = selectedIndex.Value;
                                selectedIndex.Set(existingTabIndex);
                                var existingTabId = tabs.Value[existingTabIndex].Id;
                                SetAppTitle(targetAppId);

                                if (navigateArgs.HistoryOp is HistoryOp.Push &&
                                    previousSelectedIndex != existingTabIndex)
                                    RedirectToAppIfNotError(navigateArgs, replaceHistory, existingTabId);
                                return;
                            }
                        }
                    }

                    if (navigateArgs.HistoryOp is HistoryOp.Push)
                    {
                        var app = appRepository!.GetAppOrDefault(navigateArgs.AppId);
                        var newTabs = tabs.Value.Add(new TabState(tabId, app.Id, app.Title, appHost, app.Icon,
                            Guid.NewGuid().ToString()));
                        tabs.Set(newTabs);
                        selectedIndex.Set(newTabs.Length - 1);
                        SetAppTitle(app.Id);
                        RedirectToAppIfNotError(navigateArgs, replaceHistory, tabId);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] TendrilAppShell.OpenApp failed for {navigateArgs.AppId}: {ex}");
            }
        }

        bool CheckTabExists(int tabId)
        {
            return tabId >= 0 && tabId < tabs.Value.Length;
        }

        void OnMenuSelect(Event<SidebarMenu, object> @event)
        {
            if (@event.Value is string appId) OpenApp(new NavigateArgs(appId));
        }

        ValueTask OnCtrlRightClickSelect(Event<SidebarMenu, object> @event)
        {
            if (@event.Value is string appId) client.OpenUrl(new NavigateArgs(appId, AppShell: false).GetUrl());
            return ValueTask.CompletedTask;
        }

        void OnTabSelect(Event<TabsLayout, int> @event)
        {
            if (!CheckTabExists(@event.Value)) return;

            if (selectedIndex.Value != @event.Value)
            {
                selectedIndex.Set(@event.Value);
                var tab = tabs.Value[@event.Value];
                SetAppTitle(tab.AppId);
                RedirectToAppIfNotError(new NavigateArgs(tab.AppId), tabId: tab.Id);
            }
        }

        void OnTabClose(Event<TabsLayout, int> @event)
        {
            if (!CheckTabExists(@event.Value)) return;

            var closedIndex = @event.Value;
            var wasSelected = selectedIndex.Value == closedIndex;
            var newTabs = tabs.Value.RemoveAt(closedIndex);
            int? newIndex = null;
            if (newTabs.Length > 0)
            {
                if (wasSelected)
                    newIndex = Math.Min(closedIndex, newTabs.Length - 1);
                else if (selectedIndex.Value > closedIndex)
                    newIndex = selectedIndex.Value - 1;
                else
                    newIndex = selectedIndex.Value;
            }

            selectedIndex.Set(newIndex);

            if (wasSelected)
            {
                if (newIndex != null)
                {
                    var tab = newTabs[newIndex.Value];
                    SetAppTitle(tab.AppId);
                    RedirectToAppIfNotError(new NavigateArgs(tab.AppId), tabId: tab.Id);
                }
                else
                {
                    client.SetTitle(serverArgs.Metadata.Title);
                    client.Redirect("/");
                    sidebarOpen.Set(true);
                }
            }

            tabs.Set(newTabs);
        }

        void OnTabRefresh(Event<TabsLayout, int> @event)
        {
            if (!CheckTabExists(@event.Value)) return;

            var tab = tabs.Value[@event.Value];
            tabs.Set(tabs.Value.RemoveAt(@event.Value)
                .Insert(@event.Value, tab with { RefreshToken = Guid.NewGuid().ToString() }));
            selectedIndex.Set(@event.Value);
        }

        void OnTabReorder(Event<TabsLayout, int[]> @event)
        {
            var newOrder = @event.Value;
            var reorderedTabs = newOrder.Select(index => tabs.Value[index]).ToArray();
            tabs.Set([.. reorderedTabs]);

            if (selectedIndex.Value.HasValue)
            {
                var oldSelectedIndex = selectedIndex.Value.Value;
                var newSelectedIndex = Array.IndexOf(newOrder, oldSelectedIndex);
                if (newSelectedIndex >= 0) selectedIndex.Set(newSelectedIndex);
            }
        }

        object? body;

        if (settings.Navigation == AppShellNavigation.Pages)
        {
            body = currentApp.Value;
        }
        else
        {
            if (tabs.Value.Length == 0)
            {
                body = null;
                if (settings.WallpaperAppId != null)
                    body = new AppHost(settings.WallpaperAppId, null, args.ConnectionId);
            }
            else
            {
                body = new TabsLayout(OnTabSelect, OnTabClose, OnTabRefresh, OnTabReorder, selectedIndex.Value,
                    tabs.Value.ToArray().Select(e => e.ToTab()).ToArray()
                ).RemoveParentPadding().Variant(TabsVariant.Tabs).Padding(0);
            }
        }

        var sidebarMenu = new SidebarMenu(
            OnMenuSelect,
            menuItems.Value
        )
        {
            OnCtrlRightClickSelect = new EventHandler<Event<SidebarMenu, object>>(OnCtrlRightClickSelect)
        };

        var commonMenuItems = new[]
        {
            MenuItem.Default("Import Issues from GitHub")
                .Tag("$import-issues")
                .Icon(Icons.Download)
                .OnSelect(() => importIssuesDialogOpen.Set(true)),
            MenuItem.Default("Setup")
                .Tag("$setup")
                .Icon(Icons.Construction)
                .OnSelect(() => navigator.Navigate<SetupApp>()),
            MenuItem.Default("Trash")
                .Tag("$trash")
                .Icon(Icons.Trash2)
                .OnSelect(() => navigator.Navigate<TrashApp>()),
            MenuItem.Default("Theme")
                .Tag("$theme")
                .Icon(Icons.SunMoon)
                .Children(
                    MenuItem.Checkbox("Light").Icon(Icons.Sun).OnSelect(() => client.SetThemeMode(ThemeMode.Light)),
                    MenuItem.Checkbox("Dark").Icon(Icons.Moon).OnSelect(() => client.SetThemeMode(ThemeMode.Dark)),
                    MenuItem.Checkbox("System").Icon(Icons.SunMoon)
                        .OnSelect(() => client.SetThemeMode(ThemeMode.System))
                ),
            MenuItem.Default("Open config.yaml")
                .Tag("$open-config")
                .Icon(Icons.FileText)
                .OnSelect(() => { config.OpenInEditor(config.ConfigPath); })
        };

        var authSession = auth?.GetAuthSession();
        var isLoggedIn = authSession != null;

        void OnLogout()
        {
            _ = LogoutAsync();
        }

        async Task LogoutAsync()
        {
            if (auth == null) return;

            try
            {
                await auth.LogoutAsync();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Logout failed: {ex}");
            }
        }

        DropDownMenu? footer;
        if (user.Value != null)
        {
            var trigger = new Button().Variant(ButtonVariant.Ghost)
                .Content(
                    Layout.Horizontal().AlignContent(Align.Left).Width(Size.Full())
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

            footer = footer.Items(settings.FooterMenuItemsTransformer([
                ..commonMenuItems, MenuItem.Default("Logout").Tag("$logout").Icon(Icons.LogOut).OnSelect(OnLogout)
            ], navigator));
        }
        else
        {
            var trigger = new Button("Settings")
                .Content(
                    Layout.Horizontal().AlignContent(Align.Left)
                    | Icons.Settings.ToIcon()
                    | Text.P("Settings").Small().Muted()
                )
                .Variant(ButtonVariant.Ghost).Width(Size.Full());

            var footerMenuItems = isLoggedIn
                ? [.. commonMenuItems, MenuItem.Default("Logout").Tag("$logout").Icon(Icons.LogOut).OnSelect(OnLogout)]
                : commonMenuItems;

            footer = new DropDownMenu(
                    DropDownMenu.DefaultSelectHandler(),
                    trigger)
                .Top()
                .Items(
                    settings.FooterMenuItemsTransformer(footerMenuItems, navigator)
                );
        }

        if (config.NeedsOnboarding) return new OnboardingApp();

        return new Fragment(
            new SidebarLayout(
                body ?? null!,
                sidebarMenu,
                Layout.Vertical().Gap(2)
                | settings.Header
                | new NewPlanButton()
                ,
                Layout.Vertical(
                    new SidebarNews("https://ivy.app/news.json"),
                    settings.Footer,
                    footer
                ),
                settings.Width
            ).Open(sidebarOpen.Value).MainAppSidebar(),
            new ImportIssuesDialog(importIssuesDialogOpen, config)
        );
    }

    private record TabState(string Id, string AppId, string Title, AppHost AppHost, Icons? Icon, string RefreshToken)
    {
        public Tab ToTab()
        {
            return new Tab(Title, AppHost).Icon(Icon).Key(StringHelper.GetShortHash(Id + RefreshToken));
        }
    }
}
