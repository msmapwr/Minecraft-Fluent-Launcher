namespace WINUI.Models;

/// <summary>日志级别的展示辅助。</summary>
public static class AppLogLevelExtensions
{
    /// <summary>取日志级别的中文标签。</summary>
    public static string ToLabel(this AppLogLevel level) => level switch
    {
        AppLogLevel.Error => "仅错误",
        AppLogLevel.Warning => "警告",
        AppLogLevel.Info => "常规",
        AppLogLevel.Debug => "调试",
        _ => "最详细",
    };
}
