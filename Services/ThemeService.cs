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
