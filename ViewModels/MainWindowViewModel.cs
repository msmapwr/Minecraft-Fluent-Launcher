using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using WINUI.Models;
using WINUI.Services;

namespace WINUI.ViewModels;

/// <summary>
/// 应用外壳的视图模型。
/// 承载窗口标题与主题切换；侧边导航结构由 MainWindow.xaml 声明。
/// </summary>
public sealed partial class MainWindowViewModel : ObservableObject
{
    private readonly IThemeService _themeService;

    /// <summary>应用标题。</summary>
    [ObservableProperty]
    public partial string AppTitle { get; set; }

    /// <summary>当前选中的主题选项。</summary>
    [ObservableProperty]
    public partial ThemeOption SelectedThemeOption { get; set; }

    /// <summary>正在根据主题服务广播同步选中项，避免回环写回。</summary>
    private bool _isSyncingTheme;

    /// <summary>可选主题列表（与设置页共用主题服务的单一来源）。</summary>
    public IReadOnlyList<ThemeOption> ThemeOptions { get; }

    public MainWindowViewModel(IThemeService themeService)
    {
        _themeService = themeService;

        AppTitle = AppInfo.DisplayName;

        // 选项列表来自主题服务，保证与设置页完全一致。
        ThemeOptions = themeService.Options;

        // 与已保存的主题偏好保持一致。
        SelectedThemeOption = ThemeOptions.First(option => option.Value == themeService.Current);

        // 设置页切换主题时，同步侧边栏下拉的选中项。
        themeService.ThemeChanged += OnThemeServiceChanged;
    }

    /// <summary>选中项变化时立即应用主题。</summary>
    partial void OnSelectedThemeOptionChanged(ThemeOption value)
    {
        if (_isSyncingTheme)
        {
            return;
        }

        _themeService.SetTheme(value.Value);
    }

    private void OnThemeServiceChanged(object? sender, AppTheme theme)
    {
        var option = ThemeOptions.FirstOrDefault(item => item.Value == theme);
        if (option is null || ReferenceEquals(option, SelectedThemeOption))
        {
            return;
        }

        _isSyncingTheme = true;
        try
        {
            SelectedThemeOption = option;
        }
        finally
        {
            _isSyncingTheme = false;
        }
    }
}
