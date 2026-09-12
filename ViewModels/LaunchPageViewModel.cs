using System.Collections.ObjectModel;
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
/// 数据来自 <see cref="ILauncherDataService"/>（当前为 Mock 实现）。
/// 「启动游戏」为纯 UI 演示：以进度条模拟校验 → 准备运行时 → 启动三个阶段，
/// <b>不会真正拉起任何 Java 进程</b>。
/// </para>
/// </summary>
public sealed partial class LaunchPageViewModel : ObservableObject
{
    private readonly ILauncherDataService _dataService;

    /// <summary>是否正在加载初始数据。</summary>
    [ObservableProperty]
    public partial bool IsLoading { get; set; }

    /// <summary>是否处于启动流程中。</summary>
    [ObservableProperty]
    public partial bool IsLaunching { get; set; }

    /// <summary>游戏是否正在运行（Mock）。</summary>
    [ObservableProperty]
    public partial bool IsGameRunning { get; set; }

    /// <summary>启动进度（0–100）。</summary>
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

    /// <summary>安装目录（Mock）。</summary>
    public string InstanceDirectory => @"%LOCALAPPDATA%\MinecraftFluentLauncher\instances\1.21.4-fabric";

    public LaunchPageViewModel(ILauncherDataService dataService)
    {
        _dataService = dataService;

        StatusMessage = "准备就绪";
        MemoryMb = 4096;

        // Mock 实现返回的是已完成的 Task，因此此处会同步跑完，页面绑定前数据即已就绪。
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

    /// <summary>启动 / 结束游戏（Mock）。</summary>
    [RelayCommand]
    private async Task PrimaryActionAsync()
    {
        if (IsGameRunning)
        {
            IsGameRunning = false;
            StatusMessage = "游戏已结束";
            LaunchProgress = 0;
            return;
        }

        if (IsLaunching)
        {
            return;
        }

        IsLaunching = true;
        LaunchProgress = 0;

        // Mock：模拟「校验文件 → 准备运行时 → 启动」三个阶段，共约 1.2 秒。
        for (var step = 1; step <= 10; step++)
        {
            await Task.Delay(120);
            LaunchProgress = step * 10;
            StatusMessage = step switch
            {
                <= 3 => "正在校验游戏文件…",
                <= 7 => $"正在准备 Java 运行时（{MemoryMb:0} MB 内存）…",
                _ => "正在启动游戏…",
            };
        }

        IsLaunching = false;
        IsGameRunning = true;
        StatusMessage = "游戏运行中";
    }

    /// <summary>打开实例目录（Mock）。</summary>
    [RelayCommand]
    private void OpenInstanceFolder()
    {
        StatusMessage = "（演示）将打开实例目录";
    }
}
