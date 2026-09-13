using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CmlLib.Core.VersionMetadata;
using WINUI.Models;

namespace WINUI.Services;

/// <summary>
/// 接入 CMLLib.Core 的真实数据服务（大更新 ⑦-1 / ⑦-2）。
/// <para>
/// 已真实化：版本清单（官方 manifest v2）、本地实例扫描（启动器自有游戏目录）；
/// 其余查询继续委托 <see cref="MockLauncherDataService"/>，随 ⑦-5~⑦-7 逐项真实化。
/// 接缝保持 <see cref="ILauncherDataService"/> 不变，页面与视图模型无需改动。
/// </para>
/// </summary>
public sealed class CoreLauncherDataService : ILauncherDataService
{
    private readonly MockLauncherDataService _mock;
    private readonly IGameLauncherService _game;

    public CoreLauncherDataService(IGameLauncherService gameLauncher, MockLauncherDataService mock)
    {
        _game = gameLauncher;
        _mock = mock;
    }

    /// <summary>
    /// 真实版本清单：官方 manifest v2（含本地已安装版本），映射为 <see cref="GameVersion"/>。
    /// </summary>
    public async Task<IReadOnlyList<GameVersion>> GetVersionsAsync(CancellationToken cancellationToken = default)
    {
        var metadata = await _game.Launcher.GetAllVersionsAsync(cancellationToken);

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
                IsInstalled = _game.IsInstalledLocally(item.Name),
            });
        }

        return result;
    }

    /// <summary>
    /// 真实实例列表：扫描启动器自有游戏目录下已安装的版本。
    /// </summary>
    public async Task<IReadOnlyList<GameInstance>> GetInstancesAsync(CancellationToken cancellationToken = default)
    {
        var installed = await _game.GetInstalledVersionIdsAsync(cancellationToken);

        var result = new List<GameInstance>();
        foreach (var id in installed)
        {
            result.Add(new GameInstance
            {
                Id = id,
                Name = id,
                GameVersion = id,
                Loader = "Vanilla",
                Channel = GuessChannel(id),
                LastPlayedAt = null,
                PlayTime = TimeSpan.Zero,
                SizeGb = Math.Round(GetDirectorySizeGb(id), 2),
                IsInstalled = true,
            });
        }

        return result;
    }

    /// <summary>版本类型映射：Release / Snapshot 之外（OldBeta、OldAlpha 等）归为远古版本。</summary>
    internal static VersionChannel MapChannel(MVersionType type) => type switch
    {
        MVersionType.Release => VersionChannel.Release,
        MVersionType.Snapshot => VersionChannel.Snapshot,
        _ => VersionChannel.Legacy,
    };

    /// <summary>
    /// 从版本目录名猜测发布通道：可解析出 <c>主版本.次版本</c> 视为正式版，
    /// a/b 开头视为远古版本，其余（如 <c>24w14a</c>）视为快照。
    /// </summary>
    internal static VersionChannel GuessChannel(string versionId)
    {
        if (versionId.StartsWith("a", StringComparison.OrdinalIgnoreCase)
            || versionId.StartsWith("b", StringComparison.OrdinalIgnoreCase))
        {
            return VersionChannel.Legacy;
        }

        var parts = versionId.Split('.');
        if (parts.Length >= 2
            && !versionId.Contains('-')
            && int.TryParse(parts[0], out _)
            && int.TryParse(parts[1], out _))
        {
            return VersionChannel.Release;
        }

        return VersionChannel.Snapshot;
    }

    /// <summary>计算某个版本的目录占用（GB）。</summary>
    private double GetDirectorySizeGb(string versionId)
    {
        var dir = Path.Combine(_game.GamePath.Versions ?? string.Empty, versionId);
        if (!Directory.Exists(dir))
        {
            return 0;
        }

        long bytes = 0;
        foreach (var file in Directory.EnumerateFiles(dir, "*", SearchOption.AllDirectories))
        {
            try
            {
                bytes += new FileInfo(file).Length;
            }
            catch (IOException)
            {
                // 文件被占用等：跳过该文件，不阻断统计。
            }
        }

        return bytes / 1_000_000_000.0;
    }

    // ==================== 以下查询暂委托 Mock（随 ⑦-5~⑦-7 真实化） ====================

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
