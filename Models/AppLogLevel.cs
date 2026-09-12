namespace WINUI.Models;

/// <summary>日志级别（数值越大越详细）。</summary>
public enum AppLogLevel
{
    /// <summary>仅错误。</summary>
    Error,

    /// <summary>警告及以上。</summary>
    Warning,

    /// <summary>常规信息及以上（默认）。</summary>
    Info,

    /// <summary>调试信息及以上。</summary>
    Debug,

    /// <summary>最详细（含逐帧 / 网络细节）。</summary>
    Trace,
}
