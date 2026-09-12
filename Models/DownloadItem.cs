using System;

namespace WINUI.Models;

/// <summary>
/// 下载中心的一个可下载条目。
/// <para>当前为 UI 阶段的 Mock 数据模型，不代表任何真实资源来源。</para>
/// </summary>
public sealed class DownloadItem
{
    /// <summary>条目标识。</summary>
    public required string Id { get; init; }

    /// <summary>名称。</summary>
    public required string Name { get; init; }

    /// <summary>作者 / 来源。</summary>
    public required string Author { get; init; }

    /// <summary>所属分类。</summary>
    public required DownloadCategory Category { get; init; }

    /// <summary>适配的版本范围（「版本」条目即版本号本身）。</summary>
    public required string Version { get; init; }

    /// <summary>体积（MB）。</summary>
    public required double SizeMb { get; init; }

    /// <summary>下载次数。</summary>
    public required int DownloadCount { get; init; }

    /// <summary>简介。</summary>
    public required string Description { get; init; }

    /// <summary>是否已安装。</summary>
    public bool IsInstalled { get; init; }

    /// <summary>发布通道（仅「版本」条目有意义）。</summary>
    public VersionChannel Channel { get; init; } = VersionChannel.Release;

    /// <summary>发布日期（仅「版本」条目有意义）。</summary>
    public DateOnly? ReleasedAt { get; init; }

    /// <summary>是否为可进入详情的「版本」条目。</summary>
    public bool IsVersion => Category == DownloadCategory.GameVersion;

    /// <summary>是否不是「版本」条目（与 <see cref="IsVersion"/> 互斥，用于按钮切换显示）。</summary>
    public bool IsNotVersion => !IsVersion;

    /// <summary>分类标签。</summary>
    public string CategoryLabel => Category.ToLabel();

    /// <summary>体积标签。</summary>
    public string SizeLabel => SizeMb >= 1024
        ? $"{SizeMb / 1024:0.00} GB"
        : $"{SizeMb:0} MB";

    /// <summary>下载次数标签。</summary>
    public string DownloadCountLabel => DownloadCount >= 10000
        ? $"{DownloadCount / 10000.0:0.#} 万次下载"
        : $"{DownloadCount} 次下载";

    /// <summary>安装状态标签。</summary>
    public string InstallStateLabel => IsInstalled ? "已安装" : "未安装";

    /// <summary>发布通道标签。</summary>
    public string ChannelLabel => Channel.ToLabel();

    /// <summary>发布日期标签。</summary>
    public string ReleasedAtLabel => ReleasedAt is null
        ? "—"
        : ReleasedAt.Value.ToString("yyyy-MM-dd");
}
