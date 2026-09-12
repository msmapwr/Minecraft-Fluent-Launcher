using Microsoft.UI.Xaml;
using WINUI.Services;
using WINUI.ViewModels;

namespace WINUI.Views;

/// <summary>
/// 应用主窗口。视图模型与主题服务通过构造函数注入（由 DI 容器解析）。
/// </summary>
public sealed partial class MainWindow : Window
{
    /// <summary>供 XAML 的 x:Bind 绑定的视图模型。</summary>
    public MainWindowViewModel ViewModel { get; }

    public MainWindow(MainWindowViewModel viewModel, IThemeService themeService)
    {
        // x:Bind 在 InitializeComponent 期间求值，必须先赋值 ViewModel。
        ViewModel = viewModel;
        InitializeComponent();
        Title = viewModel.AppTitle;

        // 窗口内容就绪后再绑定根元素，主题切换即时生效。
        themeService.Attach((FrameworkElement)Content);
    }
}
