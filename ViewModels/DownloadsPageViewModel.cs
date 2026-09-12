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
/// 下载中心页的视图模型。
/// <para>
/// 条目列表来自 <see cref="ILauncherDataService"/>（Mock）；下载队列为纯 UI 演示，
/// 以定时递增的进度模拟下载，<b>不会产生任何网络请求，也不会写入磁盘</b>。
/// </para>
/// <para>
/// 列表带分页：可切换每页条数（10 / 20 / 50）并通过数字页码、上一页 / 下一页翻页。
/// 页面状态（加载 / 内容 / 空 / 错误）由 <see cref="PageViewModelBase"/> 提供。
/// </para>
/// </summary>
public sealed partial class DownloadsPageViewModel : PageViewModelBase
{
    private readonly ILauncherDataService _dataService;
    private readonly INavigationService _navigation;

    /// <summary>全量条目（搜索 / 分类筛选的数据源）。</summary>
    private readonly List<DownloadItem> _allItems = [];

    /// <summary>筛选后的全部条目（分页前）。</summary>
    private readonly List<DownloadItem> _filteredItems = [];

    /// <summary>批量改页码时抑制重复刷新。</summary>
    private bool _suppressPageRefresh;

    /// <summary>搜索关键字。</summary>
    [ObservableProperty]
    public partial string SearchText { get; set; }

    /// <summary>当前分类（<c>null</c> 表示全部）。</summary>
    [ObservableProperty]
    public partial SelectOption<DownloadCategory?> SelectedCategory { get; set; }

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

    /// <summary>是否正在下载。</summary>
    [ObservableProperty]
    public partial bool IsDownloading { get; set; }

    /// <summary>整体队列进度（0–100）。</summary>
    [ObservableProperty]
    public partial double QueueProgress { get; set; }

    /// <summary>状态栏文案。</summary>
    [ObservableProperty]
    public partial string StatusMessage { get; set; }

    /// <summary>当前页的条目。</summary>
    public ObservableCollection<DownloadItem> Items { get; } = [];

    /// <summary>分页条上的数字页码。</summary>
    public ObservableCollection<PageButtonViewModel> PageButtons { get; } = [];

    /// <summary>下载队列。</summary>
    public ObservableCollection<DownloadTaskViewModel> Queue { get; } = [];

    /// <summary>可选分类。</summary>
    public IReadOnlyList<SelectOption<DownloadCategory?>> Categories { get; } =
    [
        new(null, "全部"),
        new(DownloadCategory.GameVersion, "版本"),
        new(DownloadCategory.Mod, "模组"),
        new(DownloadCategory.ResourcePack, "资源包"),
        new(DownloadCategory.Shader, "光影"),
        new(DownloadCategory.World, "世界"),
        new(DownloadCategory.DataPack, "数据包"),
        new(DownloadCategory.Modpack, "整合包"),
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
        : $"没有符合「{SelectedCategory?.DisplayName ?? "全部"}」与当前关键字的条目，试试放宽筛选条件。";

    /// <summary>队列是否为空。</summary>
    public bool IsQueueEmpty => Queue.Count == 0;

    /// <summary>是否可以翻到上一页。</summary>
    public bool CanGoPrevious => CurrentPage > 1;

    /// <summary>是否可以翻到下一页。</summary>
    public bool CanGoNext => CurrentPage < TotalPages;

    /// <summary>分页摘要。</summary>
    public string PageSummary => _filteredItems.Count == 0
        ? "没有条目"
        : $"第 {CurrentPage} / {TotalPages} 页 · 共 {_filteredItems.Count} 条";

    /// <summary>队列摘要。</summary>
    public string QueueSummary => Queue.Count == 0
        ? "队列为空"
        : $"{Queue.Count} 个任务 · 整体 {QueueProgress:0}%";

    /// <summary>破坏性操作的二次确认与操作结果通知。</summary>
    private readonly IInteractionService _interaction;

    public DownloadsPageViewModel(
        ILauncherDataService dataService,
        INavigationService navigation,
        IInteractionService interaction)
    {
        _dataService = dataService;
        _navigation = navigation;
        _interaction = interaction;

        SearchText = string.Empty;
        StatusMessage = "准备就绪";

        // 赋值顺序有讲究：会被 ApplyQuery 读取的属性必须先赋，
        // 最后再赋分类 —— 由它触发首次 ApplyQuery。
        SelectedSource = Sources[0];
        SelectedPageSize = PageSizes[0];
        SelectedCategory = Categories[0];

        Queue.CollectionChanged += OnQueueChanged;

        _ = ReloadAsync();
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
            _allItems.AddRange(items);
        }, "无法加载下载条目");

        if (!loaded)
        {
            StatusMessage = "下载条目加载失败";
            return;
        }

