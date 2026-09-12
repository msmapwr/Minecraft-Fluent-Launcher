using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WINUI.Models;
using WINUI.Services;

namespace WINUI.ViewModels;

/// <summary>
/// 设置页的视图模型。
/// <para>
/// 本页是<b>真实持久化</b>的：所有开关与下拉的改动都会立即写入
/// <c>%LOCALAPPDATA%\MinecraftFluentLauncher\settings.json</c>，并在下次启动时恢复。
/// 仅「浏览 Java 路径」与「打开数据目录」为演示行为。
/// </para>
/// </summary>
public sealed partial class SettingsPageViewModel : ObservableObject
{
    private readonly ISettingsService _settingsService;
    private readonly IThemeService _themeService;
    private readonly AppSettings _settings;

    /// <summary>加载期间抑制回写，避免构造时反复保存。</summary>
    private bool _isLoading = true;

    // ==================== 外观 ====================

    /// <summary>主题。</summary>
    [ObservableProperty]
    public partial SelectOption<AppTheme> SelectedTheme { get; set; }

    /// <summary>界面语言。</summary>
    [ObservableProperty]
    public partial SelectOption<string> SelectedLanguage { get; set; }

    /// <summary>是否启用界面动画。</summary>
    [ObservableProperty]
    public partial bool EnableAnimations { get; set; }

    /// <summary>是否显示快照 / 预览版本。</summary>
    [ObservableProperty]
    public partial bool ShowSnapshots { get; set; }

    // ==================== 启动器行为 ====================

    /// <summary>是否在启动时检查更新。</summary>
    [ObservableProperty]
    public partial bool AutoCheckUpdates { get; set; }

    /// <summary>启动游戏后是否自动关闭启动器。</summary>
    [ObservableProperty]
    public partial bool CloseLauncherAfterLaunch { get; set; }

    // ==================== Java 运行时 ====================

    /// <summary>是否自动检测 Java。</summary>
    [ObservableProperty]
    public partial bool AutoDetectJava { get; set; }

    /// <summary>手动指定的 Java 路径。</summary>
    [ObservableProperty]
    public partial string JavaPath { get; set; }

    /// <summary>默认最小内存（MB）。</summary>
    [ObservableProperty]
    public partial double MinMemoryMb { get; set; }

    /// <summary>默认最大内存（MB）。</summary>
    [ObservableProperty]
    public partial double MaxMemoryMb { get; set; }

    // ==================== 下载 ====================

    /// <summary>偏好下载源。</summary>
    [ObservableProperty]
    public partial SelectOption<DownloadSource> SelectedDownloadSource { get; set; }

    /// <summary>最大并发下载数。</summary>
    [ObservableProperty]
    public partial double MaxConcurrentDownloads { get; set; }

    // ==================== 诊断 ====================

    /// <summary>日志级别。</summary>
    [ObservableProperty]
    public partial SelectOption<AppLogLevel> SelectedLogLevel { get; set; }

    /// <summary>状态栏文案。</summary>
    [ObservableProperty]
    public partial string StatusMessage { get; set; }

    /// <summary>主题选项。</summary>
    public IReadOnlyList<SelectOption<AppTheme>> Themes { get; } =
    [
        new(AppTheme.System, "跟随系统"),
        new(AppTheme.Light, "浅色"),
        new(AppTheme.Dark, "深色"),
    ];

    /// <summary>语言选项。</summary>
    public IReadOnlyList<SelectOption<string>> Languages { get; } =
    [
        new("zh-CN", "简体中文"),
        new("en-US", "English"),
    ];

    /// <summary>下载源选项。</summary>
    public IReadOnlyList<SelectOption<DownloadSource>> DownloadSources { get; } =
    [
        new(DownloadSource.Official, "官方源"),
        new(DownloadSource.Bmclapi, "BMCLAPI 镜像"),
        new(DownloadSource.Community, "社区镜像"),
    ];

    /// <summary>日志级别选项。</summary>
    public IReadOnlyList<SelectOption<AppLogLevel>> LogLevels { get; } =
    [
        new(AppLogLevel.Error, "仅错误"),
        new(AppLogLevel.Warning, "警告"),
        new(AppLogLevel.Info, "常规"),
        new(AppLogLevel.Debug, "调试"),
        new(AppLogLevel.Trace, "最详细"),
    ];

    /// <summary>最小内存标签。</summary>
    public string MinMemoryLabel => $"{MinMemoryMb:0} MB";

    /// <summary>最大内存标签。</summary>
    public string MaxMemoryLabel => $"{MaxMemoryMb:0} MB";

