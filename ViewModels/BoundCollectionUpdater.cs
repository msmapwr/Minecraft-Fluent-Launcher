using System;
using Microsoft.UI.Dispatching;

namespace WINUI.ViewModels;

/// <summary>
/// 安全更新「与 XAML 集合控件绑定的集合」的小工具。
/// <para>
/// WinUI 有一条硬性约束：<b>绑定中的集合不得在布局（Measure / Arrange）期间被修改</b>，
/// 否则底层会抛出 <c>COMException (0x80004005)</c>，消息为
/// 「Child collection must not be modified during measure or arrange」。
/// </para>
/// <para>
/// 而 <c>ComboBox.SelectedItem</c> 这类 <c>Mode=TwoWay</c> 绑定的回调，以及
/// <c>ToggleSwitch.IsOn</c> 引发的属性变更回调，都可能恰好在布局过程中被触发 ——
/// 若此时同步去 <c>Clear()</c> / <c>Add()</c> 集合就会崩溃。
/// </para>
/// <para>
/// 因此，凡是<b>由绑定回调间接触发</b>的集合重建，都应经 <see cref="Request"/> 推迟到
/// 下一次 Dispatcher 回调执行（这也是微软对该问题的官方建议）。
/// 构造期间与按钮点击等「确定的非布局上下文」可直接同步调用。
/// </para>
/// </summary>
internal sealed class BoundCollectionUpdater
{
    private readonly DispatcherQueue? _dispatcher = DispatcherQueue.GetForCurrentThread();
    private bool _queued;

    /// <summary>
    /// 请求执行一次集合更新；取不到 Dispatcher（非 UI 线程）时退化为同步执行。
    /// <para>
    /// 同一帧内的多次请求会合并为一次，因此 <paramref name="update"/> 必须读取<b>当前</b>状态，
    /// 而不能依赖请求发出那一刻的快照。
    /// </para>
    /// </summary>
    public void Request(Action update)
    {
        if (_dispatcher is null)
        {
            update();
            return;
        }

        if (_queued)
        {
            return;
        }

        _queued = true;

        if (!_dispatcher.TryEnqueue(() =>
            {
                _queued = false;
                update();
            }))
        {
            _queued = false;
            update();
        }
    }
}
