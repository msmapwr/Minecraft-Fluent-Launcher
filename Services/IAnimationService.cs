using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace WINUI.Services;

/// <summary>
/// 界面动画开关。
/// <para>
/// 设置页的「界面动画」此前只是一个持久化字段：改它不会立刻影响界面。
/// 现在由本服务统一持有开关状态，并在变化时广播，
/// 使页面过渡与列表渐入可以立即响应。
/// </para>
/// </summary>
public interface IAnimationService
{
    /// <summary>当前是否启用界面动画。</summary>
    bool IsEnabled { get; }

    /// <summary>开关发生变化时触发（在 UI 线程）。</summary>
    event EventHandler? Changed;

    /// <summary>
    /// 把「页面过渡」应用到导航用的 <see cref="Frame"/>（动画关闭时移除）。
    /// 需要在窗口内容就绪后调用一次，并在 <see cref="Changed"/> 时重新调用。
    /// </summary>
    /// <param name="frame">承载页面的导航帧。</param>
    void ApplyFrameTransition(Frame frame);

    /// <summary>
    /// 把「内容渐入」应用到刚载入的元素（通常是页面根元素）。
    /// 动画关闭时不产生任何效果。
    /// </summary>
    /// <param name="element">目标元素。</param>
    void ApplyEntrance(UIElement element);

    /// <summary>设置开关：立即持久化并广播（值未变化时不做任何事）。</summary>
    /// <param name="enabled">是否启用界面动画。</param>
    void SetEnabled(bool enabled);
}
