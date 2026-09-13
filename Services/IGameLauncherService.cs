using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CmlLib.Core;
using WINUI.Models;

namespace WINUI.Services;

/// <summary>
/// 真实启动核心（CMLLib）的门面：持有游戏根目录与下载源配置，
/// 提供「安装版本 / 枚举本地版本」能力（大更新 ⑦-2）。
/// </summary>
public interface IGameLauncherService
{
    /// <summary>游戏根目录（启动器自有，独立于官方 .minecraft）。</summary>
    MinecraftPath GamePath { get; }

    /// <summary>CMLLib 启动器实例（供清单服务与后续启动流程共用）。</summary>
    MinecraftLauncher Launcher { get; }

    /// <summary>按设置里的下载源配置镜像（官方 / BMCLAPI）。</summary>
    void ApplyDownloadSource(DownloadSource source);

    /// <summary>安装（下载）一个版本；已安装时立即返回。</summary>
    Task InstallAsync(string versionId, CancellationToken cancellationToken = default);

    /// <summary>枚举本地已安装的版本目录名。</summary>
    Task<IReadOnlyList<string>> GetInstalledVersionIdsAsync(CancellationToken cancellationToken = default);

    /// <summary>判断某个版本是否已安装到本地。</summary>
    bool IsInstalledLocally(string versionId);
}
