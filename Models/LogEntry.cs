using System;

namespace WINUI.Models;

/// <summary>
/// 一条日志记录。
/// <para>当前为 UI 阶段的 Mock 数据；后续将由真实日志服务产生。</para>
/// </summary>
public sealed class LogEntry
{
    /// <summary>时间戳。</summary>
    public required DateTimeOffset Timestamp { get; init; }

    /// <summary>级别。</summary>
    public required AppLogLevel Level { get; init; }

    /// <summary>来源模块，如 <c>Launcher</c>、<c>Download</c>。</summary>
    public required string Source { get; init; }

    /// <summary>消息正文。</summary>
    public required string Message { get; init; }

    /// <summary>时间标签（含毫秒）。</summary>
    public string TimeLabel => Timestamp.ToString("HH:mm:ss.fff");

    /// <summary>级别短标签（用于徽章）。</summary>
    public string LevelShortLabel => Level.ToShortLabel();

    /// <summary>是否为错误级别。</summary>
    public bool IsError => Level == AppLogLevel.Error;

    /// <summary>是否为警告级别。</summary>
    public bool IsWarning => Level == AppLogLevel.Warning;

    /// <summary>是否为常规及以下级别。</summary>
    public bool IsInformational => Level is not (AppLogLevel.Error or AppLogLevel.Warning);
}
