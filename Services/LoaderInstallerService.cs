using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using WINUI.Models;

namespace WINUI.Services;

/// <inheritdoc cref="ILoaderInstallerService" />
/// <remarks>
/// 两条真实安装路径（均以官方产物为准，不依赖第三方封装）：
/// <list type="bullet">
/// <item><description>
/// <b>Fabric / Quilt</b>：取 <c>profile/json</c>（含 <c>inheritsFrom</c>、加载器库清单与主类），
/// 原样写入 <c>{游戏目录}/versions/{版本id}/{版本id}.json</c>，再交给 CMLLib 下载继承链上的全部文件。
/// </description></item>
/// <item><description>
/// <b>Forge / NeoForge</b>：下载官方 installer jar 并以 <c>--installClient</c> 调用它
/// （与 HMCL 等启动器同一做法），由官方 installer 生成版本目录；随后交由 CMLLib 补齐缺失文件。
/// </description></item>
/// </list>
/// 无论哪条路径，产物都是「versions 目录下的一个版本」，启动流程无需任何特殊处理。
/// </remarks>
public sealed class LoaderInstallerService : ILoaderInstallerService
{
    private const string FabricProfile = "https://meta.fabricmc.net/v2/versions/loader";
    private const string QuiltProfile = "https://meta.quiltmc.org/v3/versions/loader";

    /// <summary>Forge 官方 maven（installer jar 经 BMCLAPI 镜像加速）。</summary>
    private const string ForgeMaven = "https://bmclapi2.bangbang93.com/maven/net/minecraftforge/forge";

    /// <summary>NeoForge 官方 maven。</summary>
    private const string NeoForgeMaven = "https://maven.neoforged.net/releases/net/neoforged/neoforge";

    private static readonly HttpClient Http = new() { Timeout = TimeSpan.FromMinutes(10) };

    private readonly IGameLauncherService _game;
    private readonly IJavaLocatorService _javaLocator;
    private readonly ISettingsService _settings;

    public LoaderInstallerService(
        IGameLauncherService game,
        IJavaLocatorService javaLocator,
        ISettingsService settings)
    {
        _game = game;
        _javaLocator = javaLocator;
        _settings = settings;
    }

    /// <inheritdoc />
    public bool CanInstall(ModLoader loader) => loader is ModLoader.Fabric
        or ModLoader.Quilt
        or ModLoader.Forge
        or ModLoader.NeoForge;

    /// <inheritdoc />
    public async Task<string> InstallAsync(
        string gameVersion,
        ModLoader loader,
        string loaderVersion,
        IProgress<DownloadProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        if (!CanInstall(loader))
        {
            throw new NotSupportedException($"{loader} 的自动安装暂不支持。");
        }

        progress?.Report(new DownloadProgress(0, 0));

        // 两条路径都需要原版先就位（加载器版本的父版本 / installer 的输入）。
        await _game.InstallAsync(gameVersion, progress, cancellationToken);

        var versionId = loader switch
        {
            ModLoader.Fabric or ModLoader.Quilt => await InstallProfileAsync(
                gameVersion, loader, loaderVersion, cancellationToken),
            _ => await RunOfficialInstallerAsync(
                gameVersion, loader, loaderVersion, cancellationToken),
        };

        // 自检：确认 CMLLib 能解析生成/写入的版本。
        var metadata = await _game.Launcher.GetVersionAsync(versionId, cancellationToken);
        if (metadata is null)
        {
            throw new InvalidOperationException($"启动核心未能解析版本 {versionId}，安装结果不可用。");
        }

        // 补齐 installer 未覆盖的文件（幂等）。
        await _game.InstallAsync(versionId, progress, cancellationToken);

        return versionId;
    }

    // ==================== Fabric / Quilt：profile 注入 ====================

