using System;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using WINUI.Models;

namespace WINUI.Services;

/// <inheritdoc cref="ILoaderInstallerService" />
/// <remarks>
/// Fabric / Quilt 的安装方式与官方启动器一致：
/// <list type="number">
/// <item><description>取 <c>profile/json</c>（含 <c>inheritsFrom</c> 基础版本、加载器库清单与主类）；</description></item>
/// <item><description>原样写入 <c>{游戏目录}/versions/{版本id}/{版本id}.json</c>；</description></item>
/// <item><description>调用 CMLLib 安装该自定义版本（解析继承链并下载全部库）。</description></item>
/// </list>
/// 这样启动流程无需任何特殊处理：加载器版本与原版版本在启动器里是同一种东西。
/// </remarks>
public sealed class LoaderInstallerService : ILoaderInstallerService
{
    private const string FabricProfile = "https://meta.fabricmc.net/v2/versions/loader";
    private const string QuiltProfile = "https://meta.quiltmc.org/v3/versions/loader";

    private static readonly HttpClient Http = new() { Timeout = TimeSpan.FromSeconds(30) };

    private readonly IGameLauncherService _game;

    public LoaderInstallerService(IGameLauncherService game) => _game = game;

    /// <inheritdoc />
    public bool CanInstall(ModLoader loader) => loader is ModLoader.Fabric or ModLoader.Quilt;

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
            throw new NotSupportedException($"{loader} 的自动安装将在后续版本支持。");
        }

        // 1) 先确保原版可用（加载器版本的父版本）。
        progress?.Report(new DownloadProgress(0, 0));
        await _game.InstallAsync(gameVersion, progress, cancellationToken);

        // 2) 取加载器 profile 元数据。
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

        // 3) 解析版本 id（官方 profile 自带 id；缺失时按官方命名约定兜底）。
        var versionId = ReadVersionId(json)
            ?? $"{Prefix(loader)}-{loaderVersion}-{gameVersion}";

        var versionsRoot = _game.GamePath.Versions
            ?? throw new InvalidOperationException("游戏目录未初始化，无法写入版本文件。");

        var versionDirectory = Path.Combine(versionsRoot, versionId);
        Directory.CreateDirectory(versionDirectory);

        var versionJsonPath = Path.Combine(versionDirectory, versionId + ".json");
        await File.WriteAllTextAsync(versionJsonPath, json, cancellationToken);

        // 4) 自检：确认 CMLLib 能解析刚写入的自定义版本（解析失败时给出可诊断的错误，
        //    而不是等到下载阶段报出难以理解的异常）。
        var versionMetadata = await _game.Launcher.GetVersionAsync(versionId, cancellationToken);
        if (versionMetadata is null)
        {
            throw new InvalidOperationException(
                $"版本文件已写入（{versionJsonPath}），但启动核心未能解析该版本（{versionId}）。");
        }

        // 5) 交给 CMLLib 下载该版本的全部文件（继承原版 + 加载器库）。
        await _game.InstallAsync(versionId, progress, cancellationToken);

        return versionId;
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
}
