using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WINUI.Services;
using WINUI.ViewModels;

namespace WINUI.Views;

/// <summary>
/// 应用主窗口，同时充当应用外壳（NavigationView + 内容 Frame）。
/// </summary>
public sealed partial class MainWindow : Window
{
    /// <summary>供 XAML 的 x:Bind 绑定的视图模型。</summary>
    public MainWindowViewModel ViewModel { get; }

    private readonly INavigationService _navigation;

    public MainWindow(
        MainWindowViewModel viewModel,
        IThemeService themeService,
        INavigationService navigationService)
    {
        // x:Bind 在 InitializeComponent 期间求值，必须先赋值 ViewModel。
        ViewModel = viewModel;
        _navigation = navigationService;
        InitializeComponent();
        Title = viewModel.AppTitle;

        // 窗口内容就绪后再绑定根元素，主题切换即时生效。
        themeService.Attach((FrameworkElement)Content);

        _navigation.Initialize(ContentFrame);

        // 默认打开启动页，并同步侧边栏选中项。
        _navigation.Navigate("launch");
        ShellNavigation.SelectedItem = ShellNavigation.MenuItems[0];
    }

    /// <summary>侧边栏点击：按 Tag 中的路由键导航。</summary>
    private void OnNavigationItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
    {
        if (args.InvokedItemContainer?.Tag is string key)
        {
            _navigation.Navigate(key);
        }
    }
}
