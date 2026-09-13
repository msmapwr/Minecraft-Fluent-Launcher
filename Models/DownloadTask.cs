using System;

namespace WINUI.Models;

/// <summary>
/// 下载队列中的一个任务。
/// <para>
/// 纯数据模型：状态与进度由 <see cref="Services.DownloadQueueService"/> 在后台线程推进，
/// 界面侧通过 <c>DownloadTaskViewModel</c> 在 UI 线程做快照同步，因此本类不实现
/// 属性变更通知。
/// </para>
/// </summary>
public sealed class DownloadTask
{
    /// <summary>自动重试的上限次数（首次执行后再重试 3 次）。</summary>
    public const int MaxRetryCount = 3;

    /// <summary>任务标识。</summary>
    public string Id { get; } = Guid.NewGuid().ToString("N");

    /// <summary>来源条目。</summary>
    public required DownloadItem Item { get; init; }

    /// <summary>所选加载器（「版本」条目为 Vanilla / Fabric 等；其它条目为「不适用」）。</summary>
    public string Loader { get; init; } = "Vanilla";

    /// <summary>目标游戏版本（模组等资源的兼容性预判结果；版本条目即自身版本）。</summary>
    public string? TargetGameVersion { get; init; }

    /// <summary>将创建的实例名（用于完成引导与展示）。</summary>
    public string InstanceName { get; init; } = string.Empty;

    /// <summary>
    /// 是否为演示任务：模拟进度、不产生网络请求、不写入磁盘。
    /// 非「版本」类条目在资源站接入（v0.3）前均为演示任务。
    /// </summary>
    public bool IsDemo { get; init; }

    /// <summary>当前状态。</summary>
    public DownloadTaskStatus Status { get; set; } = DownloadTaskStatus.Waiting;

    /// <summary>总体进度（0–100）。</summary>
    public double Progress { get; set; }

    /// <summary>进度是否不可知（真实安装无回调时以不确定进度展示）。</summary>
    public bool IsIndeterminate { get; set; }

    /// <summary>已下载体积（MB）。</summary>
    public double ReceivedMb { get; set; }

    /// <summary>总体积（MB）；取自条目数据。</summary>
    public double TotalMb => Item.SizeMb;

    /// <summary>当前速度（MB/s）。</summary>
    public double SpeedMbps { get; set; }

    /// <summary>已执行的尝试次数（0 = 尚未开始）。</summary>
    public int Attempt { get; set; }

    /// <summary>最近一次失败的错误信息；无失败为 <c>null</c>。</summary>
    public string? ErrorMessage { get; set; }

    /// <summary>入队时间。</summary>
    public DateTimeOffset CreatedAt { get; } = DateTimeOffset.Now;

    /// <summary>任务名称。</summary>
    public string Name => Item.Name;

    /// <summary>状态标签。</summary>
    public string StatusLabel => Status.ToLabel();

    /// <summary>是否在推进中。</summary>
    public bool IsActive => Status.IsActive();

    /// <summary>是否已终结。</summary>
    public bool IsFinished => Status.IsFinished();

    /// <summary>是否失败（可手动重试）。</summary>
    public bool IsFailed => Status == DownloadTaskStatus.Failed;

    /// <summary>是否已完成（显示完成引导）。</summary>
    public bool IsCompleted => Status == DownloadTaskStatus.Completed;

    /// <summary>是否已取消。</summary>
    public bool IsCancelled => Status == DownloadTaskStatus.Cancelled;

    /// <summary>剩余体积标签。</summary>
    public string RemainingLabel => Progress >= 100
        ? "—"
        : (TotalMb - ReceivedMb) switch
        {
            < 0 => "—",
            var left => $"{left:0.0} MB",
        };

    /// <summary>速度标签。</summary>
    public string SpeedLabel => SpeedMbps <= 0 ? "—" : $"{SpeedMbps:0.0} MB/s";

    /// <summary>自动重试提示（仅重试期间可见）。</summary>
    public string? RetryNote => IsActive && Attempt > 0
        ? $"第 {Attempt}/{MaxRetryCount} 次自动重试"
        : null;
}
