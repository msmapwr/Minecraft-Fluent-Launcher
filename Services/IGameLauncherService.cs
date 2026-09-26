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

    /// <summary>
    /// 安装（下载）一个版本；已安装时立即返回。
    /// </summary>
    /// <param name="versionId">版本标识（如 <c>1.21.4</c>）。</param>
    /// <param name="progress">
    /// 可选的进度接收器（大更新 ⑨-2）：由 CMLLib 的字节进度事件驱动，
    /// 每秒回调 3–4 次，用于显示真实百分比 / 速度 / 剩余。
    /// </param>
    /// <param name="cancellationToken">取消令牌。</param>
    Task InstallAsync(
        string versionId,
        IProgress<DownloadProgress>? progress = null,
        CancellationToken cancellationToken = default);

    /// <summary>枚举本地已安装的版本目录名。</summary>
    Task<IReadOnlyList<string>> GetInstalledVersionIdsAsync(CancellationToken cancellationToken = default);

    /// <summary>判断某个版本是否已安装到本地。</summary>
    bool IsInstalledLocally(string versionId);

    /// <summary>
    /// 以离线会话启动一个版本（v0.2 / ⑦-4；⑨-3 改为全异步）：
    /// 必要时先自动安装（含进度回调），再构造并启动 Java 进程。
    /// 返回<b>已启动</b>的进程（输出由调用方接线日志）。
    /// </summary>
    /// <param name="versionId">版本标识（如 <c>1.21.4</c>）。</param>
    /// <param name="playerName">离线玩家名。</param>
    /// <param name="javaPath">javaw.exe 路径；<c>null</c> 时由 CMLLib 自动解析。</param>
    /// <param name="maxRamMb">最大内存（MB）。</param>
    /// <param name="progress">可选的下载进度接收器（版本未安装时有效）。</param>
    /// <param name="cancellationToken">取消令牌（同时作用于安装与构建阶段）。</param>
    Task<System.Diagnostics.Process> LaunchVanillaAsync(
        string versionId,
        string playerName,
        string? javaPath,
        int maxRamMb,
        IProgress<DownloadProgress>? progress = null,
        CancellationToken cancellationToken = default);
}
