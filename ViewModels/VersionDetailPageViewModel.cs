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
/// 大更新 ⑦-2 起「安装」接入 CMLLib 真实下载（安装到启动器自有游戏目录）；
/// 加载器安装尚未实现（⑦-5），勾选加载器时按原版安装并在摘要中说明。
/// </para>
/// <para>
/// 页面状态（加载 / 内容 / 错误）由 <see cref="PageViewModelBase"/> 提供。
/// 本页没有「空」状态：加载器为空是有效结果，由加载器区域自行说明。
/// </para>
/// </summary>
public sealed partial class VersionDetailPageViewModel : PageViewModelBase
{
    private readonly ILauncherDataService _dataService;
    private readonly IGameLauncherService _gameLauncher;
    private readonly IInteractionService _interaction;
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

    /// <summary>
    /// 错误状态下的恢复动作：有版本信息就重试加载器清单，
    /// 拿不到版本信息（导航参数缺失）则返回下载中心重新选择。
    /// </summary>
    public IRelayCommand RetryCommand => Version is null ? GoBackCommand : ReloadCommand;

    /// <summary>「重试」按钮的文案，与 <see cref="RetryCommand"/> 的动作保持一致。</summary>
    public string RetryText => Version is null ? "返回下载中心" : "重试";

    public VersionDetailPageViewModel(
        ILauncherDataService dataService,
        IGameLauncherService gameLauncher,
        IInteractionService interaction,
        INavigationService navigation)
    {
        _dataService = dataService;
        _gameLauncher = gameLauncher;
        _interaction = interaction;
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

        if (item is null)
        {
            // 没有拿到导航参数：视为错误，引导用户返回下载中心重新选择。
            ErrorMessage = "未获取到版本信息。请返回下载中心，重新选择要查看的版本。";
            State = PageState.Error;
            StatusMessage = "未获取到版本信息";
            RefreshSummary();
            return;
        }

        LoaderEntry[] entries = [];

        var loaded = await RunLoadAsync(
            async () => entries = (await _dataService.GetLoadersAsync(item.Version)).ToArray(),
            $"无法读取 Minecraft {item.Version} 的加载器清单");

        if (!loaded)
        {
            StatusMessage = "加载器清单加载失败";
            RefreshSummary();
            return;
        }

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

        StatusMessage = entries.Length == 0
            ? "该版本不支持第三方模组加载器，将按原版安装"
            : $"已读取 {entries.Length} 个可用加载器，可多选后一并安装";
    }

    /// <summary>重新载入当前版本（错误状态下的「重试」）。</summary>
    [RelayCommand]
    private Task ReloadAsync() => LoadAsync(Version);

    partial void OnVersionChanged(DownloadItem? value)
    {
        OnPropertyChanged(nameof(HasVersion));
        OnPropertyChanged(nameof(Title));
        OnPropertyChanged(nameof(Subtitle));
        OnPropertyChanged(nameof(RetryCommand));
        OnPropertyChanged(nameof(RetryText));
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

    /// <summary>按当前选择一并安装（真实下载；加载器安装将在 ⑦-5 接入）。</summary>
    [RelayCommand(CanExecute = nameof(CanInstall))]
    private async Task InstallAsync()
    {
        if (Version is null)
        {
            return;
        }

        IsInstalling = true;
        InstallProgress = 5;
        try
        {
            var selectedLoaders = Loaders.Count(loader => loader.IsSelected);
            StatusMessage = selectedLoaders > 0
                ? "加载器安装将在后续版本支持，本次按原版安装…"
                : "开始下载版本文件…";

            // CMLLib：下载并安装版本（已安装时立即返回）。
            await _gameLauncher.InstallAsync(Version.Version);

            InstallProgress = 100;
            StatusMessage = $"已安装到 {_gameLauncher.GamePath.BasePath}";
            _interaction.Notify(
                $"版本 {Version.Version} 已安装完成",
                NotificationSeverity.Success,
                "安装完成");
        }
        catch (Exception ex)
        {
            StatusMessage = $"安装失败：{ex.Message}";
            _interaction.Notify(
                $"版本 {Version.Version} 安装失败：{ex.Message}",
                NotificationSeverity.Error,
                "安装失败");
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
