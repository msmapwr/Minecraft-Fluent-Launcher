using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WINUI.Models;
using WINUI.ViewModels;

namespace WINUI.Views;

/// <summary>下载中心页。条目列表与下载队列均由视图模型驱动（Mock 数据）。</summary>
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
}
