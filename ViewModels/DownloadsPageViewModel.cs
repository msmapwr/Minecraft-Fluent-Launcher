using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WINUI.Models;
using WINUI.Services;

namespace WINUI.ViewModels;

/// <summary>
/// 下载中心页的视图模型（大更新 ⑧ 重构）。
/// <para>
/// 心智模型：「我想玩什么 → 选择 → 系统准备 → 完成后去玩」。
/// 进入页面先见<b>资源类型</b>（默认 Minecraft），类型内浏览 + 搜索 / 筛选 /
/// 真实安装状态；点「安装」先弹确认对话框，确认后任务进入<b>全局下载队列</b>
/// （<see cref="IDownloadQueueService"/>），离开页面不会中断。
/// </para>
/// <para>
/// 「版本」条目的安装走真实 CMLLib；其余条目在资源站接入（v0.3）前以演示任务跑通交互。
/// 列表带分页；页面状态（加载 / 内容 / 空 / 错误）由 <see cref="PageViewModelBase"/> 提供。
/// </para>
/// </summary>
public sealed partial class DownloadsPageViewModel : PageViewModelBase
{
    private readonly ILauncherDataService _dataService;
    private readonly IGameLauncherService _gameLauncher;
    private readonly IDownloadQueueService _queue;
    private readonly INavigationService _navigation;
    private readonly IInteractionService _interaction;

    /// <summary>全量条目（搜索 / 类型筛选的数据源）。</summary>
    private readonly List<DownloadItemViewModel> _allItems = [];

    /// <summary>筛选后的全部条目（分页前）。</summary>
    private readonly List<DownloadItemViewModel> _filteredItems = [];

    /// <summary>批量改页码时抑制重复刷新。</summary>
    private bool _suppressPageRefresh;

    /// <summary>当前资源类型（默认 Minecraft）。</summary>
    public DownloadCategory CurrentCategory { get; private set; } = DownloadCategory.GameVersion;

    /// <summary>搜索关键字。</summary>
    [ObservableProperty]
    public partial string SearchText { get; set; }

    /// <summary>当前下载源。</summary>
    [ObservableProperty]
    public partial SelectOption<DownloadSource> SelectedSource { get; set; }

    /// <summary>每页条数。</summary>
    [ObservableProperty]
    public partial SelectOption<int> SelectedPageSize { get; set; }

    /// <summary>当前页码（从 1 开始）。</summary>
    [ObservableProperty]
    public partial int CurrentPage { get; set; }

    /// <summary>总页数（至少为 1）。</summary>
    [ObservableProperty]
    public partial int TotalPages { get; set; }

    /// <summary>状态栏文案。</summary>
    [ObservableProperty]
    public partial string StatusMessage { get; set; }

    /// <summary>队列中正在推进的任务数（外壳入口与页内摘要共用）。</summary>
    [ObservableProperty]
    public partial int ActiveDownloadCount { get; set; }

    /// <summary>当前页的条目。</summary>
    public ObservableCollection<DownloadItemViewModel> Items { get; } = [];

    /// <summary>分页条上的数字页码。</summary>
    public ObservableCollection<PageButtonViewModel> PageButtons { get; } = [];

    /// <summary>资源类型一级入口（默认选中 Minecraft）。</summary>
    public IReadOnlyList<CategoryCardViewModel> CategoryCards { get; } =
    [
        new(DownloadCategory.GameVersion, "Minecraft", "IconPlayGlyph", "游戏版本，支持加装加载器"),
        new(DownloadCategory.Mod, "模组", "IconListGlyph", "扩展玩法与内容"),
        new(DownloadCategory.Modpack, "整合包", "IconEmptyGlyph", "一键安装的完整玩法包"),
        new(DownloadCategory.ResourcePack, "资源包", "IconTextureGlyph", "材质与音效"),
        new(DownloadCategory.Shader, "光影", "IconShaderGlyph", "画面增强"),
        new(DownloadCategory.World, "世界", "IconWorldGlyph", "地图与存档"),
        new(DownloadCategory.DataPack, "数据包", "IconLibraryGlyph", "原版玩法扩展"),
    ];

    /// <summary>可选下载源。</summary>
    public IReadOnlyList<SelectOption<DownloadSource>> Sources { get; } =
    [
        new(DownloadSource.Official, "官方源"),
        new(DownloadSource.Bmclapi, "BMCLAPI 镜像"),
        new(DownloadSource.Community, "社区镜像"),
    ];

