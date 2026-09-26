using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using WINUI.Models;
using WINUI.Services;

namespace WINUI.Tests.Helpers;

/// <summary>交互服务替身：记录通知，不弹任何界面。</summary>
public sealed class FakeInteractionService : IInteractionService
{
    /// <summary>已发出的通知消息。</summary>
    public System.Collections.Generic.List<string> Notifications { get; } = [];

    // 测试不需要订阅通知事件；用空访问器避免「事件未使用」警告。
    public event EventHandler<AppNotification>? NotificationRequested
    {
        add { }
        remove { }
    }

    public void AttachRoot(XamlRoot root)
    {
    }

    public void Notify(
        string message,
        NotificationSeverity severity = NotificationSeverity.Informational,
        string? title = null)
        => Notifications.Add(message);

    public Task<bool> ConfirmAsync(
        string title,
        string message,
        string primaryText = "确定",
        string closeText = "取消",
        bool destructive = true)
        => Task.FromResult(true);

    public Task AlertAsync(string title, string message, string closeText = "知道了")
        => Task.CompletedTask;
}
