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
                parameters.RulesEvaluator,
                parameters.JavaPathResolver);

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
    }

    /// <inheritdoc />
    public async Task InstallAsync(string versionId, CancellationToken cancellationToken = default)
    {
        if (IsInstalledLocally(versionId))
        {
            return;
        }

        await Launcher.InstallAsync(versionId, cancellationToken);
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
}