    /// <summary>可选每页条数。</summary>
    public IReadOnlyList<SelectOption<int>> PageSizes { get; } =
    [
        new(10, "每页 10 条"),
        new(20, "每页 20 条"),
        new(50, "每页 50 条"),
    ];

    /// <summary>是否有条目（用于分页条的显示控制）。</summary>
    public bool HasItems => _filteredItems.Count > 0;

    /// <summary>分页条是否可见：仅在处于内容态且有条目时显示。</summary>
    public bool ShowPager => IsContent && HasItems;

    /// <summary>数据源本身是否为空（区别于「筛选后没有匹配」）。</summary>
    private bool HasNoItems => _allItems.Count == 0;

    /// <inheritdoc />
    public override string EmptyGlyph => HasNoItems ? "\uE7B8" : "\uE721";

    /// <inheritdoc />
    public override string EmptyTitle => HasNoItems ? "还没有可下载的内容" : "没有匹配的条目";

    /// <inheritdoc />
    public override string EmptyText => HasNoItems
        ? "数据源暂时没有返回任何条目，稍后重试或更换下载源。"
        : $"没有符合「{CategoryName(CurrentCategory)}」与当前关键字的条目，试试放宽筛选条件。";

    /// <summary>是否有推进中的下载任务（页内摘要条显示）。</summary>
    public bool IsDownloadActive => ActiveDownloadCount > 0;

    /// <summary>队列摘要（页内提示条）。</summary>
    public string DownloadSummary => IsDownloadActive
        ? $"正在下载 {ActiveDownloadCount} 个任务"
        : "没有正在进行的下载任务";

    /// <summary>是否可以翻到上一页。</summary>
    public bool CanGoPrevious => CurrentPage > 1;

    /// <summary>是否可以翻到下一页。</summary>
    public bool CanGoNext => CurrentPage < TotalPages;

    /// <summary>分页摘要。</summary>
    public string PageSummary => _filteredItems.Count == 0
        ? "没有条目"
        : $"第 {CurrentPage} / {TotalPages} 页 · 共 {_filteredItems.Count} 条";

    public DownloadsPageViewModel(
        ILauncherDataService dataService,
        IGameLauncherService gameLauncher,
        IDownloadQueueService queue,
        INavigationService navigation,
        IInteractionService interaction)
    {
        _dataService = dataService;
        _gameLauncher = gameLauncher;
        _queue = queue;
        _navigation = navigation;
        _interaction = interaction;

        SearchText = string.Empty;
        StatusMessage = "准备就绪";

        // 赋值顺序有讲究：会被 ApplyQuery 读取的属性必须先赋。
        SelectedSource = Sources[0];
        SelectedPageSize = PageSizes[0];

        // 默认选中 Minecraft 类型卡。
        foreach (var card in CategoryCards)
        {
            card.SetSelected(card.Category == CurrentCategory);
        }

        _queue.TaskAdded += OnQueueTaskAdded;
        _queue.TaskChanged += OnQueueTaskChanged;
        _queue.TaskRemoved += OnQueueTaskRemoved;
        ActiveDownloadCount = _queue.ActiveCount;

        _ = ReloadAsync();
    }

    /// <summary>队列任务加入：刷新条目安装状态与摘要。</summary>
    private void OnQueueTaskAdded(object? sender, DownloadTask task) => RequestQueueRefresh();

    /// <summary>队列任务变化：刷新条目安装状态与摘要。</summary>
    private void OnQueueTaskChanged(object? sender, DownloadTask task) => RequestQueueRefresh();

    /// <summary>队列任务移除：刷新条目安装状态与摘要。</summary>
    private void OnQueueTaskRemoved(object? sender, DownloadTask task) => RequestQueueRefresh();

    // 由队列事件 / 绑定回调触发的集合与状态重建，统一推迟到 Dispatcher 回调执行。
    private readonly BoundCollectionUpdater _listUpdater = new();
    private readonly BoundCollectionUpdater _pageUpdater = new();
    private readonly BoundCollectionUpdater _queueUpdater = new();

    /// <summary>请求重新计算筛选结果（推迟到 Dispatcher 回调执行）。</summary>
    private void RequestListRefresh() => _listUpdater.Request(ApplyQuery);

    /// <summary>请求重建当前页条目与分页条（推迟到 Dispatcher 回调执行）。</summary>
    private void RequestPageRefresh() => _pageUpdater.Request(RefreshPage);

