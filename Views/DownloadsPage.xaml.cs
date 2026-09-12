using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WINUI.Models;
using WINUI.ViewModels;

namespace WINUI.Views;

/// <summary>下载中心页。条目列表（含分页）与下载队列均由视图模型驱动（Mock 数据）。</summary>
public sealed partial class DownloadsPage : Page
{
    /// <summary>视图模型。<b>必须在 <c>InitializeComponent</c> 之前赋值</b>。</summary>
    public DownloadsPageViewModel ViewModel { get; }

    public DownloadsPage()
    {
        ViewModel = App.GetService<DownloadsPageViewModel>();
        InitializeComponent();
    }

    private void OnSearchBoxTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        => ViewModel.SearchText = sender.Text;

    private void OnDownloadClick(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { Tag: DownloadItem item })
        {
            ViewModel.Enqueue(item);
        }
    }

    /// <summary>「查看」按钮：进入版本详情页。</summary>
    private void OnDetailClick(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { Tag: DownloadItem item })
        {
            ViewModel.OpenDetail(item);
        }
    }

    /// <summary>数字页码按钮。</summary>
    private void OnPageClick(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { Tag: int page })
        {
            ViewModel.GoToPage(page);
        }
    }
}
