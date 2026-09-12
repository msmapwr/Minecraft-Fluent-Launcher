using Microsoft.UI.Xaml.Controls;
using WINUI.ViewModels;

namespace WINUI.Views;

/// <summary>
/// 启动页。视图模型由容器解析——页面本身由 <c>Frame.Navigate(Type)</c> 反射创建，
/// 无法使用构造函数注入，故通过 <see cref="App.GetService{T}"/> 取得。
/// </summary>
public sealed partial class LaunchPage : Page
{
    /// <summary>视图模型。<b>必须在 <c>InitializeComponent</c> 之前赋值</b>，否则 x:Bind 会取到 null。</summary>
    public LaunchPageViewModel ViewModel { get; }

    public LaunchPage()
    {
        ViewModel = App.GetService<LaunchPageViewModel>();
        InitializeComponent();
    }
}
