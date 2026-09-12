using System;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Controls;
using WINUI.Models;

namespace WINUI.ViewModels;

/// <summary>
/// 通知宿主中的单条 InfoBar 视图模型。
/// <para>
/// 这里把模型层的 <see cref="NotificationSeverity"/> 映射为 WinUI 的
/// <see cref="InfoBarSeverity"/>，使 <c>Models</c> 保持不依赖 WinUI 类型。
/// </para>
/// </summary>
public sealed partial class NotificationItemViewModel : ObservableObject
{
    public NotificationItemViewModel(AppNotification notification)
    {
        Severity = notification.Severity switch
        {
            NotificationSeverity.Success => InfoBarSeverity.Success,
            NotificationSeverity.Warning => InfoBarSeverity.Warning,
            NotificationSeverity.Error => InfoBarSeverity.Error,
            _ => InfoBarSeverity.Informational,
        };

        Title = notification.Title;
        Message = notification.Message;
        IsOpen = true;
    }

    /// <summary>
    /// InfoBar 是否展开。用户点击关闭按钮或自动关闭计时到期时置为 <c>false</c>，
    /// 宿主据此把本项移出列表（TwoWay 绑定回写）。
    /// </summary>
    [ObservableProperty]
    public partial bool IsOpen { get; set; }

    /// <summary>InfoBar 级别。</summary>
    public InfoBarSeverity Severity { get; }

    /// <summary>标题，可为空字符串（此时只显示正文）。</summary>
    public string Title { get; }

    /// <summary>正文。</summary>
    public string Message { get; }

    /// <summary>
    /// 自动关闭延时。错误与警告停留更久，方便用户看清原因。
    /// </summary>
    public TimeSpan AutoCloseDelay => Severity switch
    {
        InfoBarSeverity.Error => TimeSpan.FromSeconds(10),
        InfoBarSeverity.Warning => TimeSpan.FromSeconds(8),
        _ => TimeSpan.FromSeconds(5),
    };
}
