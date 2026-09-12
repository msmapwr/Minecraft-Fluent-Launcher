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

    /// <summary>应用标题。占位名，待产品定名后统一替换。</summary>
    [ObservableProperty]
    public partial string AppTitle { get; set; }

    /// <summary>当前选中的主题选项。</summary>
    [ObservableProperty]
    public partial ThemeOption SelectedThemeOption { get; set; }

    /// <summary>可选主题列表。</summary>
    public IReadOnlyList<ThemeOption> ThemeOptions { get; } =
    [
        new(AppTheme.System, "跟随系统"),
        new(AppTheme.Light, "浅色"),
        new(AppTheme.Dark, "深色"),
    ];

    public MainWindowViewModel(IThemeService themeService)
    {
        _themeService = themeService;

        AppTitle = "Minecraft 启动器";

        // 与已保存的主题偏好保持一致。
        SelectedThemeOption = ThemeOptions.First(option => option.Value == themeService.Current);
    }

    /// <summary>选中项变化时立即应用主题。</summary>
    partial void OnSelectedThemeOptionChanged(ThemeOption value) => _themeService.SetTheme(value.Value);
}
