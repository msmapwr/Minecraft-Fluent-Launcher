using System.Collections.Specialized;
using Microsoft.UI.Xaml.Controls;
using WINUI.ViewModels;

namespace WINUI.Views;

/// <summary>日志页。筛选与复制由视图模型处理；自动滚动属于纯视图行为，放在代码后置。</summary>
public sealed partial class LogsPage : Page
{
    /// <summary>视图模型。<b>必须在 <c>InitializeComponent</c> 之前赋值</b>。</summary>
    public LogsPageViewModel ViewModel { get; }

    public LogsPage()
    {
        ViewModel = App.GetService<LogsPageViewModel>();
        InitializeComponent();

        ViewModel.Entries.CollectionChanged += OnEntriesChanged;
    }

    private void OnSearchBoxTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        => ViewModel.SearchText = sender.Text;

    /// <summary>列表内容变化后，按需滚动到最新一条。</summary>
    private void OnEntriesChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (!ViewModel.AutoScroll || ViewModel.Entries.Count == 0)
        {
            return;
        }

        LogList.ScrollIntoView(ViewModel.Entries[^1]);
    }
}
