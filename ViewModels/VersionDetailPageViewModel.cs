using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WINUI.Models;
using WINUI.Services;

namespace WINUI.ViewModels;

/// <summary>
/// 版本详情页的视图模型。
/// <para>
/// 展示某个游戏版本的信息，并允许<b>多选模组加载器</b>后一并安装。
/// 安装为纯 UI 演示：只走进度与文案，<b>不下载任何文件，也不创建真实实例</b>。
/// </para>
/// </summary>
public sealed partial class VersionDetailPageViewModel : ObservableObject
{
    private readonly ILauncherDataService _dataService;
    private readonly INavigationService _navigation;

    private string _summaryTitle = "—";
    private string _summaryText = "—";
    private string _instanceName = "—";

    /// <summary>当前版本条目（由导航参数传入）。</summary>
    [ObservableProperty]
    public partial DownloadItem? Version { get; set; }

    /// <summary>是否正在安装。</summary>
    [ObservableProperty]
    public partial bool IsInstalling { get; set; }

    /// <summary>安装进度（0–100）。</summary>
    [ObservableProperty]
    public partial double InstallProgress { get; set; }

    /// <summary>状态栏文案。</summary>
    [ObservableProperty]
    public partial string StatusMessage { get; set; }

    /// <summary>可选加载器。</summary>
    public ObservableCollection<LoaderOptionViewModel> Loaders { get; } = [];

    /// <summary>摘要标题，如 <c>Minecraft 1.21.4 + Fabric 0.16.9</c>。</summary>
    public string SummaryTitle
    {
        get => _summaryTitle;
        private set => SetProperty(ref _summaryTitle, value);
    }

    /// <summary>摘要正文（逐行列出将安装的内容）。</summary>
    public string SummaryText
    {
        get => _summaryText;
        private set => SetProperty(ref _summaryText, value);
    }

    /// <summary>将要创建的实例名（演示）。</summary>
    public string InstanceName
    {
        get => _instanceName;
        private set => SetProperty(ref _instanceName, value);
    }

    /// <summary>是否已载入版本信息。</summary>
    public bool HasVersion => Version is not null;

    /// <summary>该版本是否有可用的第三方加载器。</summary>
    public bool HasLoaders => Loaders.Count > 0;

    /// <summary>该版本是否没有可用的第三方加载器。</summary>
    public bool HasNoLoaders => Loaders.Count == 0;

    /// <summary>已勾选的加载器数量。</summary>
    public int SelectedLoaderCount => Loaders.Count(loader => loader.IsSelected);

    /// <summary>页头标题。</summary>
    public string Title => Version?.Name ?? "版本详情";

    /// <summary>页头副标题。</summary>
    public string Subtitle => Version is null
        ? string.Empty
        : $"{Version.Version} · {Version.ChannelLabel} · 发布于 {Version.ReleasedAtLabel}";

    /// <summary>已选加载器摘要。</summary>
    public string SelectedLoaderCountLabel => Loaders.Count == 0
        ? "该版本无可用加载器"
        : $"已选 {SelectedLoaderCount} / {Loaders.Count} 个加载器";

    public VersionDetailPageViewModel(ILauncherDataService dataService, INavigationService navigation)
    {
        _dataService = dataService;
        _navigation = navigation;
        StatusMessage = "正在读取版本信息…";
    }

    // 本页由导航进入（OnNavigatedTo）时装配加载器列表；为避免与 XAML 布局冲突，
    // 集合重建同样推迟到 Dispatcher 回调执行。
    private readonly BoundCollectionUpdater _loadersUpdater = new();

