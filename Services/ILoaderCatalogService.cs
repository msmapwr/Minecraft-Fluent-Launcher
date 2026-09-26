using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WINUI.Models;

namespace WINUI.Services;

/// <summary>
/// 加载器元数据服务（大更新 ⑩）：提供各模组加载器的<b>真实可选版本</b>。
/// <para>
/// 本轮接入 Fabric 与 Quilt（Fabric Meta / Quilt Meta 的版本清单）；
/// Forge 与 NeoForge 的清单仍由既有数据源提供，其自动安装随后续版本接入。
/// </para>
/// </summary>
public interface ILoaderCatalogService
{
    /// <summary>
    /// 读取某个游戏版本可用的加载器清单（含各自可选版本）。
    /// 单个来源失败不影响其它来源（网络异常时返回空集合）。
    /// </summary>
    Task<IReadOnlyList<LoaderEntry>> GetLoadersAsync(string gameVersion, CancellationToken cancellationToken = default);

    /// <summary>该加载器是否已支持自动安装。</summary>
    bool CanInstall(ModLoader loader);
}
