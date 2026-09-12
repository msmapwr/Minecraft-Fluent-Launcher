using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WINUI.Models;
using WINUI.ViewModels;

namespace WINUI.Views;

/// <summary>账户页。微软登录与离线账户管理均为演示（Mock 数据）。</summary>
public sealed partial class AccountsPage : Page
{
    /// <summary>视图模型。<b>必须在 <c>InitializeComponent</c> 之前赋值</b>。</summary>
    public AccountsPageViewModel ViewModel { get; }

    public AccountsPage()
    {
        ViewModel = App.GetService<AccountsPageViewModel>();
        InitializeComponent();
    }

    private void OnUseOfflineClick(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { Tag: OfflineAccount account })
        {
            ViewModel.UseOfflineAccount(account);
        }
    }

    private void OnRemoveOfflineClick(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { Tag: OfflineAccount account })
        {
            ViewModel.RemoveOfflineAccount(account);
        }
    }
}
