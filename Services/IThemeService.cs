using System;
using System.Collections.Generic;
using Microsoft.UI.Xaml;
using WINUI.Models;

namespace WINUI.Services;

/// <summary>
/// 主题服务：负责把主题偏好应用到界面根元素。
/// <para>
/// WinUI 3 中 <c>Application.RequestedTheme</c> 启动后不可变，运行时切换主题需要设置
/// 根元素的 <see cref="FrameworkElement.RequestedTheme"/>。
/// </para>
/// <para>
/// 主题的<b>单一状态源</b>：外壳侧边栏与设置页的主题下拉共用 <see cref="Options"/>，
/// 并通过 <see cref="ThemeChanged"/> 广播保持两处选中项同步。
/// </para>
/// </summary>
public interface IThemeService
{
    /// <summary>当前主题偏好。</summary>
    AppTheme Current { get; }

    /// <summary>共享主题选项列表（外壳与设置页共用的单一来源）。</summary>
    IReadOnlyList<ThemeOption> Options { get; }

    /// <summary>主题变更广播：参数为切换后的主题偏好。仅在实际发生变化时触发。</summary>
    event EventHandler<AppTheme>? ThemeChanged;

    /// <summary>绑定界面根元素（通常是窗口内容），并立即应用当前主题。</summary>
    /// <param name="root">主题作用的根元素。</param>
    void Attach(FrameworkElement root);

    /// <summary>设置并立即应用主题。</summary>
    /// <param name="theme">目标主题。</param>
    void SetTheme(AppTheme theme);
}
