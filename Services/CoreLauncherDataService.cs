using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CmlLib.Core;
using CmlLib.Core.VersionMetadata;
using WINUI.Models;

namespace WINUI.Services;

/// <summary>
/// 接入 CMLLib.Core 的真实数据服务（大更新 ⑦-1）。
/// <para>
/// 当前仅 <see cref="GetVersionsAsync"/> 走真实版本清单（官方 manifest v2）；
/// 其余查询继续委托 <see cref="MockLauncherDataService"/>，
/// 随大更新 ⑦-2~⑦-7 逐项真实化。接缝保持 <see cref="ILauncherDataService"/> 不变，
/// 页面与视图模型无需改动。
/// </para>
/// </summary>
public sealed class CoreLauncherDataService : ILauncherDataService
{
    private readonly MockLauncherDataService _mock;
    private readonly MinecraftPath _path;
    private readonly MinecraftLauncher _launcher;

    public CoreLauncherDataService(MockLauncherDataService mock)
    {
        _mock = mock;

        // 默认指向官方 %appdata%\.minecraft；⑦-2 引入实例目录后替换为启动器自有目录。
        _path = new MinecraftPath();
        _launcher = new MinecraftLauncher(_path);
    }

    /// <summary>
    /// 真实版本清单：官方 manifest v2（含本地已安装版本），映射为 <see cref="GameVersion"/>。
    /// </summary>
    public async Task<IReadOnlyList<GameVersion>> GetVersionsAsync(CancellationToken cancellationToken = default)
    {
        var metadata = await _launcher.GetAllVersionsAsync(cancellationToken);

        var result = new List<GameVersion>();
        foreach (var item in metadata)
        {
            result.Add(new GameVersion
            {
                Id = item.Name,
                DisplayName = item.Name,
                Channel = MapChannel(item.GetVersionType()),
                Loader = "Vanilla",
                ReleasedAt = DateOnly.FromDateTime(item.ReleaseTime.DateTime),
                IsInstalled = IsInstalledLocally(item.Name),
            });
        }

        return result;
    }

    /// <summary>版本类型映射：Release / Snapshot 之外（Old、OldBeta、OldAlpha 等）归为远古版本。</summary>
    internal static VersionChannel MapChannel(MVersionType type) => type switch
    {
        MVersionType.Release => VersionChannel.Release,
        MVersionType.Snapshot => VersionChannel.Snapshot,
        _ => VersionChannel.Legacy,
    };

    /// <summary>本地是否已安装：versions 目录下存在同名版本 json。</summary>
    private bool IsInstalledLocally(string versionName)
    {
        var versionsDir = _path.Versions;
        if (string.IsNullOrEmpty(versionsDir))
        {
            return false;
        }

        var jsonPath = Path.Combine(versionsDir, versionName, versionName + ".json");
        return File.Exists(jsonPath);
    }

    // ==================== 以下查询暂委托 Mock（随 ⑦-2~⑦-7 真实化） ====================

    /// <inheritdoc />
    public Task<IReadOnlyList<GameInstance>> GetInstancesAsync(CancellationToken cancellationToken = default)
        => _mock.GetInstancesAsync(cancellationToken);

    /// <inheritdoc />
    public Task<IReadOnlyList<DownloadItem>> GetDownloadItemsAsync(CancellationToken cancellationToken = default)
        => _mock.GetDownloadItemsAsync(cancellationToken);

    /// <inheritdoc />
    public Task<IReadOnlyList<LoaderEntry>> GetLoadersAsync(string gameVersion, CancellationToken cancellationToken = default)
        => _mock.GetLoadersAsync(gameVersion, cancellationToken);

    /// <inheritdoc />
    public Task<IReadOnlyList<ModEntry>> GetModsAsync(string instanceId, CancellationToken cancellationToken = default)
        => _mock.GetModsAsync(instanceId, cancellationToken);

    /// <inheritdoc />
    public Task<IReadOnlyList<NewsItem>> GetNewsAsync(CancellationToken cancellationToken = default)
        => _mock.GetNewsAsync(cancellationToken);

    /// <inheritdoc />
    public Task<PlayerAccount> GetCurrentAccountAsync(CancellationToken cancellationToken = default)
        => _mock.GetCurrentAccountAsync(cancellationToken);

    /// <inheritdoc />
    public Task<IReadOnlyList<OfflineAccount>> GetOfflineAccountsAsync(CancellationToken cancellationToken = default)
        => _mock.GetOfflineAccountsAsync(cancellationToken);

    /// <inheritdoc />
    public Task<IReadOnlyList<LogEntry>> GetLogEntriesAsync(CancellationToken cancellationToken = default)
        => _mock.GetLogEntriesAsync(cancellationToken);
}
