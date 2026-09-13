using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WINUI.ViewModels;

namespace WINUI.Views;

/// <summary>下载队列页（二级页）。任务操作以 Tag 转发给视图模型。</summary>
public sealed partial class DownloadQueuePage : Page
{
    /// <summary>视图模型。<b>必须在 <c>InitializeComponent</c> 之前赋值</b>。</summary>
    public DownloadQueuePageViewModel ViewModel { get; }

    public DownloadQueuePage()
    {
        ViewModel = App.GetService<DownloadQueuePageViewModel>();
        InitializeComponent();
    }

    private void OnCancelClick(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { Tag: DownloadTaskViewModel task })
        {
            ViewModel.CancelTask(task);
        }
    }

    private void OnRetryClick(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { Tag: DownloadTaskViewModel task })
        {
            ViewModel.RetryTask(task);
        }
    }

    private void OnRemoveClick(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { Tag: DownloadTaskViewModel task })
        {
            ViewModel.RemoveTask(task);
        }
    }

    /// <summary>完成引导：去启动游戏。</summary>
    private void OnLaunchClick(object sender, RoutedEventArgs e) => ViewModel.GoLaunch();

    /// <summary>完成引导：查看实例。</summary>
    private void OnViewInstancesClick(object sender, RoutedEventArgs e) => ViewModel.GoInstances();
}
