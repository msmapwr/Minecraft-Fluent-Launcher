namespace WINUI.Models;

/// <summary>
/// 一条界面通知。由 <c>IInteractionService.Notify</c> 发出，
/// 由通知宿主渲染成 InfoBar 并在数秒后自动消失。
/// </summary>
/// <param name="Severity">严重级别，决定 InfoBar 的图标与配色。</param>
/// <param name="Title">标题，可为空字符串（此时只显示正文）。</param>
/// <param name="Message">正文。</param>
public sealed record AppNotification(NotificationSeverity Severity, string Title, string Message);
