using System;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WINUI.Services;
using WINUI.ViewModels;

namespace WINUI.Views;

/// <summary>
/// 应用主窗口，同时充当应用外壳（自定义标题栏 + NavigationView + 内容 Frame）。
/// </summary>
public sealed partial class MainWindow : Window
{
    /// <summary>供 XAML 的 x:Bind 绑定的视图模型。</summary>
    public MainWindowViewModel ViewModel { get; }

    private readonly INavigationService _navigation;
    private readonly IInteractionService _interaction;
    private readonly IAnimationService _animation;

    public MainWindow(
        MainWindowViewModel viewModel,
        IThemeService themeService,
        INavigationService navigationService,
        IInteractionService interactionService,
        IAnimationService animationService)
    {
        // x:Bind 在 InitializeComponent 期间求值，必须先赋值 ViewModel。
        ViewModel = viewModel;
        _navigation = navigationService;
        _interaction = interactionService;
        _animation = animationService;
        InitializeComponent();
        Title = viewModel.AppTitle;

        // 窗口内容就绪后再绑定根元素，主题切换即时生效。
        var root = (FrameworkElement)Content;
        themeService.Attach(root);

        // 通知宿主与对话框需要在内容进入可视树后才有 XamlRoot。
        NotificationArea.Attach(interactionService);
        root.Loaded += OnRootLoaded;

        SetupCustomTitleBar();

        _navigation.Initialize(ContentFrame);

        // 页面过渡跟随「界面动画」开关：切换后立即生效，无需重启。
        _animation.ApplyFrameTransition(ContentFrame);
        _animation.Changed += OnAnimationSettingChanged;

        // 默认打开启动页，并同步侧边栏选中项。
        _navigation.Navigate("launch");
        ShellNavigation.SelectedItem = ShellNavigation.MenuItems[0];
    }

    /// <summary>「界面动画」开关变化时，立即更新导航帧的页面过渡。</summary>
    private void OnAnimationSettingChanged(object? sender, EventArgs e)
        => _animation.ApplyFrameTransition(ContentFrame);

    /// <summary>内容进入可视树后，把 XamlRoot 交给交互服务，ContentDialog 才能显示。</summary>
    private void OnRootLoaded(object sender, RoutedEventArgs e)
    {
        if (((FrameworkElement)sender).XamlRoot is { } xamlRoot)
        {
            _interaction.AttachRoot(xamlRoot);
        }
    }

    /// <summary>
    /// 启用自定义标题栏：内容延伸进标题栏区域，窗口按钮背景透明以露出 Mica 材质。
    /// </summary>
    private void SetupCustomTitleBar()
    {
        ExtendsContentIntoTitleBar = true;
        SetTitleBar(AppTitleBar);

        var titleBar = AppWindow.TitleBar;
        titleBar.ButtonBackgroundColor = Colors.Transparent;
        titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
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