    /// <summary>
    /// 队列状态变化后的统一刷新（推迟执行，读取当前状态）。
    /// </summary>
    private void RequestQueueRefresh() => _queueUpdater.Request(() =>
    {
        RefreshAllInstallStates();

        ActiveDownloadCount = _queue.ActiveCount;
        OnPropertyChanged(nameof(IsDownloadActive));
        OnPropertyChanged(nameof(DownloadSummary));
    });

    partial void OnSearchTextChanged(string value) => RequestListRefresh();

    partial void OnSelectedPageSizeChanged(SelectOption<int> value) => RequestListRefresh();

    partial void OnCurrentPageChanged(int value)
    {
        if (!_suppressPageRefresh)
        {
            RequestPageRefresh();
        }
    }

    /// <inheritdoc />
    protected override void OnStateChangedCore() => OnPropertyChanged(nameof(ShowPager));

    /// <summary>类型名。</summary>
    private static string CategoryName(DownloadCategory category) => category switch
    {
        DownloadCategory.GameVersion => "Minecraft",
        DownloadCategory.Mod => "模组",
        DownloadCategory.Modpack => "整合包",
        DownloadCategory.ResourcePack => "资源包",
        DownloadCategory.Shader => "光影",
        DownloadCategory.World => "世界",
        DownloadCategory.DataPack => "数据包",
        _ => "全部",
    };

    /// <summary>点击资源类型卡（code-behind 转发）。</summary>
    public void SelectCategory(CategoryCardViewModel card)
    {
        if (CurrentCategory == card.Category)
        {
            return;
        }

        CurrentCategory = card.Category;

        foreach (var candidate in CategoryCards)
        {
            candidate.SetSelected(candidate.Category == CurrentCategory);
        }

        StatusMessage = $"已切换到「{card.DisplayName}」";
        RequestListRefresh();
    }

    /// <summary>
    /// 从数据源重新载入条目。首次进入、错误状态下的「重试」与手动刷新共用此命令。
    /// </summary>
    [RelayCommand]
    private async Task ReloadAsync()
    {
        var loaded = await RunLoadAsync(async () =>
        {
            var items = await _dataService.GetDownloadItemsAsync();

            _allItems.Clear();
            _allItems.AddRange(items.Select(item => new DownloadItemViewModel(item)));
        }, "无法加载下载条目");

        if (!loaded)
        {
            StatusMessage = "下载条目加载失败";
            return;
        }

        RefreshAllInstallStates();
        ApplyQuery();
        StatusMessage = $"已载入 {_allItems.Count} 条内容 · 下载源：{SelectedSource.Value.ToLabel()}";
    }

    /// <summary>
    /// 重算全部条目的安装状态：「版本」条目按本地目录真实判定，
    /// 其余条目按队列中的演示任务判定（会话内状态，不入库）。
    /// </summary>
    private void RefreshAllInstallStates()
    {
        var activeKeys = _queue.Tasks
            .Where(task => task.Status.IsActive())
            .Select(task => task.Item.Id)
            .ToHashSet();

        var completedKeys = _queue.Tasks
            .Where(task => task.IsCompleted)
            .Select(task => task.Item.Id)
            .ToHashSet();

        foreach (var item in _allItems)
        {
            var installed = item.IsVersion
                ? _gameLauncher.IsInstalledLocally(item.Version)
                : completedKeys.Contains(item.Id);

            item.RefreshInstallState(installed, activeKeys.Contains(item.Id));
        }
    }

    /// <summary>按当前搜索 / 类型条件刷新列表，并回到第一页。</summary>
    private void ApplyQuery()
    {
        if (SelectedSource is null || SelectedPageSize is null)
        {
            return;
        }

        var query = _allItems.AsEnumerable();

        var keyword = SearchText is null ? string.Empty : SearchText.Trim();
        if (keyword.Length > 0)
        {
            query = query.Where(item =>
                item.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                item.Author.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                item.Description.Contains(keyword, StringComparison.OrdinalIgnoreCase));
        }

        query = query.Where(item => item.Item.Category == CurrentCategory);

        _filteredItems.Clear();
        _filteredItems.AddRange(query);

        TotalPages = Math.Max(1, (int)Math.Ceiling(_filteredItems.Count / (double)SelectedPageSize.Value));

        _suppressPageRefresh = true;
        CurrentPage = 1;
        _suppressPageRefresh = false;
        RequestPageRefresh();

        UpdateContentState(_filteredItems.Count > 0);

        OnPropertyChanged(nameof(HasItems));
        OnPropertyChanged(nameof(ShowPager));
        OnPropertyChanged(nameof(EmptyGlyph));
        OnPropertyChanged(nameof(EmptyTitle));
        OnPropertyChanged(nameof(EmptyText));

        StatusMessage = _filteredItems.Count == 0
            ? $"没有匹配「{CategoryName(CurrentCategory)}」的条目 · 下载源：{SelectedSource.Value.ToLabel()}"
            : $"共 {_filteredItems.Count} 条（{CategoryName(CurrentCategory)}）· 下载源：{SelectedSource.Value.ToLabel()}";
    }

