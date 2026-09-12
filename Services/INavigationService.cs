using Microsoft.UI.Xaml.Controls;

namespace WINUI.Services;

/// <summary>页面导航服务：按路由键在内容 Frame 中导航，并支持携带参数与回退。</summary>
public interface INavigationService
{
    /// <summary>当前页面路由键；尚未导航时为 <c>null</c>。</summary>
    string? CurrentKey { get; }

    /// <summary>是否可以回退到上一页。</summary>
    bool CanGoBack { get; }

    /// <summary>绑定承载页面的 Frame。</summary>
    /// <param name="frame">内容 Frame。</param>
    void Initialize(Frame frame);

    /// <summary>按路由键导航。键不存在时返回 <c>false</c>，且不会抛异常。</summary>
    /// <param name="key">路由键。</param>
    /// <returns>是否导航成功。</returns>
    bool Navigate(string key);

    /// <summary>
    /// 按路由键导航并携带参数。键不存在时返回 <c>false</c>，且不会抛异常。
    /// <para>参数由目标页面的 <c>OnNavigatedTo</c> 取出。</para>
    /// </summary>
    /// <param name="key">路由键。</param>
    /// <param name="parameter">导航参数，可为 <c>null</c>。</param>
    /// <returns>是否导航成功。</returns>
    bool Navigate(string key, object? parameter);

    /// <summary>回退到上一页。无法回退时返回 <c>false</c>。</summary>
    /// <returns>是否回退成功。</returns>
    bool GoBack();
}
