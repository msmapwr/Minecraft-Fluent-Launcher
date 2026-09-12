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
/// </summary>
public sealed partial class DownloadsPageViewModel : ObservableObject
{
    private readonly ILauncherDataService _dataService;

    /// <summary>全量条目（搜索/筛选的数据源）。</summary>
    private readonly List<DownloadItem> _allItems = [];

    /// <summary>搜索关键字。</summary>
    [ObservableProperty]
    public partial string SearchText { get; set; }

    /// <summary>当前分类（<c>null</c> 表示全部）。</summary>
    [ObservableProperty]
    public partial SelectOption<DownloadCategory?> SelectedCategory { get; set; }

    /// <summary>当前下载源。</summary>
    [ObservableProperty]
    public partial SelectOption<DownloadSource> SelectedSource { get; set; }

    /// <summary>是否正在下载。</summary>
    [ObservableProperty]
    public partial bool IsDownloading { get; set; }

    /// <summary>整体队列进度（0–100）。</summary>
    [ObservableProperty]
    public partial double QueueProgress { get; set; }

    /// <summary>状态栏文案。</summary>
    [ObservableProperty]
    public partial string StatusMessage { get; set; }

    /// <summary>筛选后的条目列表。</summary>
    public ObservableCollection<DownloadItem> Items { get; } = [];

    /// <summary>下载队列。</summary>
    public ObservableCollection<DownloadTaskViewModel> Queue { get; } = [];

    /// <summary>可选分类。</summary>
    public IReadOnlyList<SelectOption<DownloadCategory?>> Categories { get; } =
    [
        new(null, "全部"),
        new(DownloadCategory.GameVersion, "游戏版本"),
        new(DownloadCategory.Loader, "模组加载器"),
        new(DownloadCategory.Mod, "模组"),
        new(DownloadCategory.ResourcePack, "资源包"),
        new(DownloadCategory.Shader, "光影"),
        new(DownloadCategory.Modpack, "整合包"),
        new(DownloadCategory.World, "地图存档"),
    ];

    /// <summary>可选下载源。</summary>
    public IReadOnlyList<SelectOption<DownloadSource>> Sources { get; } =
    [
        new(DownloadSource.Official, "官方源"),
        new(DownloadSource.Bmclapi, "BMCLAPI 镜像"),
        new(DownloadSource.Community, "社区镜像"),
    ];

    /// <summary>列表是否为空。</summary>
    public bool IsEmpty => Items.Count == 0;

    /// <summary>队列是否为空。</summary>
    public bool IsQueueEmpty => Queue.Count == 0;

    /// <summary>队列摘要。</summary>
    public string QueueSummary => Queue.Count == 0
        ? "队列为空"
        : $"{Queue.Count} 个任务 · 整体 {QueueProgress:0}%";

    public DownloadsPageViewModel(ILauncherDataService dataService)
    {
        _dataService = dataService;

        SearchText = string.Empty;
        StatusMessage = "准备就绪";

        // 先赋下载源，再赋分类——赋分类会触发 ApplyQuery，其中会读取下载源。
        SelectedSource = Sources[0];
        SelectedCategory = Categories[0];

        Queue.CollectionChanged += OnQueueChanged;

        // Mock 实现返回已完成的 Task，此处会同步跑完。
        _ = InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        _allItems.AddRange(await _dataService.GetDownloadItemsAsync());
        ApplyQuery();
    }

    private void OnQueueChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        OnPropertyChanged(nameof(IsQueueEmpty));
        OnPropertyChanged(nameof(QueueSummary));
        StartQueueCommand.NotifyCanExecuteChanged();
    }

    partial void OnSearchTextChanged(string value) => ApplyQuery();

    partial void OnSelectedCategoryChanged(SelectOption<DownloadCategory?> value) => ApplyQuery();

    partial void OnQueueProgressChanged(double value) => OnPropertyChanged(nameof(QueueSummary));

    partial void OnIsDownloadingChanged(bool value) => StartQueueCommand.NotifyCanExecuteChanged();

    /// <summary>按当前搜索 / 分类条件刷新列表。</summary>
    private void ApplyQuery()
    {
        if (SelectedCategory is null || SelectedSource is null)
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

        Items.Clear();
        foreach (var item in query)
        {
            Items.Add(item);
        }

        OnPropertyChanged(nameof(IsEmpty));
        StatusMessage = $"显示 {Items.Count} / {_allItems.Count} 个条目 · 下载源：{SelectedSource.Value.ToLabel()}";
    }

    /// <summary>刷新列表（演示）。</summary>
    [RelayCommand]
    private void Refresh() => StatusMessage = $"已刷新列表（演示）· 下载源：{SelectedSource.Value.ToLabel()}";

    /// <summary>把条目加入下载队列。</summary>
    public void Enqueue(DownloadItem item)
    {
        if (Queue.Any(task => task.Item.Id == item.Id && !task.IsCompleted))
        {
            StatusMessage = $"「{item.Name}」已在下载队列中";
            return;
        }

        Queue.Add(new DownloadTaskViewModel(item));
        UpdateQueueProgress();
        StatusMessage = $"已加入队列：{item.Name}（演示，未下载）";
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
        }
        finally
        {
            IsDownloading = false;
            UpdateQueueProgress();
        }
    }

    /// <summary>清空下载队列。</summary>
    [RelayCommand]
    private void ClearQueue()
    {
        if (IsDownloading)
        {
            StatusMessage = "下载进行中，无法清空队列";
            return;
        }

        Queue.Clear();
        UpdateQueueProgress();
        StatusMessage = "已清空下载队列";
    }

    private void UpdateQueueProgress()
    {
        QueueProgress = Queue.Count == 0
            ? 0
            : Queue.Average(task => task.Progress);
    }
}
