using Microsoft.UI.Xaml.Controls;
using WINUI.ViewModels;

namespace WINUI.Views;

/// <summary>模组管理页。列表、筛选与详情均由视图模型驱动（Mock 数据）。</summary>
public sealed partial class ModsPage : Page
{
    /// <summary>视图模型。<b>必须在 <c>InitializeComponent</c> 之前赋值</b>。</summary>
    public ModsPageViewModel ViewModel { get; }

    public ModsPage()
    {
        ViewModel = App.GetService<ModsPageViewModel>();
        InitializeComponent();
    }

    private void OnSearchBoxTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        => ViewModel.SearchText = sender.Text;
}
