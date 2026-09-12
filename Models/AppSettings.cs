namespace WINUI.Models;

/// <summary>
/// 应用设置，持久化到本地 JSON 文件。
/// <para>
/// 新增字段时请保留已有默认值，以保证旧设置文件仍可反序列化（缺失字段回退到默认值）。
/// </para>
/// </summary>
public sealed class AppSettings
{
    /// <summary>设置结构版本，便于后续迁移。</summary>
    public int Version { get; set; } = 1;

    // ==================== 外观 ====================

    /// <summary>主题偏好。</summary>
    public AppTheme Theme { get; set; } = AppTheme.System;

    /// <summary>界面语言（BCP-47）。</summary>
    public string Language { get; set; } = "zh-CN";

    /// <summary>是否启用界面动画。</summary>
    public bool EnableAnimations { get; set; } = true;

    /// <summary>是否在版本列表中显示快照 / 预览版本。</summary>
    public bool ShowSnapshots { get; set; }

    // ==================== 启动器行为 ====================

    /// <summary>是否在启动时检查更新。</summary>
    public bool AutoCheckUpdates { get; set; } = true;

    /// <summary>启动游戏后是否自动关闭启动器。</summary>
    public bool CloseLauncherAfterLaunch { get; set; }

    // ==================== Java 运行时 ====================

    /// <summary>是否自动检测 Java 运行时。</summary>
    public bool AutoDetectJava { get; set; } = true;

    /// <summary>手动指定的 Java 可执行文件路径（<see cref="AutoDetectJava"/> 为 <c>false</c> 时生效）。</summary>
    public string JavaPath { get; set; } = string.Empty;

    /// <summary>全局默认最小内存（MB）。</summary>
    public int MinMemoryMb { get; set; } = 1024;

    /// <summary>全局默认最大内存（MB）。</summary>
    public int MaxMemoryMb { get; set; } = 4096;

    // ==================== 下载 ====================

    /// <summary>偏好的下载源。</summary>
    public DownloadSource DownloadSource { get; set; } = DownloadSource.Bmclapi;

    /// <summary>最大并发下载数。</summary>
    public int MaxConcurrentDownloads { get; set; } = 8;

    // ==================== 诊断 ====================

    /// <summary>日志级别。</summary>
    public AppLogLevel LogLevel { get; set; } = AppLogLevel.Info;
}