        ApplyQuery();
        StatusMessage = $"已载入 {_allItems.Count} 条内容 · 下载源：{SelectedSource.Value.ToLabel()}";
    }

    private void OnQueueChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        OnPropertyChanged(nameof(IsQueueEmpty));
        OnPropertyChanged(nameof(QueueSummary));
        StartQueueCommand.NotifyCanExecuteChanged();
    }

    // 由绑定回调（ComboBox 的 TwoWay 选中项、AutoSuggestBox 文本）触发的集合重建，
    // 必须推迟到 Dispatcher 回调里执行，否则可能与 XAML 布局冲突并抛出 COMException。
    private readonly BoundCollectionUpdater _listUpdater = new();
    private readonly BoundCollectionUpdater _pageUpdater = new();

    /// <summary>请求重新计算筛选结果（推迟到 Dispatcher 回调执行）。</summary>
    private void RequestListRefresh() => _listUpdater.Request(ApplyQuery);

    /// <summary>请求重建当前页条目与分页条（推迟到 Dispatcher 回调执行）。</summary>
    private void RequestPageRefresh() => _pageUpdater.Request(RefreshPage);

    partial void OnSearchTextChanged(string value) => RequestListRefresh();

    partial void OnSelectedCategoryChanged(SelectOption<DownloadCategory?> value) => RequestListRefresh();

    partial void OnSelectedPageSizeChanged(SelectOption<int> value) => RequestListRefresh();

    partial void OnCurrentPageChanged(int value)
    {
        if (!_suppressPageRefresh)
        {
            RequestPageRefresh();
        }
    }

    partial void OnQueueProgressChanged(double value) => OnPropertyChanged(nameof(QueueSummary));

    partial void OnIsDownloadingChanged(bool value) => StartQueueCommand.NotifyCanExecuteChanged();

    /// <inheritdoc />
    protected override void OnStateChangedCore() => OnPropertyChanged(nameof(ShowPager));

    /// <summary>按当前搜索 / 分类条件刷新列表，并回到第一页。</summary>
    private void ApplyQuery()
    {
        if (SelectedCategory is null || SelectedSource is null || SelectedPageSize is null)
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

        if (SelectedCategory.Value is { } category)
        {
            query = query.Where(item => item.Category == category);
        }

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
            ? $"没有匹配「{SelectedCategory.DisplayName}」的条目 · 下载源：{SelectedSource.Value.ToLabel()}"
            : $"共 {_filteredItems.Count} 条（{SelectedCategory.DisplayName}）· 下载源：{SelectedSource.Value.ToLabel()}";
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
    /// <param name="item">条目。</param>
    public void OpenDetail(DownloadItem item)
    {
        if (!item.IsVersion)
        {
            return;
        }

        _navigation.Navigate("version-detail", item);
    }

    /// <summary>把条目加入下载队列。</summary>
    public void Enqueue(DownloadItem item)
    {
        if (Queue.Any(task => task.Item.Id == item.Id && !task.IsCompleted))
        {
            StatusMessage = $"「{item.Name}」已在下载队列中";

            _interaction.Notify(
                $"「{item.Name}」已经在下载队列中了。",
                NotificationSeverity.Warning,
                "重复添加");

            return;
        }

        Queue.Add(new DownloadTaskViewModel(item));
        UpdateQueueProgress();
        StatusMessage = $"已加入队列：{item.Name}（演示，未下载）";

        _interaction.Notify(
            $"已把「{item.Name}」加入下载队列（演示，未下载）",
            NotificationSeverity.Success,
            "已加入队列");
    }

    private bool CanStartQueue() => !IsDownloading && Queue.Count > 0;

    /// <summary>依次执行队列中的任务（演示）。</summary>
    [RelayCommand(CanExecute = nameof(CanStartQueue))]
    private async Task StartQueueAsync()
    {
        IsDownloading = true;
        try
        {
            foreach (var task in Queue.Where(task => !task.IsCompleted).ToList())
            {
                task.Status = "下载中";

                while (task.Progress < 100)
                {
                    await Task.Delay(90);
                    task.Progress = Math.Min(100, task.Progress + 10);
                    UpdateQueueProgress();
                }

                task.Status = "已完成";
            }

            StatusMessage = "全部任务已完成（演示，未写入磁盘）";

            _interaction.Notify(
                "全部下载任务已完成（演示，未写入磁盘）",
                NotificationSeverity.Success,
                "下载完成");
        }
        finally
        {
            IsDownloading = false;
            UpdateQueueProgress();
        }
    }

    /// <summary>
    /// 清空下载队列。队列中可能还有未完成的任务，因此先弹出二次确认。
    /// </summary>
    [RelayCommand]
    private async Task ClearQueueAsync()
    {
        if (IsDownloading)
        {
            StatusMessage = "下载进行中，无法清空队列";

            _interaction.Notify(
                "下载正在进行，请等待完成后再清空队列。",
                NotificationSeverity.Warning,
                "无法清空");

            return;
        }

        if (Queue.Count == 0)
        {
            StatusMessage = "下载队列本来就是空的";
            return;
        }

        var pending = Queue.Count(task => !task.IsCompleted);

        var confirmed = await _interaction.ConfirmAsync(
            "清空下载队列",
            pending > 0
                ? $"队列中还有 {pending} 个未完成的任务，清空后将全部丢弃。"
                : "将移出队列中的全部任务。",
            "清空",
            "取消");

        if (!confirmed)
        {
            StatusMessage = "已取消清空队列";
            return;
        }

        Queue.Clear();
        UpdateQueueProgress();
        StatusMessage = "已清空下载队列";

        _interaction.Notify(
            "下载队列已清空（演示，未影响磁盘文件）",
            NotificationSeverity.Success,
            "已清空");
    }

    private void UpdateQueueProgress()
    {
        QueueProgress = Queue.Count == 0
            ? 0
            : Queue.Average(task => task.Progress);
    }
}
