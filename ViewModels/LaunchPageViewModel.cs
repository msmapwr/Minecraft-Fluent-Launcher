using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WINUI.Models;
using WINUI.Services;

namespace WINUI.ViewModels;

/// <summary>
/// 启动页的视图模型。
/// <para>
/// 版本清单来自真实数据（CMLLib）；「启动游戏」自大更新 ⑦-4 起为真实启动：
/// 校验/下载版本 → 解析 Java → 以离线会话拉起 Java 进程，
/// 进程输出实时写入 <see cref="ILogStore"/> 并在日志页展示。
/// </para>
/// </summary>
public sealed partial class LaunchPageViewModel : ObservableObject
{
    private readonly ILauncherDataService _dataService;
    private readonly IGameLauncherService _gameLauncher;
    private readonly IJavaLocatorService _javaLocator;
    private readonly ILogStore _logStore;
    private readonly ISettingsService _settingsService;
    private readonly IInteractionService _interaction;

    /// <summary>捕获 UI 线程调度器（进程退出等后台回调经它更新绑定属性）。</summary>
    private readonly BoundCollectionUpdater _ui = new();

    /// <summary>当前运行中的游戏进程；未运行为 <c>null</c>。</summary>
    private Process? _runningProcess;

    /// <summary>是否正在加载初始数据。</summary>
    [ObservableProperty]
    public partial bool IsLoading { get; set; }

    /// <summary>是否处于启动流程中。</summary>
    [ObservableProperty]
    public partial bool IsLaunching { get; set; }

    /// <summary>游戏是否正在运行。</summary>
    [ObservableProperty]
    public partial bool IsGameRunning { get; set; }

    /// <summary>启动进度（0–100；下载阶段估计值）。</summary>
    [ObservableProperty]
    public partial double LaunchProgress { get; set; }

    /// <summary>状态栏文案。</summary>
    [ObservableProperty]
    public partial string StatusMessage { get; set; }

    /// <summary>当前选中的版本 / 实例。</summary>
    [ObservableProperty]
    public partial GameVersion? SelectedVersion { get; set; }

    /// <summary>当前账户。</summary>
    [ObservableProperty]
    public partial PlayerAccount? Account { get; set; }

    /// <summary>内存分配（MB）。</summary>
    [ObservableProperty]
    public partial double MemoryMb { get; set; }

    /// <summary>可选的版本 / 实例列表。</summary>
    public ObservableCollection<GameVersion> Versions { get; } = [];

    /// <summary>新闻与公告。</summary>
    public ObservableCollection<NewsItem> News { get; } = [];

    /// <summary>主按钮文案，随运行状态变化。</summary>
    public string PrimaryActionText => IsGameRunning
        ? "结束游戏"
        : IsLaunching ? "正在启动…" : "启动游戏";

    /// <summary>主按钮是否可用（启动过程中禁用）。</summary>
    public bool IsPrimaryActionEnabled => !IsLaunching;

    /// <summary>内存分配标签。</summary>
    public string MemoryLabel => $"{MemoryMb:0} MB";

    /// <summary>游戏根目录（真实路径）。</summary>
    public string InstanceDirectory => _gameLauncher.GamePath.BasePath;

    public LaunchPageViewModel(
        ILauncherDataService dataService,
        IGameLauncherService gameLauncher,
        IJavaLocatorService javaLocator,
        ILogStore logStore,
        ISettingsService settingsService,
        IInteractionService interaction)
    {
        _dataService = dataService;
        _gameLauncher = gameLauncher;
        _javaLocator = javaLocator;
        _logStore = logStore;
        _settingsService = settingsService;
        _interaction = interaction;

        StatusMessage = "准备就绪";
        MemoryMb = _settingsService.Settings.MaxMemoryMb;

        _ = InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        IsLoading = true;
        try
        {
            foreach (var version in await _dataService.GetVersionsAsync())
            {
                Versions.Add(version);
            }

            foreach (var item in await _dataService.GetNewsAsync())
            {
                News.Add(item);
            }

            Account = await _dataService.GetCurrentAccountAsync();

            // 默认选中第一个已安装的实例。
            SelectedVersion = Versions.FirstOrDefault(version => version.IsInstalled)
                              ?? Versions.FirstOrDefault();

            StatusMessage = SelectedVersion is null
                ? "尚未安装任何版本"
                : "准备就绪";
        }
        finally
        {
            IsLoading = false;
        }
    }

    partial void OnIsLaunchingChanged(bool value)
    {
        OnPropertyChanged(nameof(PrimaryActionText));
        OnPropertyChanged(nameof(IsPrimaryActionEnabled));
    }

    partial void OnIsGameRunningChanged(bool value) => OnPropertyChanged(nameof(PrimaryActionText));