    /// <summary>由页面在导航进入时调用，载入指定版本与可用加载器。</summary>
    /// <param name="item">导航参数（版本条目）。</param>
    public async Task LoadAsync(DownloadItem? item)
    {
        Version = item;
        IsInstalling = false;
        InstallProgress = 0;

        foreach (var loader in Loaders)
        {
            loader.PropertyChanged -= OnLoaderPropertyChanged;
        }

        var entries = item is null
            ? Array.Empty<LoaderEntry>()
            : (await _dataService.GetLoadersAsync(item.Version)).ToArray();

        _loadersUpdater.Request(() =>
        {
            Loaders.Clear();

            foreach (var entry in entries)
            {
                var option = new LoaderOptionViewModel(entry);
                option.PropertyChanged += OnLoaderPropertyChanged;
                Loaders.Add(option);
            }

            RefreshSummary();
            NotifyLoaderState();
        });

        StatusMessage = item is null
            ? "未获取到版本信息，请返回下载中心重试"
            : entries.Length == 0
                ? "该版本不支持第三方模组加载器，将按原版安装"
                : $"已读取 {entries.Length} 个可用加载器，可多选后一并安装";
    }

    partial void OnVersionChanged(DownloadItem? value)
    {
        OnPropertyChanged(nameof(HasVersion));
        OnPropertyChanged(nameof(Title));
        OnPropertyChanged(nameof(Subtitle));
        InstallCommand.NotifyCanExecuteChanged();
    }

    partial void OnIsInstallingChanged(bool value) => InstallCommand.NotifyCanExecuteChanged();

    private void OnLoaderPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(LoaderOptionViewModel.IsSelected) or nameof(LoaderOptionViewModel.SelectedVersion))
        {
            RefreshSummary();
        }
    }

    private void NotifyLoaderState()
    {
        OnPropertyChanged(nameof(HasLoaders));
        OnPropertyChanged(nameof(HasNoLoaders));
        OnPropertyChanged(nameof(SelectedLoaderCount));
        OnPropertyChanged(nameof(SelectedLoaderCountLabel));
    }

    /// <summary>根据当前勾选情况刷新安装摘要。</summary>
    private void RefreshSummary()
    {
        if (Version is null)
        {
            SummaryTitle = "—";
            SummaryText = "—";
            InstanceName = "—";
            NotifyLoaderState();
            return;
        }

        var selected = Loaders.Where(loader => loader.IsSelected).ToList();

        if (selected.Count == 0)
        {
            SummaryTitle = $"仅 Minecraft {Version.Version}（原版）";
            SummaryText = $"Minecraft {Version.Version}";
            InstanceName = $"{Version.Version} · 原版";
        }
        else
        {
            var parts = selected
                .Select(loader => $"{loader.Label} {loader.SelectedVersion?.Value ?? loader.Entry.RecommendedVersion}")
                .ToList();

            SummaryTitle = $"Minecraft {Version.Version} + {string.Join(" + ", parts)}";
            SummaryText = $"Minecraft {Version.Version}\n" + string.Join("\n", parts.Select(part => $"+ {part}"));
            InstanceName = $"{Version.Version} · {parts[0]}";
        }

        NotifyLoaderState();
        InstallCommand.NotifyCanExecuteChanged();
    }

    private bool CanInstall() => Version is not null && !IsInstalling;

    /// <summary>按当前选择一并安装（演示）。</summary>
    [RelayCommand(CanExecute = nameof(CanInstall))]
    private async Task InstallAsync()
    {
        if (Version is null)
        {
            return;
        }

        IsInstalling = true;
        try
        {
            InstallProgress = 0;

            string[] stages = ["校验版本文件", "准备模组加载器", "生成实例目录"];
            var stepCount = stages.Length * 4;

            foreach (var stage in stages)
            {
                StatusMessage = $"{stage}…";
                for (var step = 0; step < 4; step++)
                {
                    await Task.Delay(80);
                    InstallProgress = Math.Min(100, InstallProgress + 100.0 / stepCount);
                }
            }

            InstallProgress = 100;
            StatusMessage = $"已创建实例「{InstanceName}」（演示，未下载任何文件）";
        }
        finally
        {
            IsInstalling = false;
        }
    }

    /// <summary>返回下载中心。</summary>
    [RelayCommand]
    private void GoBack() => _navigation.GoBack();
}
