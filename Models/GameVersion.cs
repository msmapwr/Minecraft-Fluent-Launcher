using System;

namespace WINUI.Models;

/// <summary>
/// 一个可启动的游戏「实例」：版本号 + 加载器组合。
/// <para>当前为 UI 阶段的 Mock 数据模型，后续将由真实版本清单构建。</para>
/// </summary>
public sealed class GameVersion
{
    /// <summary>版本标识，如 <c>1.21.4</c>。</summary>
    public required string Id { get; init; }

    /// <summary>展示用版本名。</summary>
    public required string DisplayName { get; init; }

    /// <summary>发布通道。</summary>
    public required VersionChannel Channel { get; init; }

    /// <summary>加载器名称（Vanilla / Fabric / NeoForge …）。</summary>
    public required string Loader { get; init; }

    /// <summary>发布日期。</summary>
    public required DateOnly ReleasedAt { get; init; }

    /// <summary>是否已安装到本地。</summary>
    public bool IsInstalled { get; init; }

    /// <summary>下拉框展示名，如 <c>1.21.4 · Fabric 0.16.9</c>。</summary>
    public string FullName => $"{DisplayName} · {Loader}";

    /// <summary>发布通道的中文标签。</summary>
    public string ChannelLabel => Channel switch
    {
        VersionChannel.Release => "正式版",
        VersionChannel.Snapshot => "快照",
        _ => "远古版",
    };

    /// <summary>发布日期标签。</summary>
    public string ReleasedAtLabel => ReleasedAt.ToString("yyyy-MM-dd");

    /// <summary>安装状态标签。</summary>
    public string InstallStateLabel => IsInstalled ? "已安装" : "未安装";
}