    /// <summary>按当前页码重建当前页条目与分页条。</summary>
    private void RefreshPage()
    {
        var pageSize = SelectedPageSize?.Value ?? 10;

        Items.Clear();
        foreach (var item in _filteredItems.Skip((CurrentPage - 1) * pageSize).Take(pageSize))
        {
            Items.Add(item);
        }

        // 数字页码：最多显示 7 个，围绕当前页居中。
        PageButtons.Clear();
        var start = Math.Max(1, Math.Min(CurrentPage - 3, TotalPages - 6));
        var end = Math.Min(TotalPages, start + 6);
        for (var page = start; page <= end; page++)
        {
            PageButtons.Add(new PageButtonViewModel(page, page == CurrentPage));
        }

        OnPropertyChanged(nameof(PageSummary));
        OnPropertyChanged(nameof(CanGoPrevious));
        OnPropertyChanged(nameof(CanGoNext));
        PreviousPageCommand.NotifyCanExecuteChanged();
        NextPageCommand.NotifyCanExecuteChanged();
    }

    /// <summary>跳转到指定页码（越界会被收敛到有效范围）。</summary>
    public void GoToPage(int page)
    {
        var target = Math.Clamp(page, 1, Math.Max(1, TotalPages));
        if (target == CurrentPage)
        {
            return;
        }

        CurrentPage = target;
    }

    private bool CanGoPreviousPage() => CanGoPrevious;

    private bool CanGoNextPage() => CanGoNext;

    /// <summary>上一页。</summary>
    [RelayCommand(CanExecute = nameof(CanGoPreviousPage))]
    private void PreviousPage() => GoToPage(CurrentPage - 1);

    /// <summary>下一页。</summary>
    [RelayCommand(CanExecute = nameof(CanGoNextPage))]
    private void NextPage() => GoToPage(CurrentPage + 1);

    /// <summary>第一页。</summary>
    [RelayCommand]
    private void FirstPage() => GoToPage(1);

    /// <summary>最后一页。</summary>
    [RelayCommand]
    private void LastPage() => GoToPage(TotalPages);

    /// <summary>进入版本详情页（仅「版本」条目可用）。</summary>
    public void OpenDetail(DownloadItemViewModel item)
    {
        if (!item.IsVersion)
        {
            return;
        }

        _navigation.Navigate("version-detail", item.Item);
    }

    /// <summary>
    /// 确认对话框关闭后的入队入口（页面 code-behind 转发）。
    /// </summary>
    /// <param name="item">条目。</param>
    /// <param name="result">对话框结果（加载器 / 目标版本 / 实例名）。</param>
    public void ConfirmInstall(DownloadItemViewModel item, InstallDialogResult result)
    {
        var isReal = item.IsVersion;

        if (isReal && _gameLauncher.IsInstalledLocally(item.Version))
        {
            StatusMessage = $"「{item.Name}」已经安装";
            return;
        }

        var duplicate = _queue.Tasks.Any(task =>
            task.Item.Id == item.Id &&
            task.Status.IsActive() &&
            task.Loader == result.Loader);

        if (duplicate)
        {
            _interaction.Notify(
                $"「{item.Name}」正在下载中。",
                NotificationSeverity.Warning,
                "重复添加");
            return;
        }

        var task = _queue.Enqueue(
            item.Item,
            result.Loader,
            result.TargetGameVersion,
            result.TargetInstanceName,
            isDemo: !isReal);

        StatusMessage = isReal
            ? $"已开始安装：{item.Name}（可在右上角查看进度）"
            : $"已加入队列（演示）：{item.Name}";

        _interaction.Notify(
            isReal
                ? $"「{item.Name}」已开始安装，可随时离开本页。"
                : $"「{item.Name}」已加入队列（演示任务，不会下载文件）。",
            NotificationSeverity.Success,
            "已加入队列");
    }

    /// <summary>打开下载队列页（页内摘要条与外壳入口共用）。</summary>
    [RelayCommand]
    private void OpenQueue() => _navigation.Navigate("download-queue");
}
