using CommunityToolkit.Mvvm.ComponentModel;
using WINUI.Models;

namespace WINUI.ViewModels;

/// <summary>
/// 下载队列中一个任务的界面投影。
/// <para>
/// 任务数据在后台线程推进（<see cref="DownloadTask"/>），本类只在 UI 线程通过
/// <see cref="SyncFrom"/> 做快照同步，避免跨线程更新绑定目标。
/// </para>
/// </summary>
public sealed partial class DownloadTaskViewModel : ObservableObject
{
    /// <summary>任务名称。</summary>
    [ObservableProperty]
    public partial string Name { get; set; } = string.Empty;

    /// <summary>分类标签。</summary>
    [ObservableProperty]
    public partial string CategoryLabel { get; set; } = string.Empty;

    /// <summary>加载器标签。</summary>
    [ObservableProperty]
    public partial string LoaderLabel { get; set; } = string.Empty;

    /// <summary>实例名标签。</summary>
    [ObservableProperty]
    public partial string InstanceLabel { get; set; } = string.Empty;

    /// <summary>状态标签。</summary>
    [ObservableProperty]
    public partial string StatusLabel { get; set; } = string.Empty;

    /// <summary>总体进度（0–100）。</summary>
    [ObservableProperty]
    public partial double Progress { get; set; }

    /// <summary>进度是否不可知（显示不确定进度条）。</summary>
    [ObservableProperty]
    public partial bool IsIndeterminate { get; set; }

    /// <summary>速度标签。</summary>
    [ObservableProperty]
    public partial string SpeedLabel { get; set; } = "—";

    /// <summary>剩余体积标签。</summary>
    [ObservableProperty]
    public partial string RemainingLabel { get; set; } = "—";

    /// <summary>已下载 / 总体积标签。</summary>
    [ObservableProperty]
    public partial string SizeLabel { get; set; } = string.Empty;

    /// <summary>错误信息（失败时显示）。</summary>
    [ObservableProperty]
    public partial string? ErrorMessage { get; set; }

    /// <summary>是否有错误信息（控制可见性）。</summary>
    [ObservableProperty]
    public partial bool HasError { get; set; }

    /// <summary>自动重试提示（重试期间显示）。</summary>
    [ObservableProperty]
    public partial string? RetryNote { get; set; }

    /// <summary>是否有重试提示（控制可见性）。</summary>
    [ObservableProperty]
    public partial bool HasRetryNote { get; set; }

    /// <summary>是否可以取消。</summary>
    [ObservableProperty]
    public partial bool CanCancel { get; set; }

    /// <summary>是否可以重试（仅失败任务）。</summary>
    [ObservableProperty]
    public partial bool CanRetry { get; set; }

    /// <summary>是否可以移出列表（仅已终结任务）。</summary>
    [ObservableProperty]
    public partial bool CanRemove { get; set; }

    /// <summary>是否显示完成引导（启动游戏 / 查看实例）。</summary>
    [ObservableProperty]
    public partial bool ShowLaunchActions { get; set; }

    /// <summary>是否为演示任务（无真实下载，界面上明确标注）。</summary>
    public bool IsDemo { get; private set; }

    /// <summary>对应的队列任务。</summary>
    public DownloadTask Task { get; }

    public DownloadTaskViewModel(DownloadTask task)
    {
        Task = task;
        SyncFrom(task);
    }

    /// <summary>在 UI 线程从任务模型同步一份快照。</summary>
    public void SyncFrom(DownloadTask task)
    {
        Name = task.Name;
        CategoryLabel = task.Item.CategoryLabel;
        LoaderLabel = task.Item.Category == DownloadCategory.GameVersion ? task.Loader : "—";
        InstanceLabel = string.IsNullOrWhiteSpace(task.InstanceName) ? "—" : task.InstanceName;
        StatusLabel = task.StatusLabel;
        Progress = task.Progress;
        IsIndeterminate = task.IsIndeterminate && task.Status.IsActive();
        SpeedLabel = task.SpeedLabel;
        RemainingLabel = task.RemainingLabel;
        SizeLabel = $"{task.ReceivedMb:0.0} / {task.TotalMb:0.0} MB";
        ErrorMessage = task.ErrorMessage;
        HasError = !string.IsNullOrEmpty(task.ErrorMessage);
        RetryNote = task.RetryNote;
        HasRetryNote = !string.IsNullOrEmpty(task.RetryNote);
        CanCancel = task.IsActive;
        CanRetry = task.IsFailed;
        CanRemove = task.IsFinished;
        ShowLaunchActions = task.IsCompleted && task.Item.Category == DownloadCategory.GameVersion && !task.IsDemo;
        IsDemo = task.IsDemo;
    }
}