    /// <summary>并发下载数标签。</summary>
    public string ConcurrencyLabel => $"{MaxConcurrentDownloads:0} 个";

    /// <summary>数据目录（真实路径）。</summary>
    public string DataDirectory => _settingsService.DataDirectory;

    /// <summary>手动 Java 路径是否可编辑。</summary>
    public bool IsJavaPathEditable => !AutoDetectJava;

    /// <summary>破坏性操作的二次确认与操作结果通知。</summary>
    private readonly IInteractionService _interaction;

    /// <summary>「界面动画」开关的真正持有者，切换后立即广播到界面。</summary>
    private readonly IAnimationService _animation;

    public SettingsPageViewModel(
        ISettingsService settingsService,
        IThemeService themeService,
        IInteractionService interaction,
        IAnimationService animation)
    {
        _settingsService = settingsService;
        _themeService = themeService;
        _interaction = interaction;
        _animation = animation;
        _settings = settingsService.Settings;

        StatusMessage = "设置会自动保存";

        SelectedTheme = Themes.First(option => option.Value == _settings.Theme);
        SelectedLanguage = Languages.FirstOrDefault(option => option.Value == _settings.Language) ?? Languages[0];
        EnableAnimations = _settings.EnableAnimations;
        ShowSnapshots = _settings.ShowSnapshots;
        AutoCheckUpdates = _settings.AutoCheckUpdates;
        CloseLauncherAfterLaunch = _settings.CloseLauncherAfterLaunch;
        AutoDetectJava = _settings.AutoDetectJava;
        JavaPath = _settings.JavaPath;
        MinMemoryMb = _settings.MinMemoryMb;
        MaxMemoryMb = _settings.MaxMemoryMb;
        SelectedDownloadSource = DownloadSources.First(option => option.Value == _settings.DownloadSource);
        MaxConcurrentDownloads = _settings.MaxConcurrentDownloads;
        SelectedLogLevel = LogLevels.First(option => option.Value == _settings.LogLevel);

        _isLoading = false;
    }

    // ==================== 回写 ====================

    private void Persist(string message = "设置已保存")
    {
        if (_isLoading)
        {
            return;
        }

        _settingsService.Save();
        StatusMessage = message;
    }

    partial void OnSelectedThemeChanged(SelectOption<AppTheme> value)
    {
        if (_isLoading)
        {
            return;
        }

        // ThemeService 内部会写入设置并落盘，此处不再重复保存。
        _themeService.SetTheme(value.Value);
        StatusMessage = $"主题已切换为「{value.DisplayName}」";
    }

    partial void OnSelectedLanguageChanged(SelectOption<string> value)
    {
        _settings.Language = value.Value;
        Persist(value.Value == "zh-CN" ? "语言已设为简体中文" : "语言已设为 English（界面文案暂未本地化）");
    }

    partial void OnEnableAnimationsChanged(bool value)
    {
        if (_isLoading)
        {
            return;
        }

        // 动画服务负责落盘并广播：页面过渡与内容渐入会立即随之启停，无需重启。
        _animation.SetEnabled(value);
        StatusMessage = value
            ? "界面动画已开启"
            : "界面动画已关闭（页面过渡与内容渐入不再播放）";
    }

    partial void OnShowSnapshotsChanged(bool value)
    {
        _settings.ShowSnapshots = value;
        Persist();
    }

    partial void OnAutoCheckUpdatesChanged(bool value)
    {
        _settings.AutoCheckUpdates = value;
        Persist();
    }

    partial void OnCloseLauncherAfterLaunchChanged(bool value)
    {
        _settings.CloseLauncherAfterLaunch = value;
        Persist();
    }

    partial void OnAutoDetectJavaChanged(bool value)
    {
        _settings.AutoDetectJava = value;
        OnPropertyChanged(nameof(IsJavaPathEditable));
        Persist();
    }

    partial void OnJavaPathChanged(string value)
    {
        _settings.JavaPath = value;
        Persist();
    }

    partial void OnMinMemoryMbChanged(double value)
    {
        var memory = (int)value;
        _settings.MinMemoryMb = memory;

        if (MaxMemoryMb < value)
        {
            MaxMemoryMb = value;
        }

        OnPropertyChanged(nameof(MinMemoryLabel));
        Persist();
    }

    partial void OnMaxMemoryMbChanged(double value)
    {
        _settings.MaxMemoryMb = (int)value;

        if (MinMemoryMb > value)
        {
            MinMemoryMb = value;
        }

        OnPropertyChanged(nameof(MaxMemoryLabel));
        Persist();
    }

