using System;
using System.Threading;
using System.Threading.Tasks;
using WINUI.Models;

namespace WINUI.Services;

/// <summary>
/// 模组加载器安装服务（大更新 ⑩）。
/// <para>
/// 本轮支持 Fabric 与 Quilt：取官方 profile 元数据 → 写入启动器自有游戏目录的
/// 版本 json → 交由 CMLLib 下载该版本所需的全部库。
/// 返回生成的版本标识，可直接用于启动。
/// </para>
/// </summary>
public interface ILoaderInstallerService
{
    /// <summary>该加载器是否已支持自动安装。</summary>
    bool CanInstall(ModLoader loader);

    /// <summary>
    /// 安装「游戏版本 + 加载器」组合。必要时先补齐原版，再注入加载器并下载其库。
    /// </summary>
    /// <param name="gameVersion">基础游戏版本（如 <c>1.21.4</c>）。</param>
    /// <param name="loader">加载器种类（目前支持 Fabric / Quilt）。</param>
    /// <param name="loaderVersion">加载器版本（如 <c>0.16.9</c>）。</param>
    /// <param name="progress">可选的下载进度接收器。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>安装完成后可直接启动的版本标识（如 <c>fabric-loader-0.16.9-1.21.4</c>）。</returns>
    Task<string> InstallAsync(
        string gameVersion,
        ModLoader loader,
        string loaderVersion,
        IProgress<DownloadProgress>? progress = null,
        CancellationToken cancellationToken = default);
}
