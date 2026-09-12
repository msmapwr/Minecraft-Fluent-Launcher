using CommunityToolkit.Mvvm.ComponentModel;
using WINUI.Models;

namespace WINUI.ViewModels;

/// <summary>下载队列中的一个任务。</summary>
public sealed partial class DownloadTaskViewModel : ObservableObject
{
    /// <summary>来源条目。</summary>
    public DownloadItem Item { get; }

    /// <summary>进度（0–100）。</summary>
    [ObservableProperty]
    public partial double Progress { get; set; }

    /// <summary>状态文案。</summary>
    [ObservableProperty]
    public partial string Status { get; set; }

    /// <summary>任务名称。</summary>
    public string Name => Item.Name;

    /// <summary>体积标签。</summary>
    public string SizeLabel => Item.SizeLabel;

    /// <summary>进度标签。</summary>
    public string ProgressLabel => $"{Progress:0}%";

    /// <summary>是否正在下载（用于显示进度条）。</summary>
    public bool IsActive => Progress > 0 && Progress < 100;

    /// <summary>是否已完成。</summary>
    public bool IsCompleted => Progress >= 100;

    public DownloadTaskViewModel(DownloadItem item)
    {
        Item = item;
        Progress = 0;
        Status = "排队中";
    }

    partial void OnProgressChanged(double value)
    {
        OnPropertyChanged(nameof(ProgressLabel));
        OnPropertyChanged(nameof(IsActive));
        OnPropertyChanged(nameof(IsCompleted));
    }
}
