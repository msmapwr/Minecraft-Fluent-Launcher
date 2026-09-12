using Microsoft.UI.Xaml;
using WINUI.Models;

namespace WINUI.Services;

/// <summary>
/// 主题服务：负责把主题偏好应用到界面根元素。
/// <para>
/// WinUI 3 中 <c>Application.RequestedTheme</c> 启动后不可变，运行时切换主题需要设置
/// 根元素的 <see cref="FrameworkElement.RequestedTheme"/>。
/// </para>
/// </summary>
public interface IThemeService
{
    /// <summary>当前主题偏好。</summary>
    AppTheme Current { get; }

    /// <summary>绑定界面根元素（通常是窗口内容），并立即应用当前主题。</summary>
    /// <param name="root">主题作用的根元素。</param>
    void Attach(FrameworkElement root);

    /// <summary>设置并立即应用主题。</summary>
    /// <param name="theme">目标主题。</param>
    void SetTheme(AppTheme theme);
}
