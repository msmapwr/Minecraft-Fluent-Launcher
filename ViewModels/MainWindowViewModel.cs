using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using WINUI.Models;
using WINUI.Services;

namespace WINUI.ViewModels;

/// <summary>
/// 应用外壳的视图模型。
/// 承载窗口标题、主题切换与「下载中 N」全局徽标（大更新 ⑧-4）；
/// 侧边导航结构由 MainWindow.xaml 声明。
/// </summary>
public sealed partial class MainWindowViewModel : ObservableObject
{
    private readonly IThemeService _themeService;

    /// <summary>应用标题。</summary>
    [ObservableProperty]
    public partial string AppTitle { get; set; }

    /// <summary>当前选中的主题选项。</summary>
    [ObservableProperty]
    public partial ThemeOption SelectedThemeOption { get; set; }

    /// <summary>队列中正在推进的任务数（全局下载徽标）。</summary>
    [ObservableProperty]
    public partial int ActiveDownloadCount { get; set; }

    /// <summary>队列整体进度（0–100，用于标题栏按钮上的小进度环）。</summary>
    [ObservableProperty]
    public partial double DownloadOverallProgress { get; set; }

    /// <summary>是否正在根据主题服务广播同步选中项，避免回环写回。</summary>
    private bool _isSyncingTheme;

    // 队列事件在后台线程触发，徽标更新经 BoundCollectionUpdater 调度到 UI 线程。
    private readonly BoundCollectionUpdater _queueUpdater = new();

    /// <summary>是否显示下载徽标（有推进中的任务）。</summary>
    public bool HasActiveDownloads => ActiveDownloadCount > 0;

    /// <summary>徽标文案。</summary>
    public string DownloadBadgeLabel => HasActiveDownloads ? $"下载中 {ActiveDownloadCount}" : "下载管理";

    /// <summary>可选主题列表（与设置页共用主题服务的单一来源）。</summary>
    public IReadOnlyList<ThemeOption> ThemeOptions { get; }

    public MainWindowViewModel(IThemeService themeService, IDownloadQueueService queue)
    {
        _themeService = themeService;

        AppTitle = AppInfo.DisplayName;

        // 选项列表来自主题服务，保证与设置页完全一致。
        ThemeOptions = themeService.Options;

        // 与已保存的主题偏好保持一致。
        SelectedThemeOption = ThemeOptions.First(option => option.Value == themeService.Current);

        // 设置页切换主题时，同步侧边栏下拉的选中项。
        themeService.ThemeChanged += OnThemeServiceChanged;

        // 全局下载徽标：队列任一变化都重读当前状态。
        queue.TaskAdded += OnQueueChanged;
        queue.TaskChanged += OnQueueChanged;
        queue.TaskRemoved += OnQueueChanged;
        RefreshQueueBadge(queue);
    }

    private void OnQueueChanged(object? sender, DownloadTask task)
        => _queueUpdater.Request(() => RefreshQueueBadge((IDownloadQueueService)sender!));

    private void RefreshQueueBadge(IDownloadQueueService queue)
    {
        var tasks = queue.Tasks;

        ActiveDownloadCount = tasks.Count(task => task.Status.IsActive());
        DownloadOverallProgress = tasks.Count == 0
            ? 0
            : tasks.Average(task => task.Progress);

        OnPropertyChanged(nameof(HasActiveDownloads));
        OnPropertyChanged(nameof(DownloadBadgeLabel));
    }

    /// <summary>选中项变化时立即应用主题。</summary>
    partial void OnSelectedThemeOptionChanged(ThemeOption value)
    {
        if (_isSyncingTheme)
        {
            return;
        }

        _themeService.SetTheme(value.Value);
    }

    private void OnThemeServiceChanged(object? sender, AppTheme theme)
    {
        var option = ThemeOptions.FirstOrDefault(item => item.Value == theme);
        if (option is null || ReferenceEquals(option, SelectedThemeOption))
        {
            return;
        }

        _isSyncingTheme = true;
        try
        {
            SelectedThemeOption = option;
        }
        finally
        {
            _isSyncingTheme = false;
        }
    }
}
