using System;
using System.Collections.Generic;
using Microsoft.UI.Xaml.Controls;
using WINUI.Views;

namespace WINUI.Services;

/// <summary>
/// 导航服务实现。
/// <para>
/// 路由表把路由键映射到页面类型。<b>路由键必须与 MainWindow.xaml 中
/// 各 NavigationViewItem 的 Tag 保持一致</b>，两处需同步修改。
/// </para>
/// </summary>
public sealed class NavigationService : INavigationService
{
    /// <summary>路由表：路由键 → 页面类型。</summary>
    private static readonly Dictionary<string, Type> Routes = new(StringComparer.Ordinal)
    {
        ["launch"] = typeof(LaunchPage),
        ["instances"] = typeof(InstancesPage),
        ["downloads"] = typeof(DownloadsPage),
        ["mods"] = typeof(ModsPage),
        ["accounts"] = typeof(AccountsPage),
        ["settings"] = typeof(SettingsPage),
        ["logs"] = typeof(LogsPage),
        ["about"] = typeof(AboutPage),
    };

    private Frame? _frame;

    /// <inheritdoc />
    public string? CurrentKey { get; private set; }

    /// <inheritdoc />
    public void Initialize(Frame frame) => _frame = frame;

    /// <inheritdoc />
    public bool Navigate(string key)
    {
        if (_frame is null || !Routes.TryGetValue(key, out var pageType))
        {
            return false;
        }

        // 已在目标页面时不重复导航，避免回退栈堆积。
        if (CurrentKey == key && _frame.CurrentSourcePageType == pageType)
        {
            return true;
        }

        if (!_frame.Navigate(pageType))
        {
            return false;
        }

        CurrentKey = key;
        return true;
    }
}
