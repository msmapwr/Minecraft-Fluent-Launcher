using System;
using System.Collections.Generic;
using Microsoft.UI.Xaml;
using WINUI.Models;

namespace WINUI.Services;

/// <inheritdoc cref="IThemeService" />
public sealed class ThemeService : IThemeService
{
    private readonly ISettingsService _settings;
    private FrameworkElement? _root;

    public ThemeService(ISettingsService settings)
    {
        _settings = settings;
        Current = settings.Settings.Theme;
    }

    /// <inheritdoc />
    public AppTheme Current { get; private set; }

    /// <inheritdoc />
    public IReadOnlyList<ThemeOption> Options { get; } =
    [
        new(AppTheme.System, "跟随系统"),
        new(AppTheme.Light, "浅色"),
        new(AppTheme.Dark, "深色"),
    ];

    /// <inheritdoc />
    public event EventHandler<AppTheme>? ThemeChanged;

    /// <inheritdoc />
    public void Attach(FrameworkElement root)
    {
        _root = root;
        Apply(Current);
    }

    /// <inheritdoc />
    public void SetTheme(AppTheme theme)
    {
        if (theme != Current)
        {
            Current = theme;
            _settings.Settings.Theme = theme;
            _settings.Save();

            // 广播给所有持有主题选择器的视图模型（外壳侧边栏、设置页）。
            ThemeChanged?.Invoke(this, theme);
        }

        Apply(theme);
    }

    private void Apply(AppTheme theme)
    {
        // 根元素尚未绑定（例如窗口还未创建）时，仅记录偏好，待 Attach 时生效。
        if (_root is null)
        {
            return;
        }

        _root.RequestedTheme = theme switch
        {
            AppTheme.Light => ElementTheme.Light,
            AppTheme.Dark => ElementTheme.Dark,
            _ => ElementTheme.Default,
        };
    }
}
