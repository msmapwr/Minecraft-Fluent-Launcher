using System;
using System.Collections.Generic;
using Microsoft.UI.Xaml.Controls;
using WINUI.Views;

namespace WINUI.Services;

/// <summary>
/// 导航服务实现。
/// <para>
/// 路由表把路由键映射到页面类型。<b>一级路由键必须与 MainWindow.xaml 中
/// 各 NavigationViewItem 的 Tag 保持一致</b>，两处需同步修改；
/// <c>version-detail</c> 为二级页面，仅由代码触发（携带参数）。
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
        ["version-detail"] = typeof(VersionDetailPage),
    };

    /// <summary>用于在回退时还原路由键。</summary>
    private readonly List<string?> _history = [];

    private Frame? _frame;

    /// <inheritdoc />
    public string? CurrentKey { get; private set; }

    /// <inheritdoc />
    public bool CanGoBack => _frame?.CanGoBack == true;

    /// <inheritdoc />
    public void Initialize(Frame frame) => _frame = frame;

    /// <inheritdoc />
    public bool Navigate(string key) => Navigate(key, null);

    /// <inheritdoc />
    public bool Navigate(string key, object? parameter)
    {
        if (_frame is null || !Routes.TryGetValue(key, out var pageType))
        {
            return false;
        }

        // 已在目标页面且未携带新参数时不重复导航，避免回退栈堆积。
        if (CurrentKey == key && parameter is null && _frame.CurrentSourcePageType == pageType)
        {
            return true;
        }

        if (!_frame.Navigate(pageType, parameter))
        {
            return false;
        }

        _history.Add(CurrentKey);
        CurrentKey = key;
        return true;
    }

    /// <inheritdoc />
    public bool GoBack()
    {
        if (_frame?.CanGoBack != true)
        {
            return false;
        }

        _frame.GoBack();

        if (_history.Count > 0)
        {
            CurrentKey = _history[^1];
            _history.RemoveAt(_history.Count - 1);
        }

        return true;
    }
}