    partial void OnSelectedDownloadSourceChanged(SelectOption<DownloadSource> value)
    {
        _settings.DownloadSource = value.Value;
        Persist($"下载源已切换为「{value.DisplayName}」");
    }

    partial void OnMaxConcurrentDownloadsChanged(double value)
    {
        _settings.MaxConcurrentDownloads = (int)value;
        OnPropertyChanged(nameof(ConcurrencyLabel));
        Persist();
    }

    partial void OnSelectedLogLevelChanged(SelectOption<AppLogLevel> value)
    {
        _settings.LogLevel = value.Value;
        Persist();
    }

    // ==================== 命令 ====================

    /// <summary>浏览 Java 可执行文件（演示）。</summary>
    [RelayCommand]
    private void BrowseJavaPath()
    {
        JavaPath = @"C:\Program Files\Java\jdk-21\bin\javaw.exe";
        StatusMessage = "已填入示例路径（演示，未打开文件选择器）";
    }

    /// <summary>打开数据目录（演示）。</summary>
    [RelayCommand]
    private void OpenDataDirectory()
        => StatusMessage = $"（演示）将打开：{DataDirectory}";

    /// <summary>
    /// 恢复默认设置。会覆盖当前的全部设置项，因此先弹出二次确认。
    /// </summary>
    [RelayCommand]
    private async Task ResetSettingsAsync()
    {
        var confirmed = await _interaction.ConfirmAsync(
            "恢复默认设置",
            "所有设置项都会恢复为初始值（主题、语言、界面动画、内存、下载源与日志级别等）。\n账户与实例数据不受影响，但此操作无法撤销。",
            "恢复默认",
            "取消");

        if (!confirmed)
        {
            StatusMessage = "已取消恢复默认设置";
            return;
        }

        var defaults = new AppSettings();

        _settings.Version = defaults.Version;
        _settings.Theme = defaults.Theme;
        _settings.Language = defaults.Language;
        _settings.EnableAnimations = defaults.EnableAnimations;
        _settings.ShowSnapshots = defaults.ShowSnapshots;
        _settings.AutoCheckUpdates = defaults.AutoCheckUpdates;
        _settings.CloseLauncherAfterLaunch = defaults.CloseLauncherAfterLaunch;
        _settings.AutoDetectJava = defaults.AutoDetectJava;
        _settings.JavaPath = defaults.JavaPath;
        _settings.MinMemoryMb = defaults.MinMemoryMb;
        _settings.MaxMemoryMb = defaults.MaxMemoryMb;
        _settings.DownloadSource = defaults.DownloadSource;
        _settings.MaxConcurrentDownloads = defaults.MaxConcurrentDownloads;
        _settings.LogLevel = defaults.LogLevel;

        SyncFromSettings();
        _themeService.SetTheme(defaults.Theme);
        _animation.SetEnabled(defaults.EnableAnimations);
        _settingsService.Save();

        StatusMessage = "已恢复默认设置（不含账户与实例数据）";

        _interaction.Notify(
            "设置已恢复为默认值（不含账户与实例数据）",
            NotificationSeverity.Success,
            "已恢复默认");
    }

    /// <summary>把设置实例的当前值刷回界面。</summary>
    private void SyncFromSettings()
    {
        _isLoading = true;

        SelectedTheme = Themes.First(option => option.Value == _settings.Theme);
        SelectedLanguage = Languages.FirstOrDefault(option => option.Value == _settings.Language) ?? Languages[0];
        EnableAnimations = _settings.EnableAnimations;
        ShowSnapshots = _settings.ShowSnapshots;
        AutoCheckUpdates = _settings.AutoCheckUpdates;
        CloseLauncherAfterLaunch = _settings.CloseLauncherAfterLaunch;
        AutoDetectJava = _settings.AutoDetectJava;
        JavaPath = _settings.JavaPath;
        MinMemoryMb = _settings.MinMemoryMb;
        MaxMemoryMb = _settings.MaxMemoryMb;
        SelectedDownloadSource = DownloadSources.First(option => option.Value == _settings.DownloadSource);
        MaxConcurrentDownloads = _settings.MaxConcurrentDownloads;
        SelectedLogLevel = LogLevels.First(option => option.Value == _settings.LogLevel);

        _isLoading = false;

        OnPropertyChanged(nameof(MinMemoryLabel));
        OnPropertyChanged(nameof(MaxMemoryLabel));
        OnPropertyChanged(nameof(ConcurrencyLabel));
        OnPropertyChanged(nameof(IsJavaPathEditable));
    }
}