    /// <summary>取官方 profile 元数据并写入本地版本目录，返回版本 id。</summary>
    private async Task<string> InstallProfileAsync(
        string gameVersion,
        ModLoader loader,
        string loaderVersion,
        CancellationToken cancellationToken)
    {
        var baseUrl = loader == ModLoader.Fabric ? FabricProfile : QuiltProfile;
        var profileUrl = $"{baseUrl}/{gameVersion}/{loaderVersion}/profile/json";

        string json;
        try
        {
            json = await Http.GetStringAsync(profileUrl, cancellationToken);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            throw new InvalidOperationException(
                $"无法获取 {loader} {loaderVersion}（{gameVersion}）的元数据：{ex.Message}", ex);
        }

        var versionId = ReadVersionId(json)
            ?? $"{Prefix(loader)}-{loaderVersion}-{gameVersion}";

        var versionDirectory = EnsureVersionDirectory(versionId);
        var versionJsonPath = Path.Combine(versionDirectory, versionId + ".json");

        await File.WriteAllTextAsync(versionJsonPath, json, cancellationToken);

        return versionId;
    }

    // ==================== Forge / NeoForge：官方 installer ====================

    /// <summary>
    /// 下载官方 installer 并执行 <c>--installClient</c>，返回生成的版本 id。
    /// installer 自身负责解压/校验/写版本文件，这里只负责准备与结果核验。
    /// </summary>
    private async Task<string> RunOfficialInstallerAsync(
        string gameVersion,
        ModLoader loader,
        string loaderVersion,
        CancellationToken cancellationToken)
    {
        var installerUrl = BuildInstallerUrl(gameVersion, loader, loaderVersion);
        var installerPath = Path.Combine(Path.GetTempPath(), $"mfl-installer-{Guid.NewGuid():N}.jar");

        var javaPath = ResolveJavaPath()
            ?? throw new InvalidOperationException(
                "安装 Forge / NeoForge 需要 Java 运行时，但未检测到。请在设置页指定 javaw.exe 路径。");

        try
        {
            await DownloadInstallerAsync(installerUrl, installerPath, cancellationToken);

            var gameDirectory = _game.GamePath.BasePath
                ?? throw new InvalidOperationException("游戏目录未初始化。");

            var (exitCode, output) = await ExecuteInstallerAsync(
                javaPath, installerPath, gameDirectory, cancellationToken);

            if (exitCode != 0)
            {
                throw new InvalidOperationException(
                    $"{loader} installer 执行失败（退出码 {exitCode}）：{Tail(output)}");
            }
        }
        finally
        {
            TryDelete(installerPath);
        }

        return ResolveInstalledVersionId(gameVersion, loader, loaderVersion);
    }

    /// <summary>installer jar 的下载地址（Forge 走 BMCLAPI 镜像，NeoForge 走官方 maven）。</summary>
    internal static string BuildInstallerUrl(string gameVersion, ModLoader loader, string loaderVersion) => loader switch
    {
        ModLoader.Forge =>
            $"{ForgeMaven}/{gameVersion}-{loaderVersion}/forge-{gameVersion}-{loaderVersion}-installer.jar",
        _ =>
            $"{NeoForgeMaven}/{loaderVersion}/neoforge-{loaderVersion}-installer.jar",
    };

