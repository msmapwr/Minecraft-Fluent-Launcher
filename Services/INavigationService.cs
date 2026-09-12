using Microsoft.UI.Xaml.Controls;

namespace WINUI.Services;

/// <summary>页面导航服务：按路由键在内容 Frame 中导航。</summary>
public interface INavigationService
{
    /// <summary>当前页面路由键；尚未导航时为 <c>null</c>。</summary>
    string? CurrentKey { get; }

    /// <summary>绑定承载页面的 Frame。</summary>
    /// <param name="frame">内容 Frame。</param>
    void Initialize(Frame frame);

    /// <summary>按路由键导航。键不存在时返回 <c>false</c>，且不会抛异常。</summary>
    /// <param name="key">路由键。</param>
    /// <returns>是否导航成功。</returns>
    bool Navigate(string key);
}