    partial void OnMemoryMbChanged(double value) => OnPropertyChanged(nameof(MemoryLabel));

    /// <summary>启动 / 结束游戏（真实启动，⑦-4）。</summary>
    [RelayCommand]
    private async Task PrimaryActionAsync()
    {
        if (IsGameRunning)
        {
            try
            {
                _runningProcess?.Kill(entireProcessTree: true);
            }
            catch (Exception ex) when (ex is InvalidOperationException or SystemException)
            {
                _logStore.Log(AppLogLevel.Warning, "Launcher", $"结束游戏进程时出现异常：{ex.Message}");
            }

            return;
        }

        if (IsLaunching || SelectedVersion is null)
        {
            return;
        }

        var versionId = SelectedVersion.Id;
        var playerName = Account?.Name ?? "Player";
        var maxRamMb = (int)MemoryMb;

        // 解析 Java：手动路径优先，其次自动检测。
        var javaPath = _settingsService.Settings.JavaPath;
        if (string.IsNullOrWhiteSpace(javaPath) || !File.Exists(javaPath))
        {
            javaPath = _javaLocator.FindDefaultJavaPath();
        }

        if (string.IsNullOrWhiteSpace(javaPath))
        {
            StatusMessage = "未检测到 Java 运行时，请安装 Java 或在设置中手动指定路径";
            _interaction.Notify(
                "未检测到 Java 运行时。请安装 Java（21+），或在设置页手动指定 javaw.exe 路径。",
                NotificationSeverity.Error,
                "缺少 Java");
            return;
        }

        IsLaunching = true;
        LaunchProgress = 0;
        StatusMessage = "正在校验 / 下载游戏文件…";

        try
        {
            var process = await Task.Run(() =>
            {
                var p = _gameLauncher.LaunchVanilla(versionId, playerName, javaPath, maxRamMb);
                return p;
            });

            AttachProcessOutput(process, versionId);
            _runningProcess = process;

            IsLaunching = false;
            IsGameRunning = true;
            LaunchProgress = 0;
            StatusMessage = $"游戏运行中（{playerName} · {MemoryMb:0} MB）";
            _logStore.Log(AppLogLevel.Info, "Launcher", $"游戏进程已启动：{versionId}（PID {process.Id}）");
        }
        catch (Exception ex)
        {
            IsLaunching = false;
            LaunchProgress = 0;
            StatusMessage = $"启动失败：{ex.Message}";
            _logStore.Log(AppLogLevel.Error, "Launcher", $"启动 {versionId} 失败：{ex.Message}");
            _interaction.Notify(
                $"启动 {versionId} 失败：{ex.Message}",
                NotificationSeverity.Error,
                "启动失败");
        }
    }

    /// <summary>把进程 stdout / stderr 接线到日志仓库，并在退出时回收状态。</summary>
    private void AttachProcessOutput(Process process, string versionId)
    {
        process.EnableRaisingEvents = true;

        process.OutputDataReceived += (_, e) =>
        {
            if (string.IsNullOrEmpty(e.Data))
            {
                return;
            }

            _logStore.Log(ClassifyGameLog(e.Data), "Game", e.Data);
        };

        process.ErrorDataReceived += (_, e) =>
        {
            if (string.IsNullOrEmpty(e.Data))
            {
                return;
            }

            _logStore.Log(AppLogLevel.Warning, "Game", e.Data);
        };

        process.Exited += (_, _) =>
        {
            _ui.Request(() =>
            {
                IsGameRunning = false;
                StatusMessage = $"游戏已退出（{versionId}）";
            });

            _logStore.Log(AppLogLevel.Info, "Launcher", $"游戏进程已退出（{versionId}）");
            _runningProcess = null;
        };

        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
    }

    /// <summary>按 Minecraft 日志行前缀（[INFO]/[WARN]/[ERROR]）粗分级。</summary>
    private static AppLogLevel ClassifyGameLog(string line)
    {
        if (line.Contains("[ERROR]", StringComparison.OrdinalIgnoreCase)
            || line.Contains("ERROR", StringComparison.Ordinal) && line.Contains('[', StringComparison.Ordinal))
        {
            return AppLogLevel.Error;
        }

        if (line.Contains("[WARN]", StringComparison.OrdinalIgnoreCase)
            || line.Contains("WARN", StringComparison.Ordinal) && line.Contains('[', StringComparison.Ordinal))
        {
            return AppLogLevel.Warning;
        }

        return AppLogLevel.Info;
    }

    /// <summary>打开游戏根目录（真实路径）。</summary>
    [RelayCommand]
    private void OpenInstanceFolder()
    {
        StatusMessage = $"游戏目录：{InstanceDirectory}";
        _logStore.Log(AppLogLevel.Info, "Launcher", $"游戏根目录：{InstanceDirectory}");
    }
}
