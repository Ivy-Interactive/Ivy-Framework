using Ivy;
using Ivy.Core;
using Ivy.Core.Apps;
using Ivy.Core.AppShell;
using Ivy.Core.Server;
using Ivy.Tendril.Apps;
using Ivy.Tendril.Apps.Jobs;
using Ivy.Tendril.Apps.Plans;
using Ivy.Tendril.Services;
using Ivy.Widgets.Internal;
using Ivy.Widgets.ScreenshotFeedback;
using System.Collections.Immutable;
using System.Reactive.Disposables;
using Ivy.Tendril.Views;
using AppContext = Ivy.AppContext;

namespace Ivy.Tendril.AppShell;

#pragma warning disable IVYAPP001
#pragma warning disable IVYHOOK005

public class TendrilAppShell(AppShellSettings settings) : ViewBase
{
    internal AppShellSettings Settings => settings;

    private record TabState(string Id, string AppId, string Title, AppHost AppHost, Icons? Icon, string RefreshToken)
    {
        public Tab ToTab() => new Tab(Title, AppHost).Icon(Icon).Key(StringHelper.GetShortHash(Id + RefreshToken));
    }

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
            ["jobs"] = planCounts.RunningJobs,
            ["icebox"] = planCounts.Icebox
        };
        return repo.GetMenuItems().Select(m => AddBadge(m, badges)).ToArray();
    }

    public override object? Build()
    {
        // Check if onboarding is needed first
        var config = UseService<ConfigService>();
        if (config.NeedsOnboarding)
        {
            return new OnboardingApp();
        }

        // All hooks must be at the top of Build()
        var tabs = UseState(ImmutableArray.Create<TabState>);
        var selectedIndex = UseState<int?>();
        var appRepository = UseService<IAppRepository>();
        var client = UseService<IClientProvider>();
        Context.TryUseService<IAuthService>(out var auth);
        var user = UseState<UserInfo?>();
        var currentApp = UseState<AppHost?>();
        var search = UseState("");
        var countsService = UseService<PlanCountsService>();
        var menuItems = UseState(() => BuildMenuItems(appRepository, countsService.Current));
        var counts = UseState(() => countsService.Current);
        var jobService = UseService<JobService>();
        var sidebarOpen = UseState(settings.SidebarOpen);
        var args = UseService<AppContext>();
        var serverArgs = UseService<ServerArgs>();
        var navigate = Context.UseSignal<NavigateSignal, NavigateArgs, Unit>();
        var navigator = UseNavigation();
        var feedbackOpen = UseState(false);
        var feedbackScreenshot = UseState<FileUpload<byte[]>?>();
        var feedbackUploadCtx = UseUpload(MemoryStreamUploadHandler.Create(feedbackScreenshot));

        UseEffect(() =>
        {
            void OnChanged() => counts.Set(countsService.Current);
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

        UseEffect(() =>
        {
            if (string.IsNullOrWhiteSpace(search.Value))
            {
                menuItems.Set(BuildMenuItems(appRepository, counts.Value));
            }
            else
            {
                var result = appRepository.GetMenuItems()
                    .FlattenWithPath()
                    .Select(x => new { x.Item, x.Path, Score = AppShellUtils.ItemMatchScore(x.Item, search.Value) })
                    .Where(x => x.Score > 0)
                    .OrderByDescending(x => x.Score)
                    .ThenBy(x => x.Item.Label)
                    .Select(x => x.Item with { Path = string.IsNullOrEmpty(x.Path) ? null : x.Path })
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
        }, search, appRepository.Reloaded.ToTrigger(), counts);

        UseEffect(async () =>
        {
            if (auth != null)
            {
                var userInfo = await auth.GetUserInfoAsync();
                user.Set(userInfo);
            }

            var initialAppId = args.NavigationAppId ?? settings.DefaultAppId;
            if (!string.IsNullOrWhiteSpace(initialAppId))
            {
                var appArgs = args.GetArgs<object>();
                OpenApp(new NavigateArgs(initialAppId, appArgs), replaceHistory: true);
            }
            else
            {
                client.Redirect("/", replaceHistory: true);
            }
        });

        // Auto-default: if there's exactly one visible app, select it and close sidebar
        var visibleApps = appRepository.GetMenuItems().FlattenWithPath().ToArray();
        if (visibleApps.Length == 1 && visibleApps[0].Item.Tag is string singleAppId)
        {
            settings = settings with
            {
                DefaultAppId = settings.DefaultAppId ?? singleAppId,
                SidebarOpen = false
            };
        }

        void SetAppTitle(string appId)
        {
            var app = appRepository.GetAppOrDefault(appId);
            if (app.Title is { } title)
            {
                client.SetTitle(title, serverArgs.Metadata.Title);
            }
        }

        bool IsErrorApp(string? appId) =>
            appId != null && appRepository.GetAppOrDefault(appId).Id == AppIds.ErrorNotFound;

        void RedirectToAppIfNotError(NavigateArgs navigateArgs, bool replaceHistory = false, string? tabId = null)
        {
            if (IsErrorApp(navigateArgs.AppId)) return;
            client.Redirect(navigateArgs.GetUrl(), replaceHistory, tabId: tabId);
        }

        void OpenApp(NavigateArgs navigateArgs, bool replaceHistory = false)
        {
            try
            {
                if (settings.Navigation == AppShellNavigation.Pages)
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

                    if (navigateArgs.AppId != null)
                    {
                        SetAppTitle(navigateArgs.AppId);
                    }

                    if (navigateArgs.HistoryOp is HistoryOp.Push && previousApp != navigateArgs.AppId)
                    {
                        RedirectToAppIfNotError(navigateArgs, replaceHistory);
                    }
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
                            {
                                RedirectToAppIfNotError(navigateArgs, replaceHistory, tab.Id);
                            }
                            return;
                        }

                        if (navigateArgs.HistoryOp is HistoryOp.Pop)
                        {
                            client.Error(new InvalidOperationException("Tab no longer exists."));
                            return;
                        }
                    }

                    if (navigateArgs.AppId == null)
                    {
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
                            SetAppTitle(appId);

                            if (navigateArgs.HistoryOp is HistoryOp.Push && previousSelectedIndex != existingTabIndex)
                            {
                                RedirectToAppIfNotError(navigateArgs, replaceHistory, tabId);
                            }
                            return;
                        }
                    }

                    if (navigateArgs.HistoryOp is HistoryOp.Push)
                    {
                        var app = appRepository!.GetAppOrDefault(navigateArgs.AppId);
                        var newTabs = tabs.Value.Add(new TabState(tabId, app.Id, app.Title, appHost, app.Icon, Guid.NewGuid().ToString()));
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
            if (@event.Value is string appId)
            {
                OpenApp(new NavigateArgs(appId));
            }
        }

        ValueTask OnCtrlRightClickSelect(Event<SidebarMenu, object> @event)
        {
            if (@event.Value is string appId)
            {
                client.OpenUrl(new NavigateArgs(appId, AppShell: false).GetUrl());
            }
            return ValueTask.CompletedTask;
        }

        void OnTabSelect(Event<TabsLayout, int> @event)
        {
            if (!CheckTabExists(@event.Value))
            {
                return;
            }

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
            var reorderedTabs = newOrder.Select(index => tabs.Value[index]).ToArray();
            tabs.Set([.. reorderedTabs]);

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
            OnCtrlRightClickSelect = new(OnCtrlRightClickSelect),
            SearchActive = !string.IsNullOrWhiteSpace(search.Value)
        };

        var commonMenuItems = new[]
        {
            MenuItem.Default("Settings")
                .Tag("$setup")
                .Icon(Icons.Settings)
                .OnSelect(() => navigator.Navigate<SetupApp>()),
            MenuItem.Default("Tendril Feedback")
                .Tag("$feedback")
                .Icon(Icons.MessageSquare)
                .OnSelect(() => feedbackOpen.Set(true)),
            MenuItem.Default("Theme")
                .Tag("$theme")
                .Icon(Icons.SunMoon)
                .Children(
                    MenuItem.Checkbox("Light").Icon(Icons.Sun).OnSelect(() => client.SetThemeMode(ThemeMode.Light)),
                    MenuItem.Checkbox("Dark").Icon(Icons.Moon).OnSelect(() => client.SetThemeMode(ThemeMode.Dark)),
                    MenuItem.Checkbox("System").Icon(Icons.SunMoon).OnSelect(() => client.SetThemeMode(ThemeMode.System))
                )
        };

        var authSession = auth?.GetAuthSession();
        var isLoggedIn = authSession != null;

        var onLogout = new Action(async () =>
        {
            try
            {
                if (auth == null) return;
                await auth.LogoutAsync();
            }
            catch (Exception)
            {
            }
        });

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
                ..commonMenuItems, MenuItem.Default("Logout").Tag("$logout").Icon(Icons.LogOut).OnSelect(onLogout)
            ], navigator));
        }
        else
        {
            var trigger = new Button("Setup")
                .Content(
                    Layout.Horizontal().AlignContent(Align.Left)
                        | Icons.Construction.ToIcon()
                        | Text.P("Setup").Small().Muted()
                    )
                    .Variant(ButtonVariant.Ghost).Width(Size.Full());

            var footerMenuItems = isLoggedIn
                ? [.. commonMenuItems, MenuItem.Default("Logout").Tag("$logout").Icon(Icons.LogOut).OnSelect(onLogout)]
                : commonMenuItems;

            footer = new DropDownMenu(
                    DropDownMenu.DefaultSelectHandler(),
                    trigger)
                .Top()
                .Items(
                    settings.FooterMenuItemsTransformer(footerMenuItems, navigator)
                );
        }

        return new Fragment(
            new SidebarLayout(
                body ?? null!,
                sidebarMenu,
                Layout.Vertical().Gap(2)
                    | settings.Header
                    | searchInput
                    | new NewPlanFooterButton()
                ,
                Layout.Vertical(
                    new SidebarNews("https://ivy.app/news.json"),
                    settings.Footer,
                    footer
                ),
                settings.Width
            ).Open(sidebarOpen.Value).MainAppSidebar(true),
            ScreenshotFeedbackExtensions.OnCancel(
                ScreenshotFeedbackExtensions.OnSave(
                    new ScreenshotFeedback()
                        .UploadUrl(feedbackUploadCtx.Value.UploadUrl)
                        .Open(feedbackOpen.Value),
                    data =>
                    {
                        feedbackOpen.Set(false);

                        if (feedbackScreenshot.Value?.Content != null)
                        {
                            var tempPath = Path.Combine(Path.GetTempPath(), $"tendril-feedback-{DateTime.UtcNow:yyyyMMdd-HHmmss}.png");
                            File.WriteAllBytes(tempPath, feedbackScreenshot.Value.Content);

                            var texts = data.Shapes
                                .Select(s => s switch
                                {
                                    CalloutAnnotation c => $"[{c.Number}] {c.Text}",
                                    TextAnnotation t => t.Text,
                                    _ => null
                                })
                                .Where(t => !string.IsNullOrWhiteSpace(t))
                                .ToList();

                            var description = string.Join("\n", texts);
                            if (string.IsNullOrWhiteSpace(description))
                                description = "Visual feedback";

                            description = $"Screenshot feedback:\n\n{description}\n\nScreenshot: {tempPath}";

                            jobService.StartJob("MakePlan", "-Description", description, "-Project", "Tendril");
                        }

                        feedbackScreenshot.Set(null);
                    }),
                () => feedbackOpen.Set(false))
        );
    }
}
