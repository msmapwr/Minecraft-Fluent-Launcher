namespace WINUI.Models;

/// <summary>
/// 界面通知的严重级别。在 <c>Controls.NotificationHost</c> 中映射为 InfoBar 的
/// <c>InfoBarSeverity</c>；这样模型层不依赖 WinUI 类型。
/// </summary>
public enum NotificationSeverity
{
    /// <summary>一般信息。</summary>
    Informational,

    /// <summary>操作成功。</summary>
    Success,

    /// <summary>警告。</summary>
    Warning,

    /// <summary>错误。</summary>
    Error,
}
