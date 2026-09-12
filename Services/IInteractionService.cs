using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using WINUI.Models;

namespace WINUI.Services;

/// <summary>
/// 界面交互服务：统一提供「模态确认 / 提示」与「轻量通知」两种反馈方式。
/// <para>
/// 约定（大更新 ⑤ 已确认）：
/// </para>
/// <list type="bullet">
/// <item><description>破坏性、不可撤销的操作 → <see cref="ConfirmAsync"/>（ContentDialog 二次确认）；</description></item>
/// <item><description>需要用户明确知晓但不阻塞的结果 → <see cref="AlertAsync"/>（ContentDialog 单按钮）；</description></item>
/// <item><description>操作完成等轻量反馈 → <see cref="Notify"/>（InfoBar，数秒后自动消失，不打断操作）。</description></item>
/// </list>
/// <para>
/// 视图模型只依赖本接口，不直接构造对话框，因此可测试。实际渲染由
/// <c>Controls.NotificationHost</c> 与 <c>ContentDialog</c> 完成。
/// </para>
/// </summary>
public interface IInteractionService
{
    /// <summary>
    /// 有新的轻量通知请求时触发。通知宿主订阅此事件并渲染为 InfoBar。
    /// </summary>
    event EventHandler<AppNotification>? NotificationRequested;

    /// <summary>
    /// 绑定界面根元素。ContentDialog 必须设置 <see cref="XamlRoot"/> 才能显示，
    /// 因此窗口内容就绪后需要调用一次。
    /// </summary>
    /// <param name="root">窗口内容的 XamlRoot。</param>
    void AttachRoot(XamlRoot root);

    /// <summary>发出一条轻量通知（不阻塞调用方）。</summary>
    /// <param name="message">正文。</param>
    /// <param name="severity">严重级别，默认「一般信息」。</param>
    /// <param name="title">可选的标题。</param>
    void Notify(
        string message,
        NotificationSeverity severity = NotificationSeverity.Informational,
        string? title = null);

    /// <summary>
    /// 弹出二次确认对话框。
    /// </summary>
    /// <param name="title">标题。</param>
    /// <param name="message">正文，说明后果。</param>
    /// <param name="primaryText">确认按钮文案。</param>
    /// <param name="closeText">取消按钮文案。</param>
    /// <param name="destructive">是否为不可撤销操作（确认按钮标红，且默认焦点落在取消上）。</param>
    /// <returns>用户点击确认为 <c>true</c>；取消、按 Esc 或界面未就绪为 <c>false</c>。</returns>
    Task<bool> ConfirmAsync(
        string title,
        string message,
        string primaryText = "确定",
        string closeText = "取消",
        bool destructive = true);

    /// <summary>弹出只有一个「知道了」按钮的提示对话框。</summary>
    /// <param name="title">标题。</param>
    /// <param name="message">正文。</param>
    /// <param name="closeText">关闭按钮文案。</param>
    Task AlertAsync(string title, string message, string closeText = "知道了");
}
