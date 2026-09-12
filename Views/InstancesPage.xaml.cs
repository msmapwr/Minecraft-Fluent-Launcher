using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WINUI.Models;
using WINUI.ViewModels;

namespace WINUI.Views;

/// <summary>
/// 实例列表页。
/// <para>
/// 搜索 / 筛选 / 排序由视图模型完成；卡片内的操作按钮通过 <c>Tag</c> 携带的
/// <see cref="GameInstance"/> 回调视图模型（<c>x:Bind</c> 无法在 <c>DataTemplate</c>
/// 内跨命名空间访问页面级命令，故采用事件回调）。
/// </para>
/// </summary>
public sealed partial class InstancesPage : Page
{
    /// <summary>视图模型。<b>必须在 <c>InitializeComponent</c> 之前赋值</b>。</summary>
    public InstancesPageViewModel ViewModel { get; }

    public InstancesPage()
    {
        ViewModel = App.GetService<InstancesPageViewModel>();
        InitializeComponent();
    }

    private void OnSearchBoxTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        => ViewModel.SearchText = sender.Text;

    private void OnLaunchClick(object sender, RoutedEventArgs e)
    {
        if (ResolveInstance(sender) is { } instance)
        {
            ViewModel.Launch(instance);
        }
    }

    private void OnOpenFolderClick(object sender, RoutedEventArgs e)
    {
        if (ResolveInstance(sender) is { } instance)
        {
            ViewModel.OpenFolder(instance);
        }
    }

    /// <summary>
    /// 删除实例。删除前会弹二次确认，故此处需要异步等待；
    /// 事件处理器使用 <c>async void</c> 是 WinUI 事件的标准写法。
    /// </summary>
    private async void OnDeleteClick(object sender, RoutedEventArgs e)
    {
        if (ResolveInstance(sender) is { } instance)
        {
            await ViewModel.DeleteAsync(instance);
        }
    }

    /// <summary>从按钮的 <c>Tag</c>（或 <c>DataContext</c>）取回卡片对应的实例。</summary>
    private static GameInstance? ResolveInstance(object sender) => sender switch
    {
        FrameworkElement { Tag: GameInstance instance } => instance,
        FrameworkElement { DataContext: GameInstance instance } => instance,
        _ => null,
    };
}
