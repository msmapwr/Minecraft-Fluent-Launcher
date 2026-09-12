using Microsoft.UI.Xaml.Controls;
using WINUI.ViewModels;

namespace WINUI.Views;

/// <summary>关于页。产品信息与运行环境为真实取值。</summary>
public sealed partial class AboutPage : Page
{
    /// <summary>视图模型。<b>必须在 <c>InitializeComponent</c> 之前赋值</b>。</summary>
    public AboutPageViewModel ViewModel { get; }

    public AboutPage()
    {
        ViewModel = App.GetService<AboutPageViewModel>();
        InitializeComponent();
    }
}
