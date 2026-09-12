using Microsoft.UI.Xaml.Controls;
using WINUI.ViewModels;

namespace WINUI.Views;

/// <summary>设置页。改动会通过 <c>ISettingsService</c> 立即持久化到本地设置文件。</summary>
public sealed partial class SettingsPage : Page
{
    /// <summary>视图模型。<b>必须在 <c>InitializeComponent</c> 之前赋值</b>。</summary>
    public SettingsPageViewModel ViewModel { get; }

    public SettingsPage()
    {
        ViewModel = App.GetService<SettingsPageViewModel>();
        InitializeComponent();
    }
}
