using System;
using System.Runtime.InteropServices;

namespace WINUI.Models;

/// <summary>
/// 应用元信息。集中定义产品名、版本与运行环境描述，避免在界面上散落字面量。
/// </summary>
public static class AppInfo
{
    /// <summary>产品全名。</summary>
    public const string DisplayName = "Minecraft Fluent Launcher";

    /// <summary>产品简称。</summary>
    public const string ShortName = "MFL";

    /// <summary>版本号。发布时随 CHANGELOG 一同递增。</summary>
    public const string Version = "0.1.0";

    /// <summary>发布阶段说明。</summary>
    public const string ReleaseStage = "开发预览（尚未发布）";

    /// <summary>版权声明。</summary>
    public const string Copyright = "© 2026 Msmapwr";

    /// <summary>UI 框架。</summary>
    public const string UiFramework = "WinUI 3 / Windows App SDK 2.4.0";

    /// <summary>目标框架。</summary>
    public const string TargetFramework = ".NET 8 (net8.0-windows10.0.19041.0)";

    /// <summary>架构模式与依赖。</summary>
    public const string Architecture = "MVVM + 依赖注入（CommunityToolkit.Mvvm / Microsoft.Extensions.DependencyInjection）";

    /// <summary>版本全称，如 <c>MFL 0.1.0</c>。</summary>
    public static string FullVersion => $"{ShortName} {Version}";

    /// <summary>操作系统版本描述。</summary>
    public static string OperatingSystem => Environment.OSVersion.VersionString;

    /// <summary>.NET 运行时描述。</summary>
    public static string DotNetRuntime => RuntimeInformation.FrameworkDescription;

    /// <summary>进程架构。</summary>
    public static string ProcessArchitecture => RuntimeInformation.ProcessArchitecture.ToString();
}
