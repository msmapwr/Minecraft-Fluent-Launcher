using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CmlLib.Core;
using WINUI.Models;
using WINUI.Services;

namespace WINUI.Tests.Services;

/// <summary>
/// <see cref="IGameLauncherService"/> 的测试替身：记录安装调用，行为可注入。
/// </summary>
internal sealed class FakeGameLauncherService : IGameLauncherService
{
    public int InstallCalls;

    /// <summary>InstallAsync 的行为（默认立即完成）。</summary>
    public Func<string, CancellationToken, Task> InstallBehavior { get; set; } = (_, _) => Task.CompletedTask;

    public MinecraftPath GamePath { get; } = new(Path.Combine(Path.GetTempPath(), "mfl-tests"));

    public MinecraftLauncher Launcher => throw new NotSupportedException("测试中不使用真实启动器");

    public void ApplyDownloadSource(DownloadSource source)
    {
    }

    public Task InstallAsync(string versionId, IProgress<DownloadProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        Interlocked.Increment(ref InstallCalls);

        // 上报一次进度（若调用方关心）：模拟 1 MB / 1 MB。
        progress?.Report(new DownloadProgress(1_000_000, 1_000_000));

        return InstallBehavior(versionId, cancellationToken);
    }

    public Task<System.Collections.Generic.IReadOnlyList<string>> GetInstalledVersionIdsAsync(CancellationToken cancellationToken = default)
        => Task.FromResult<System.Collections.Generic.IReadOnlyList<string>>([]);

    public bool IsInstalledLocally(string versionId) => false;

    public Task<Process> LaunchVanillaAsync(
        string versionId,
        string playerName,
        string? javaPath,
        int maxRamMb,
        IProgress<DownloadProgress>? progress = null,
        CancellationToken cancellationToken = default)
        => throw new NotSupportedException("测试中不启动游戏进程");
}
