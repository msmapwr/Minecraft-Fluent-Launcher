namespace WINUI.Models;

/// <summary>
/// 下载队列任务的状态机。
/// <para>
/// 状态推进路径：<see cref="Waiting"/> → <see cref="Preparing"/> →
/// <see cref="Downloading"/> → <see cref="Installing"/> → <see cref="Completed"/>；
/// 任意阶段可进入 <see cref="Failed"/>（自动重试会回到 <see cref="Preparing"/>）或
/// <see cref="Cancelled"/>。
/// </para>
/// </summary>
public enum DownloadTaskStatus
{
    /// <summary>等待中：已入队，尚无执行者。</summary>
    Waiting,

    /// <summary>准备中：解析版本清单 / 依赖。</summary>
    Preparing,

    /// <summary>下载中：传输文件。</summary>
    Downloading,

    /// <summary>安装中：整合文件、写入实例。</summary>
    Installing,

    /// <summary>已完成。</summary>
    Completed,

    /// <summary>失败（自动重试耗尽或不可恢复错误）。</summary>
    Failed,

    /// <summary>已取消（用户主动；已下载的文件会被保留）。</summary>
    Cancelled,
}

/// <summary><see cref="DownloadTaskStatus"/> 的标签与语义扩展。</summary>
public static class DownloadTaskStatusExtensions
{
    /// <summary>状态展示标签。</summary>
    public static string ToLabel(this DownloadTaskStatus status) => status switch
    {
        DownloadTaskStatus.Waiting => "等待中",
        DownloadTaskStatus.Preparing => "准备中",
        DownloadTaskStatus.Downloading => "下载中",
        DownloadTaskStatus.Installing => "安装中",
        DownloadTaskStatus.Completed => "已完成",
        DownloadTaskStatus.Failed => "失败",
        DownloadTaskStatus.Cancelled => "已取消",
        _ => status.ToString(),
    };

    /// <summary>任务是否仍在推进中（不可移除、不可重复入队）。</summary>
    public static bool IsActive(this DownloadTaskStatus status) => status is
        DownloadTaskStatus.Waiting or
        DownloadTaskStatus.Preparing or
        DownloadTaskStatus.Downloading or
        DownloadTaskStatus.Installing;

    /// <summary>任务是否已终结（可移除）。</summary>
    public static bool IsFinished(this DownloadTaskStatus status) => !status.IsActive();
}
