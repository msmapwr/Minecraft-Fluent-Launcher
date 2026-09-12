using Microsoft.UI.Xaml;
using WINUI.ViewModels;

namespace WINUI.Views;

/// <summary>
/// 应用主窗口。视图模型通过构造函数注入（由 DI 容器解析）。
/// </summary>
public sealed partial class MainWindow : Window
{
    /// <summary>供 XAML 的 x:Bind 绑定的视图模型。</summary>
    public MainWindowViewModel ViewModel { get; }

    public MainWindow(MainWindowViewModel viewModel)
    {
        // x:Bind 在 InitializeComponent 期间求值，必须先赋值 ViewModel。
        ViewModel = viewModel;
        InitializeComponent();
        Title = viewModel.AppTitle;
    }
}
