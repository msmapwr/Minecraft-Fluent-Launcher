using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using WINUI.Models;
using WINUI.ViewModels;

namespace WINUI.Views;

/// <summary>
/// 版本详情页（二级页面，由下载中心「查看」进入）。
/// <para>展示版本信息，并支持多选模组加载器后一并安装（Mock 演示）。</para>
/// </summary>
public sealed partial class VersionDetailPage : Page
{
    /// <summary>视图模型。<b>必须在 <c>InitializeComponent</c> 之前赋值</b>。</summary>
    public VersionDetailPageViewModel ViewModel { get; }

    public VersionDetailPage()
    {
        ViewModel = App.GetService<VersionDetailPageViewModel>();
        InitializeComponent();
    }

    /// <summary>接收导航参数（<see cref="DownloadItem"/>）并载入详情。</summary>
    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        _ = ViewModel.LoadAsync(e.Parameter as DownloadItem);
    }
}
