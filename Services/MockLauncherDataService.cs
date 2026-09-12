using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WINUI.Models;

namespace WINUI.Services;

/// <summary>
/// <see cref="ILauncherDataService"/> 的 Mock 实现，用于 UI 阶段填充界面。
/// <para>
/// 所有数据均为硬编码常量，<b>不产生任何网络请求</b>，也<b>不代表真实版本清单</b>。
/// 接入真实启动核心后，本类将被替换（或移除）。
/// </para>
/// </summary>
public sealed class MockLauncherDataService : ILauncherDataService
{
    private static readonly IReadOnlyList<GameVersion> Versions =
    [
        new()
        {
            Id = "1.21.4",
            DisplayName = "1.21.4",
            Channel = VersionChannel.Release,
            Loader = "Fabric 0.16.9",
            ReleasedAt = new DateOnly(2024, 12, 3),
            IsInstalled = true,
        },
        new()
        {
            Id = "1.21.4",
            DisplayName = "1.21.4",
            Channel = VersionChannel.Release,
            Loader = "Vanilla",
            ReleasedAt = new DateOnly(2024, 12, 3),
            IsInstalled = true,
        },
        new()
        {
            Id = "1.21.1",
            DisplayName = "1.21.1",
            Channel = VersionChannel.Release,
            Loader = "NeoForge 21.1.72",
            ReleasedAt = new DateOnly(2024, 8, 8),
            IsInstalled = true,
        },
        new()
        {
            Id = "1.20.1",
            DisplayName = "1.20.1",
            Channel = VersionChannel.Release,
            Loader = "Forge 47.3.0",
            ReleasedAt = new DateOnly(2023, 6, 12),
        },
        new()
        {
            Id = "1.20.1",
            DisplayName = "1.20.1",
            Channel = VersionChannel.Release,
            Loader = "Fabric 0.15.11",
            ReleasedAt = new DateOnly(2023, 6, 12),
        },
        new()
        {
            Id = "1.19.2",
            DisplayName = "1.19.2",
            Channel = VersionChannel.Release,
            Loader = "Quilt 0.23.1",
            ReleasedAt = new DateOnly(2022, 8, 5),
        },
        new()
        {
            Id = "25w03a",
            DisplayName = "25w03a",
            Channel = VersionChannel.Snapshot,
            Loader = "Vanilla",
            ReleasedAt = new DateOnly(2025, 1, 15),
        },
        new()
        {
            Id = "b1.7.3",
            DisplayName = "Beta 1.7.3",
            Channel = VersionChannel.Legacy,
            Loader = "Vanilla",
            ReleasedAt = new DateOnly(2011, 7, 8),
        },
    ];

    private static readonly IReadOnlyList<NewsItem> News =
    [
        new()
        {
            Title = "1.21.4 正式版已发布",
            Category = "更新",
            Summary = "新增「苍园」生物群系与两种木质建材变体，并优化了区块加载性能。",
            PublishedAt = new DateOnly(2024, 12, 3),
        },
        new()
        {
            Title = "MFL 界面骨架完成，进入核心页面阶段",
            Category = "公告",
            Summary = "设计系统、导航外壳与自定义标题栏已就绪，接下来逐个落地核心页面。",
            PublishedAt = new DateOnly(2025, 1, 8),
        },
        new()
        {
            Title = "NeoForge 21.4 兼容性提示",
            Category = "提示",
            Summary = "使用 1.21.1 及以上版本时，请确认模组与 NeoForge 版本匹配，避免启动崩溃。",
            PublishedAt = new DateOnly(2025, 1, 6),
        },
        new()
        {
            Title = "春季建筑大赛开始报名",
            Category = "活动",
            Summary = "主题「绿洲之城」，报名截止 3 月 15 日，优秀作品将在启动器首页展示。",
            PublishedAt = new DateOnly(2025, 1, 2),
        },
    ];

    private static readonly PlayerAccount Account = new()
    {
        Name = "Msmapwr",
        AccountType = "微软正版",
        IsSignedIn = true,
    };

    private static readonly IReadOnlyList<GameInstance> Instances =
    [
        new()
        {
            Id = "survival-1.21.4-fabric",
            Name = "生存 · 长期档",
            GameVersion = "1.21.4",
            Loader = "Fabric 0.16.9",
            Channel = VersionChannel.Release,
            LastPlayedAt = new DateTimeOffset(2025, 1, 8, 21, 42, 0, TimeSpan.FromHours(8)),
            PlayTime = TimeSpan.FromHours(146.5),
            SizeGb = 4.82,
            IsInstalled = true,
        },
        new()
        {
            Id = "neoforge-1.21.1-modpack",
            Name = "整合包 · 机械工坊",
            GameVersion = "1.21.1",
            Loader = "NeoForge 21.1.72",
            Channel = VersionChannel.Release,
            LastPlayedAt = new DateTimeOffset(2025, 1, 6, 19, 5, 0, TimeSpan.FromHours(8)),
            PlayTime = TimeSpan.FromHours(58.2),
            SizeGb = 12.34,
            IsInstalled = true,
        },
        new()
        {
            Id = "vanilla-1.21.4",
            Name = "原版 · 纯净",
            GameVersion = "1.21.4",
            Loader = "Vanilla",
            Channel = VersionChannel.Release,
            LastPlayedAt = new DateTimeOffset(2025, 1, 3, 22, 18, 0, TimeSpan.FromHours(8)),
            PlayTime = TimeSpan.FromHours(9.4),
            SizeGb = 1.06,
            IsInstalled = true,
        },
        new()
        {
            Id = "forge-1.20.1-old",
            Name = "怀旧 · 1.20.1 Forge",
            GameVersion = "1.20.1",
            Loader = "Forge 47.3.0",
            Channel = VersionChannel.Release,
            LastPlayedAt = new DateTimeOffset(2024, 11, 24, 20, 30, 0, TimeSpan.FromHours(8)),
            PlayTime = TimeSpan.FromHours(31.7),
            SizeGb = 7.19,
            IsInstalled = true,
        },
        new()
        {
            Id = "snapshot-25w03a",
            Name = "快照实验 · 25w03a",
            GameVersion = "25w03a",
            Loader = "Vanilla",
            Channel = VersionChannel.Snapshot,
            PlayTime = TimeSpan.Zero,
            SizeGb = 0.94,
        },
        new()
        {
            Id = "legacy-b1.7.3",
            Name = "远古 · Beta 1.7.3",
            GameVersion = "b1.7.3",
            Loader = "Vanilla",
            Channel = VersionChannel.Legacy,
            PlayTime = TimeSpan.FromMinutes(42),
            SizeGb = 0.18,
            IsInstalled = true,
        },
    ];

    /// <inheritdoc />
    public Task<IReadOnlyList<GameVersion>> GetVersionsAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(Versions);

    /// <inheritdoc />
    public Task<IReadOnlyList<GameInstance>> GetInstancesAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(Instances);

    /// <inheritdoc />
    public Task<IReadOnlyList<NewsItem>> GetNewsAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(News);

    /// <inheritdoc />
    public Task<PlayerAccount> GetCurrentAccountAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(Account);
}
