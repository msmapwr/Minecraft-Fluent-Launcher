using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WINUI.Models;

namespace WINUI.Services;

/// <inheritdoc cref="IInteractionService" />
/// <remarks>
/// 对话框通过 <see cref="SemaphoreSlim"/> 串行化：WinUI 同一时刻只允许一个 ContentDialog，
/// 若并发弹出会直接抛异常。这里改为排队，调用方无需关心。
/// </remarks>
public sealed class InteractionService : IInteractionService
{
    /// <summary>串行化对话框，保证同一时刻只有一个 ContentDialog。</summary>
    private readonly SemaphoreSlim _dialogGate = new(1, 1);

    private XamlRoot? _root;

    /// <inheritdoc />
    public event EventHandler<AppNotification>? NotificationRequested;

    /// <inheritdoc />
    public void AttachRoot(XamlRoot root) => _root = root;

    /// <inheritdoc />
    public void Notify(
        string message,
        NotificationSeverity severity = NotificationSeverity.Informational,
        string? title = null)
        => NotificationRequested?.Invoke(
            this,
            new AppNotification(severity, title ?? string.Empty, message));

    /// <inheritdoc />
    public async Task<bool> ConfirmAsync(
        string title,
        string message,
        string primaryText = "确定",
        string closeText = "取消",
        bool destructive = true)
    {
        if (!TryCreateDialog(title, message, out var dialog))
        {
            // 界面尚未就绪时宁可放弃操作，也不要「未确认就执行破坏性动作」。
            return false;
        }

        dialog.PrimaryButtonText = primaryText;
        dialog.CloseButtonText = closeText;

        // 破坏性操作默认焦点落在「取消」，避免直接回车误删。
        dialog.DefaultButton = destructive ? ContentDialogButton.Close : ContentDialogButton.Primary;

        if (destructive)
        {
            dialog.PrimaryButtonStyle = TryGetStyle("AppDangerButtonStyle");
        }

        return await ShowAsync(dialog) == ContentDialogResult.Primary;
    }

    /// <inheritdoc />
    public async Task AlertAsync(string title, string message, string closeText = "知道了")
    {
        if (!TryCreateDialog(title, message, out var dialog))
        {
            return;
        }

        dialog.CloseButtonText = closeText;
        dialog.DefaultButton = ContentDialogButton.Close;

        await ShowAsync(dialog);
    }

    /// <summary>构造一个已绑定 XamlRoot 的空壳对话框。</summary>
    private bool TryCreateDialog(string title, string message, out ContentDialog dialog)
    {
        if (_root is null)
        {
            dialog = null!;
            Notify("界面尚未就绪，无法显示对话框。", NotificationSeverity.Warning);
            return false;
        }

        dialog = new ContentDialog
        {
            XamlRoot = _root,
            Title = title,
            Content = message,
        };

        return true;
    }

    /// <summary>串行显示对话框并返回结果。</summary>
    private async Task<ContentDialogResult> ShowAsync(ContentDialog dialog)
    {
        await _dialogGate.WaitAsync();
        try
        {
            return await dialog.ShowAsync();
        }
        catch (Exception)
        {
            // 对话框在显示期间被关闭（例如窗口关闭）时不向外抛出，交由调用方按「取消」处理。
            return ContentDialogResult.None;
        }
        finally
        {
            _dialogGate.Release();
        }
    }

    /// <summary>
    /// 按需从应用资源中取样式；缺失时返回 <c>null</c> 以便回退到默认外观。
    /// <para>
    /// 样式定义在 <c>Themes/Controls.xaml</c> 这类<b>合并字典</b>里，而
    /// <see cref="ResourceDictionary.TryGetValue"/> 不会自动下钻 MergedDictionaries，
    /// 因此这里手动递归查找。
    /// </para>
    /// </summary>
    private static Style? TryGetStyle(string key)
    {
        var root = Application.Current?.Resources;

        return root is null ? null : FindStyle(root, key);
    }

    private static Style? FindStyle(ResourceDictionary dictionary, string key)
    {
        if (dictionary.TryGetValue(key, out var value) && value is Style style)
        {
            return style;
        }

        foreach (var merged in dictionary.MergedDictionaries)
        {
            if (FindStyle(merged, key) is { } found)
            {
                return found;
            }
        }

        return null;
    }
}