    private async Task DownloadInstallerAsync(string url, string destination, CancellationToken cancellationToken)
    {
        using var response = await Http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"下载安装器失败（HTTP {(int)response.StatusCode}）：{url}");
        }

        await using var target = File.Create(destination);
        await response.Content.CopyToAsync(target, cancellationToken);
    }

    /// <summary>执行 installer 并等待结束（输出用于失败诊断）。</summary>
    private static async Task<(int ExitCode, string Output)> ExecuteInstallerAsync(
        string javaPath,
        string installerPath,
        string gameDirectory,
        CancellationToken cancellationToken)
    {
        var startInfo = new ProcessStartInfo(javaPath, $"-jar \"{installerPath}\" --installClient \"{gameDirectory}\"")
        {
            WorkingDirectory = gameDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        using var process = Process.Start(startInfo)
            ?? throw new InvalidOperationException("无法启动 installer 进程。");

        // 必须异步读取：否则输出缓冲写满会让 installer 阻塞。
        var standardOutput = process.StandardOutput.ReadToEndAsync(cancellationToken);
        var standardError = process.StandardError.ReadToEndAsync(cancellationToken);

        await process.WaitForExitAsync(cancellationToken);

        var output = (await standardOutput) + (await standardError);
        return (process.ExitCode, output);
    }

    /// <summary>定位 installer 生成的版本 id（优先官方命名约定，其次在游戏目录中反查）。</summary>
    private string ResolveInstalledVersionId(string gameVersion, ModLoader loader, string loaderVersion)
    {
        var versionsRoot = _game.GamePath.Versions
            ?? throw new InvalidOperationException("游戏目录未初始化。");

        var expected = loader == ModLoader.Forge
            ? $"{gameVersion}-forge-{loaderVersion}"
            : $"neoforge-{loaderVersion}";

        if (File.Exists(Path.Combine(versionsRoot, expected, expected + ".json")))
        {
            return expected;
        }

        // 兜底：某些 installer 版本会在 id 上附加后缀（如 -beta），按加载器版本号反查。
        var match = Directory.EnumerateDirectories(versionsRoot)
            .Select(Path.GetFileName)
            .FirstOrDefault(name => name is not null
                                    && name.Contains(loaderVersion, StringComparison.OrdinalIgnoreCase)
                                    && File.Exists(Path.Combine(versionsRoot, name, name + ".json")));

        return match
               ?? throw new InvalidOperationException(
                   $"installer 已执行，但未找到 {loader} {loaderVersion} 生成的版本目录（{versionsRoot}）。");
    }

    // ==================== 公共辅助 ====================

    /// <summary>解析可用的 Java（手动路径优先，其次自动检测）。</summary>
    private string? ResolveJavaPath()
    {
        var configured = _settings.Settings.JavaPath;
        if (!string.IsNullOrWhiteSpace(configured) && File.Exists(configured))
        {
            return configured;
        }

        var detected = _javaLocator.FindDefaultJavaPath();
        if (string.IsNullOrEmpty(detected))
        {
            return null;
        }

        // javaw.exe 无控制台输出，installer 需要 java.exe。
        var directory = Path.GetDirectoryName(detected);
        var javaExe = string.IsNullOrEmpty(directory) ? null : Path.Combine(directory, "java.exe");

        return javaExe is not null && File.Exists(javaExe) ? javaExe : detected;
    }

    private string EnsureVersionDirectory(string versionId)
    {
        var versionsRoot = _game.GamePath.Versions
            ?? throw new InvalidOperationException("游戏目录未初始化，无法写入版本文件。");

        var directory = Path.Combine(versionsRoot, versionId);
        Directory.CreateDirectory(directory);
        return directory;
    }

    /// <summary>读取 profile 里的 <c>id</c> 字段。</summary>
    internal static string? ReadVersionId(string json)
    {
        try
        {
            using var document = JsonDocument.Parse(json);

            return document.RootElement.TryGetProperty("id", out var id)
                   && id.GetString() is { Length: > 0 } value
                ? value
                : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static string Prefix(ModLoader loader) => loader switch
    {
        ModLoader.Fabric => "fabric-loader",
        ModLoader.Quilt => "quilt-loader",
        _ => "loader",
    };

    /// <summary>截取输出末尾用于错误信息（installer 输出很长）。</summary>
    private static string Tail(string output, int maxLength = 600)
    {
        var trimmed = output.Trim();

        return trimmed.Length <= maxLength
            ? trimmed
            : trimmed[^maxLength..];
    }

    private static void TryDelete(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // 临时文件清理失败不影响安装结果。
        }
    }
}
