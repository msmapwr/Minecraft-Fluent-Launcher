using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WINUI.Models;
using WINUI.Services;

namespace WINUI.ViewModels;

/// <summary>
/// 下载队列页（二级页，右上角「下载中 N」入口）的视图模型。
/// <para>
/// 任务数据由 <see cref="IDownloadQueueService"/> 全局持有：页面离开不中断任务，
/// 重进页面时从服务恢复列表（「记住进度状态」）。事件在后台线程触发，
/// 集合与属性更新经 <see cref="BoundCollectionUpdater"/> 调度到 UI 线程。
/// </para>
/// </summary>
public sealed partial class DownloadQueuePageViewModel : PageViewModelBase
{
    private readonly IDownloadQueueService _queue;
    private readonly IGameLauncherService _gameLauncher;
    private readonly INavigationService _navigation;
    private readonly IInteractionService _interaction;

    /// <summary>队列任务列表（UI 投影）。</summary>
    public ObservableCollection<DownloadTaskViewModel> Tasks { get; } = [];

    /// <summary>正在推进的任务数。</summary>
    [ObservableProperty]
    public partial int ActiveCount { get; set; }

    /// <summary>是否正在下载（标题徽章）。</summary>
    public bool IsDownloading => ActiveCount > 0;

    /// <summary>标题徽章文案。</summary>
    public string BadgeLabel => IsDownloading ? $"下载中 {ActiveCount}" : "没有进行中的下载";

    /// <summary>整体进度（所有任务的均值）。</summary>
    public double OverallProgress => Tasks.Count == 0
        ? 0
        : Tasks.Average(task => task.Progress);

    /// <summary>队列摘要。</summary>
    public string QueueSummary => Tasks.Count switch
    {
        0 => "队列是空的",
        _ => $"{Tasks.Count} 个任务 · {Tasks.Count(task => task.Task.IsCompleted)} 已完成"
            + (IsDownloading ? $" · {ActiveCount} 进行中" : string.Empty),
    };

    /// <inheritdoc />
    public override string EmptyGlyph => "\uE896";

    /// <inheritdoc />
    public override string EmptyTitle => "暂无下载任务";

    /// <inheritdoc />
    public override string EmptyText => "在下载中心选择内容并确认安装后，任务会出现在这里；离开页面不会中断下载。";

    public DownloadQueuePageViewModel(
        IDownloadQueueService queue,
        IGameLauncherService gameLauncher,
        INavigationService navigation,
        IInteractionService interaction)
    {
        _queue = queue;
        _gameLauncher = gameLauncher;
        _navigation = navigation;
        _interaction = interaction;

        _queue.TaskAdded += OnTaskAdded;
        _queue.TaskChanged += OnTaskChanged;
        _queue.TaskRemoved += OnTaskRemoved;

        // 页面可能被反复进出：VM 是单例，构造时同步一次现有任务即可恢复状态。
        RefreshAll();
    }

    // 事件在后台线程触发，统一经 BoundCollectionUpdater 调度回 UI 线程；
    // 回调必须读取「当前」状态（Request 会合并同一帧内的多次请求）。
    private readonly BoundCollectionUpdater _updater = new();

    private void OnTaskAdded(object? sender, DownloadTask task) => _updater.Request(RefreshAll);

    private void OnTaskChanged(object? sender, DownloadTask task) => _updater.Request(RefreshAll);

    private void OnTaskRemoved(object? sender, DownloadTask task) => _updater.Request(RefreshAll);

    /// <summary>全量重建任务列表（合并同一帧的多次事件；量级为个人下载队列，足够高效）。</summary>
    private void RefreshAll()
    {
        var tasks = _queue.Tasks;

        // 同步已有投影 + 追加新任务（保持顺序稳定，避免整表 Clear 打断进行中的进度条）。
        for (var index = Tasks.Count - 1; index >= 0; index--)
        {
            var existing = Tasks[index];
            var current = tasks.FirstOrDefault(candidate => candidate.Id == existing.Task.Id);

            if (current is null)
            {
                Tasks.RemoveAt(index);
            }
            else
            {
                existing.SyncFrom(current);
            }
        }

        foreach (var task in tasks)
        {
            if (Tasks.All(existing => existing.Task.Id != task.Id))
            {
                Tasks.Add(new DownloadTaskViewModel(task));
            }
        }

        // 保持与入队顺序一致（新任务追加在尾部）。
        var order = tasks.Select((task, index) => (task.Id, index)).ToDictionary(pair => pair.Id, pair => pair.index);
        if (Tasks.Select((vm, index) => (vm, index)).Any(pair => order.TryGetValue(pair.vm.Task.Id, out var target) && target != pair.index))
        {
            var ordered = Tasks.OrderBy(vm => order.TryGetValue(vm.Task.Id, out var target) ? target : int.MaxValue).ToList();
            Tasks.Clear();
            foreach (var vm in ordered)
            {
                Tasks.Add(vm);
            }
        }

        ActiveCount = tasks.Count(task => task.Status.IsActive());
        UpdateContentState(Tasks.Count > 0);

        OnPropertyChanged(nameof(OverallProgress));
        OnPropertyChanged(nameof(QueueSummary));
        OnPropertyChanged(nameof(BadgeLabel));
        OnPropertyChanged(nameof(IsDownloading));
        ClearFinishedCommand.NotifyCanExecuteChanged();
    }

    /// <summary>取消任务（已下载文件保留）。code-behind 以 Tag 转发。</summary>
    public void CancelTask(DownloadTaskViewModel task) => _queue.Cancel(task.Task.Id);

    /// <summary>手动重试失败任务。code-behind 以 Tag 转发。</summary>
    public void RetryTask(DownloadTaskViewModel task) => _queue.Retry(task.Task.Id);

    /// <summary>把已终结任务移出列表。code-behind 以 Tag 转发。</summary>
    public void RemoveTask(DownloadTaskViewModel task) => _queue.Remove(task.Task.Id);

    private bool CanClearFinished() => Tasks.Any(task => task.CanRemove);

    /// <summary>清空已终结（完成 / 失败 / 取消）的任务。</summary>
    [RelayCommand(CanExecute = nameof(CanClearFinished))]
    private void ClearFinished() => _queue.ClearFinished();

    /// <summary>完成引导：去启动游戏（进入启动页）。</summary>
    public void GoLaunch() => _navigation.Navigate("launch");

    /// <summary>完成引导：查看实例。</summary>
    public void GoInstances() => _navigation.Navigate("instances");

    /// <summary>返回上一页。</summary>
    [RelayCommand]
    private void GoBack() => _navigation.GoBack();
}
