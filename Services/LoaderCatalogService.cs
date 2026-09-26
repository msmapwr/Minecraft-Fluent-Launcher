using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using WINUI.Models;

namespace WINUI.Services;

/// <inheritdoc cref="ILoaderCatalogService" />
/// <remarks>
/// 数据来源（均为官方公开接口，无需凭据）：
/// <list type="bullet">
/// <item><description>Fabric：<c>https://meta.fabricmc.net/v2/versions/loader/{游戏版本}</c></description></item>
/// <item><description>Quilt：<c>https://meta.quiltmc.org/v3/versions/loader/{游戏版本}</c></description></item>
/// </list>
/// 两者返回结构一致：数组元素含 <c>loader.version</c> 与 <c>loader.stable</c>，按版本降序排列。
/// </remarks>
public sealed class LoaderCatalogService : ILoaderCatalogService
{
    private const string FabricMeta = "https://meta.fabricmc.net/v2/versions/loader";
    private const string QuiltMeta = "https://meta.quiltmc.org/v3/versions/loader";

    /// <summary>清单请求超时（清单很小，超时即视为该来源不可用）。</summary>
    private static readonly HttpClient Http = new() { Timeout = TimeSpan.FromSeconds(12) };

    /// <summary>每个加载器最多展示的可选版本数（避免下拉过长）。</summary>
    private const int MaxVersionsPerLoader = 12;

    /// <inheritdoc />
    public bool CanInstall(ModLoader loader) => loader is ModLoader.Fabric or ModLoader.Quilt;

    /// <inheritdoc />
    public async Task<IReadOnlyList<LoaderEntry>> GetLoadersAsync(
        string gameVersion,
        CancellationToken cancellationToken = default)
    {
        var result = new List<LoaderEntry>();

        var fabric = await TryReadVersionsAsync($"{FabricMeta}/{gameVersion}", cancellationToken);
        if (fabric.Count > 0)
        {
            result.Add(new LoaderEntry
            {
                Loader = ModLoader.Fabric,
                Versions = fabric,
                RecommendedVersion = fabric[0],
            });
        }

        var quilt = await TryReadVersionsAsync($"{QuiltMeta}/{gameVersion}", cancellationToken);
        if (quilt.Count > 0)
        {
            result.Add(new LoaderEntry
            {
                Loader = ModLoader.Quilt,
                Versions = quilt,
                RecommendedVersion = quilt[0],
            });
        }

        return result;
    }

    /// <summary>读取一个来源的版本列表；失败（网络 / 解析 / 无该版本）返回空列表。</summary>
    private static async Task<IReadOnlyList<string>> TryReadVersionsAsync(string url, CancellationToken cancellationToken)
    {
        var versions = new List<string>();

        try
        {
            var json = await Http.GetStringAsync(url, cancellationToken);

            using var document = JsonDocument.Parse(json);
            if (document.RootElement.ValueKind != JsonValueKind.Array)
            {
                return versions;
            }

            string? stableVersion = null;

            foreach (var element in document.RootElement.EnumerateArray())
            {
                if (!element.TryGetProperty("loader", out var loader)
                    || !loader.TryGetProperty("version", out var versionElement)
                    || versionElement.GetString() is not { Length: > 0 } version)
                {
                    continue;
                }

                // 接口按版本降序返回；优先记录第一个稳定版作为推荐版本。
                if (stableVersion is null
                    && loader.TryGetProperty("stable", out var stable)
                    && stable.ValueKind == JsonValueKind.True)
                {
                    stableVersion = version;
                }

                if (versions.Count < MaxVersionsPerLoader)
                {
                    versions.Add(version);
                }
            }

            // 把稳定版提到最前（作为推荐安装版本）。
            if (stableVersion is not null && versions.Remove(stableVersion))
            {
                versions.Insert(0, stableVersion);
            }
        }
        catch (Exception ex) when (ex is HttpRequestException
                                       or TaskCanceledException
                                       or JsonException
                                       or InvalidOperationException)
        {
            // 该来源不可用：返回空，交由调用方与其它来源合并。
        }

        return versions;
    }
}
