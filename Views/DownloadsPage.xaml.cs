using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WINUI.ViewModels;

namespace WINUI.Views;

/// <summary>
/// 下载中心页（大更新 ⑧ 重构）。
/// 资源类型入口 → 条目浏览 → 安装确认对话框；入队与进度由全局下载队列承担。
/// </summary>
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

    /// <summary>「安装」按钮：弹出安装确认对话框，确认后进入全局下载队列。</summary>
    private async void OnInstallClick(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement { Tag: DownloadItemViewModel item })
        {
            return;
        }

        var dialog = await InstallConfirmDialog.CreateAsync(item.Item);
        dialog.XamlRoot = XamlRoot;

        var result = await dialog.ShowAsync();

        if (result == ContentDialogResult.Primary && dialog.Result is not null)
        {
            ViewModel.ConfirmInstall(item, dialog.Result);
        }
    }

    /// <summary>「查看」按钮：进入版本详情页。</summary>
    private void OnDetailClick(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { Tag: DownloadItemViewModel item })
        {
            ViewModel.OpenDetail(item);
        }
    }

    /// <summary>资源类型卡点击。</summary>
    private void OnCategoryCardClick(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { Tag: CategoryCardViewModel card })
        {
            ViewModel.SelectCategory(card);
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
