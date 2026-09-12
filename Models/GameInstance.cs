using System;

namespace WINUI.Models;

/// <summary>
/// 一个游戏实例（版本 + 加载器 + 独立存档/模组目录）。
/// <para>当前为 UI 阶段的 Mock 数据模型。</para>
/// </summary>
public sealed class GameInstance
{
    /// <summary>实例标识（目录名）。</summary>
    public required string Id { get; init; }

    /// <summary>实例名称。</summary>
    public required string Name { get; init; }

    /// <summary>Minecraft 版本号。</summary>
    public required string GameVersion { get; init; }

    /// <summary>加载器名称（Vanilla / Fabric / NeoForge …）。</summary>
    public required string Loader { get; init; }

    /// <summary>发布通道。</summary>
    public required VersionChannel Channel { get; init; }

    /// <summary>最近一次游玩时间；<c>null</c> 表示从未启动。</summary>
    public DateTimeOffset? LastPlayedAt { get; init; }

    /// <summary>累计游玩时长。</summary>
    public required TimeSpan PlayTime { get; init; }

    /// <summary>占用空间（GB）。</summary>
    public required double SizeGb { get; init; }

    /// <summary>是否已安装。</summary>
    public bool IsInstalled { get; init; }

    /// <summary>版本 + 加载器摘要，如 <c>1.21.4 · Fabric 0.16.9</c>。</summary>
    public string VersionSummary => $"{GameVersion} · {Loader}";

    /// <summary>发布通道标签。</summary>
    public string ChannelLabel => Channel.ToLabel();

    /// <summary>最近游玩时间标签。</summary>
    public string LastPlayedLabel => LastPlayedAt is null
        ? "从未启动"
        : $"最近游玩 {LastPlayedAt:yyyy-MM-dd HH:mm}";

    /// <summary>累计游玩时长标签。</summary>
    public string PlayTimeLabel => PlayTime.TotalHours >= 1
        ? $"累计 {PlayTime.TotalHours:0.#} 小时"
        : $"累计 {PlayTime.TotalMinutes:0} 分钟";

    /// <summary>占用空间标签。</summary>
    public string SizeLabel => $"{SizeGb:0.00} GB";

    /// <summary>安装状态标签。</summary>
    public string InstallStateLabel => IsInstalled ? "已安装" : "未安装";
}
