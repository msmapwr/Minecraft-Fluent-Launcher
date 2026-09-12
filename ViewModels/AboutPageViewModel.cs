using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WINUI.Models;
using WINUI.Services;

namespace WINUI.ViewModels;

/// <summary>
/// 关于页的视图模型。产品信息与运行环境均为真实取值，
/// 仅「打开项目主页 / 打开数据目录」因尚未配置仓库地址而保持演示行为。
/// </summary>
public sealed partial class AboutPageViewModel : ObservableObject
{
    private readonly IClipboardService _clipboardService;

    /// <summary>状态栏文案。</summary>
    [ObservableProperty]
    public partial string StatusMessage { get; set; }

    /// <summary>产品全名。</summary>
    public string DisplayName => AppInfo.DisplayName;

    /// <summary>产品简称。</summary>
    public string ShortName => AppInfo.ShortName;

    /// <summary>版本号。</summary>
    public string Version => AppInfo.Version;

    /// <summary>版本全称。</summary>
    public string FullVersion => AppInfo.FullVersion;

    /// <summary>发布阶段。</summary>
    public string ReleaseStage => AppInfo.ReleaseStage;

    /// <summary>版权声明。</summary>
    public string Copyright => AppInfo.Copyright;

    /// <summary>UI 框架。</summary>
    public string UiFramework => AppInfo.UiFramework;

    /// <summary>目标框架。</summary>
    public string TargetFramework => AppInfo.TargetFramework;

    /// <summary>架构与依赖。</summary>
    public string Architecture => AppInfo.Architecture;

    /// <summary>操作系统。</summary>
    public string OperatingSystem => AppInfo.OperatingSystem;

    /// <summary>.NET 运行时。</summary>
    public string DotNetRuntime => AppInfo.DotNetRuntime;

    /// <summary>进程架构。</summary>
    public string ProcessArchitecture => AppInfo.ProcessArchitecture;

    /// <summary>数据目录（真实路径）。</summary>
    public string DataDirectory { get; }

    /// <summary>产品简介。</summary>
    public string Description =>
        "基于 WinUI 3 构建的 Minecraft 启动器，目标是提供原生 Fluent 观感、清晰的实例管理与可扩展的启动流程。当前处于 UI 优先阶段，尚未接入真实的下载、认证与启动核心。";

    public AboutPageViewModel(ISettingsService settingsService, IClipboardService clipboardService)
    {
        _clipboardService = clipboardService;

        DataDirectory = settingsService.DataDirectory;
        StatusMessage = "准备就绪";
    }

    /// <summary>复制环境信息到剪贴板。</summary>
    [RelayCommand]
    private void CopyEnvironmentInfo()
    {
        var builder = new StringBuilder();
        builder.AppendLine($"{DisplayName} ({ShortName} {Version})");
        builder.AppendLine($"发布阶段：{ReleaseStage}");
        builder.AppendLine($"UI 框架：{UiFramework}");
        builder.AppendLine($"目标框架：{TargetFramework}");
        builder.AppendLine($"架构：{Architecture}");
        builder.AppendLine($"操作系统：{OperatingSystem}");
        builder.AppendLine($"运行时：{DotNetRuntime}");
        builder.AppendLine($"进程架构：{ProcessArchitecture}");
        builder.Append($"数据目录：{DataDirectory}");

        StatusMessage = _clipboardService.TrySetText(builder.ToString())
            ? "已复制环境信息到剪贴板"
            : "复制失败：剪贴板被其他程序占用，请重试";
    }

    /// <summary>打开数据目录（演示）。</summary>
    [RelayCommand]
    private void OpenDataDirectory()
        => StatusMessage = $"（演示）将打开：{DataDirectory}";

    /// <summary>打开项目主页（演示：仓库地址尚未配置）。</summary>
    [RelayCommand]
    private void OpenProjectPage()
        => StatusMessage = "尚未配置 Git 仓库地址，暂时无法打开项目主页";
}
