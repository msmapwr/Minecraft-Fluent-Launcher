using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CmlLib.Core;
using CmlLib.Core.FileExtractors;
using WINUI.Models;

namespace WINUI.Services;

/// <inheritdoc cref="IGameLauncherService" />
public sealed class GameLauncherService : IGameLauncherService
{
    /// <summary>BMCLAPI 镜像根地址。</summary>
    private const string BmclapiBase = "https://bmclapi2.bangbang93.com";

    private readonly ISettingsService _settings;
    private readonly MinecraftPath _path;

    /// <summary>
    /// 当前安装 / 启动流程的进度接收器（大更新 ⑨-2）。
    /// 队列与启动页都是串行使用，同一时刻至多一个流程，因此用单槽即可。
    /// </summary>
    private IProgress<DownloadProgress>? _activeProgress;

    public GameLauncherService(ISettingsService settings)
    {
        _settings = settings;

        // 启动器自有游戏目录，独立于官方 .minecraft，避免污染用户已有安装。
        var root = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "MinecraftFluentLauncher",
            "game");

        _path = new MinecraftPath(root);

        ApplyDownloadSource(_settings.Settings.DownloadSource);
    }

    /// <inheritdoc />
    public MinecraftPath GamePath => _path;

    /// <inheritdoc />
    public MinecraftLauncher Launcher { get; private set; } = new();

    /// <inheritdoc />
    public void ApplyDownloadSource(DownloadSource source)
    {
        var parameters = MinecraftLauncherParameters.CreateDefault(_path, new HttpClient());

        // 官方源与未实现的社区镜像走 Mojang 默认；BMCLAPI 替换资源与库下载服务器。
        if (source == DownloadSource.Bmclapi)
        {
            var extractors = DefaultFileExtractors.CreateDefault(
                parameters.HttpClient,
                parameters.RulesEvaluator!,
                parameters.JavaPathResolver!);

            if (extractors.Asset is not null)
            {
                extractors.Asset.AssetServer = $"{BmclapiBase}/assets";
            }

            if (extractors.Library is not null)
            {
                extractors.Library.LibraryServer = $"{BmclapiBase}/maven";
            }
            parameters.FileExtractors = extractors.ToExtractorCollection();
        }

        Launcher = new MinecraftLauncher(parameters);

        // 真实下载进度（每秒 3–4 次）：转发给当前流程的接收器。
        // 事件在后台线程触发；接收器实现负责把数据安全地送达界面。
        Launcher.ByteProgressChanged += (_, e) =>
            _activeProgress?.Report(new DownloadProgress(e.ProgressedBytes, e.TotalBytes));
    }

    /// <inheritdoc />
    public async Task InstallAsync(
        string versionId,
        IProgress<DownloadProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        if (IsInstalledLocally(versionId))
        {
            // 已安装：无需下载，直接上报完成态。
            progress?.Report(new DownloadProgress(0, 0));
            return;
        }

        await RunWithProgressAsync(
            progress,
            async () => await Launcher.InstallAsync(versionId, cancellationToken));
    }

    /// <summary>把进度接收器接到本次流程上，结束后恢复前值。</summary>
    private async Task RunWithProgressAsync(IProgress<DownloadProgress>? progress, Func<Task> action)
    {
        var previous = _activeProgress;
        _activeProgress = progress;

        try
        {
            await action();
        }
        finally
        {
            _activeProgress = previous;
        }
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<string>> GetInstalledVersionIdsAsync(CancellationToken cancellationToken = default)
    {
        var versionsDir = _path.Versions;
        var result = new List<string>();

        if (!string.IsNullOrEmpty(versionsDir) && Directory.Exists(versionsDir))
        {
            foreach (var dir in Directory.EnumerateDirectories(versionsDir))
            {
                var name = Path.GetFileName(dir);
                if (!string.IsNullOrEmpty(name) && File.Exists(Path.Combine(dir, name + ".json")))
                {
                    result.Add(name);
                }
            }
        }

        return Task.FromResult<IReadOnlyList<string>>(result);
    }

    /// <inheritdoc />
    public bool IsInstalledLocally(string versionId)
    {
        var versionsDir = _path.Versions;
        if (string.IsNullOrEmpty(versionsDir))
        {
            return false;
        }

        return File.Exists(Path.Combine(versionsDir, versionId, versionId + ".json"));
    }

    /// <inheritdoc />
    public async Task<System.Diagnostics.Process> LaunchVanillaAsync(
        string versionId,
        string playerName,
        string? javaPath,
        int maxRamMb,
        IProgress<DownloadProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var option = new CmlLib.Core.ProcessBuilder.MLaunchOption
        {
            Session = CmlLib.Core.Auth.MSession.CreateOfflineSession(playerName),
            MaximumRamMb = maxRamMb,
        };

        if (!string.IsNullOrWhiteSpace(javaPath))
        {
            option.JavaPath = javaPath;
        }

        // 版本未安装时先下载（InstallAsync 对已安装版本幂等）。
        await InstallAsync(versionId, progress, cancellationToken);

        // 构建启动参数（含 classpath / 资源索引 / JVM 参数），全部异步完成后再起进程。
        var process = await Launcher.BuildProcessAsync(versionId, option);
        process.Start();
        return process;
    }
}
