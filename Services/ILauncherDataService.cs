using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WINUI.Models;

namespace WINUI.Services;

/// <summary>
/// 启动器数据来源的抽象。
/// <para>
/// UI 阶段由 <see cref="MockLauncherDataService"/> 提供 Mock 数据；
/// 后续接入真实启动核心（版本清单 / 新闻接口 / 账户认证）时，
/// 只需替换实现并调整 <c>App.xaml.cs</c> 中的 DI 注册，页面与视图模型无需改动。
/// </para>
/// </summary>
public interface ILauncherDataService
{
    /// <summary>获取可启动的版本 / 实例列表。</summary>
    Task<IReadOnlyList<GameVersion>> GetVersionsAsync(CancellationToken cancellationToken = default);

    /// <summary>获取已创建的实例列表。</summary>
    Task<IReadOnlyList<GameInstance>> GetInstancesAsync(CancellationToken cancellationToken = default);

    /// <summary>获取下载中心的可下载条目。</summary>
    Task<IReadOnlyList<DownloadItem>> GetDownloadItemsAsync(CancellationToken cancellationToken = default);

    /// <summary>获取新闻与公告。</summary>
    Task<IReadOnlyList<NewsItem>> GetNewsAsync(CancellationToken cancellationToken = default);

    /// <summary>获取当前登录的账户。</summary>
    Task<PlayerAccount> GetCurrentAccountAsync(CancellationToken cancellationToken = default);
}
