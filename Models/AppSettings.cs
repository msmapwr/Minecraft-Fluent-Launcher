namespace WINUI.Models;

/// <summary>应用设置，持久化到本地 JSON 文件。</summary>
public sealed class AppSettings
{
    /// <summary>主题偏好。</summary>
    public AppTheme Theme { get; set; } = AppTheme.System;
}
