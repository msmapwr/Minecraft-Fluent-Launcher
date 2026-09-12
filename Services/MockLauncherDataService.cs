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

    private static readonly IReadOnlyList<DownloadItem> DownloadItems =
    [
        new()
        {
            Id = "mc-1.21.4",
            Name = "Minecraft 1.21.4",
            Author = "Mojang Studios",
            Category = DownloadCategory.GameVersion,
            Version = "1.21.4",
            SizeMb = 312,
            DownloadCount = 128_400,
            Description = "最新正式版，包含「苍园」生物群系与两种木质建材变体。",
            IsInstalled = true,
        },
        new()
        {
            Id = "mc-1.21.1",
            Name = "Minecraft 1.21.1",
            Author = "Mojang Studios",
            Category = DownloadCategory.GameVersion,
            Version = "1.21.1",
            SizeMb = 306,
            DownloadCount = 96_200,
            Description = "模组生态最成熟的 1.21 版本，推荐用于整合包。",
            IsInstalled = true,
        },
        new()
        {
            Id = "loader-neoforge",
            Name = "NeoForge",
            Author = "NeoForged",
            Category = DownloadCategory.Loader,
            Version = "21.1.72 / 21.4.x",
            SizeMb = 24,
            DownloadCount = 42_800,
            Description = "Forge 的社区继任者，支持 1.20.1 及以上的现代模组加载。",
            IsInstalled = true,
        },
        new()
        {
            Id = "loader-fabric",
            Name = "Fabric Loader",
            Author = "FabricMC",
            Category = DownloadCategory.Loader,
            Version = "0.16.9",
            SizeMb = 6,
            DownloadCount = 88_500,
            Description = "轻量、启动快、更新及时，适合性能向与小型模组。",
            IsInstalled = true,
        },
        new()
        {
            Id = "mod-sodium",
            Name = "Sodium",
            Author = "CaffeineMC",
            Category = DownloadCategory.Mod,
            Version = "1.21.4",
            SizeMb = 2,
            DownloadCount = 61_300,
            Description = "重写渲染管线，显著提升帧率与区块加载速度。",
        },
        new()
        {
            Id = "mod-jei",
            Name = "Just Enough Items",
            Author = "mezz",
            Category = DownloadCategory.Mod,
            Version = "1.20.1 – 1.21.4",
            SizeMb = 3,
            DownloadCount = 74_900,
            Description = "物品与合成表查询，模组包必备的基础工具。",
            IsInstalled = true,
        },
        new()
        {
            Id = "resource-faithful",
            Name = "Faithful 32x",
            Author = "Faithful Team",
            Category = DownloadCategory.ResourcePack,
            Version = "1.21.x",
            SizeMb = 18,
            DownloadCount = 52_100,
            Description = "在原版风格基础上提升至 32x 分辨率的经典资源包。",
        },
        new()
        {
            Id = "shader-complementary",
            Name = "Complementary Reimagined",
            Author = "EminGT",
            Category = DownloadCategory.Shader,
            Version = "r5.3",
            SizeMb = 12,
            DownloadCount = 39_600,
            Description = "兼顾性能与观感的写实光影，需搭配 Iris / OptiFine。",
        },
        new()
        {
            Id = "modpack-better-mc",
            Name = "Better MC [FORGE]",
            Author = "Luna Pixel Studios",
            Category = DownloadCategory.Modpack,
            Version = "1.20.1",
            SizeMb = 1024,
            DownloadCount = 23_400,
            Description = "面向探索与冒险的整合包，含数百个模组与任务书。",
        },
        new()
        {
            Id = "world-skyblock",
            Name = "SkyBlock 空岛生存",
            Author = "社区地图",
            Category = DownloadCategory.World,
            Version = "1.21.x",
            SizeMb = 42,
            DownloadCount = 17_800,
            Description = "经典空岛生存地图，附带自定义进度与商店系统。",
        },
    ];

    private static readonly IReadOnlyList<ModEntry> Mods =
    [
        new()
        {
            Id = "sodium",
            Name = "Sodium",
            Author = "CaffeineMC",
            Version = "0.6.5",
            UpdateVersion = "0.6.9",
            Loader = "Fabric",
            GameVersion = "1.21.4",
            FileName = "sodium-fabric-0.6.5.jar",
            SizeKb = 1180,
            Description = "重写渲染管线，大幅提升帧率与区块加载速度，同时保持原版画面风格。",
            Dependencies = [],
            Side = ModSide.Client,
        },
        new()
        {
            Id = "lithium",
            Name = "Lithium",
            Author = "CaffeineMC",
            Version = "0.14.3",
            Loader = "Fabric",
            GameVersion = "1.21.4",
            FileName = "lithium-fabric-0.14.3.jar",
            SizeKb = 864,
            Description = "优化物理、方块刻与实体 AI 等通用逻辑，不改变游戏行为。",
            Dependencies = [],
            Side = ModSide.Both,
        },
        new()
        {
            Id = "fabric-api",
            Name = "Fabric API",
            Author = "FabricMC",
            Version = "0.119.2",
            UpdateVersion = "0.119.4",
            Loader = "Fabric",
            GameVersion = "1.21.4",
            FileName = "fabric-api-0.119.2.jar",
            SizeKb = 2140,
            Description = "Fabric 生态的基础库，绝大多数 Fabric 模组的前置依赖。",
            Dependencies = [],
            Side = ModSide.Both,
        },
        new()
        {
            Id = "jei",
            Name = "Just Enough Items",
            Author = "mezz",
            Version = "19.21.0",
            Loader = "Fabric",
            GameVersion = "1.21.4",
            FileName = "jei-1.21.4-fabric-19.21.0.jar",
            SizeKb = 1320,
            Description = "在界面中浏览全部物品与合成表，模组包必备的查询工具。",
            Dependencies = [],
            Side = ModSide.Client,
        },
        new()
        {
            Id = "modmenu",
            Name = "Mod Menu",
            Author = "Prospector",
            Version = "11.0.3",
            Loader = "Fabric",
            GameVersion = "1.21.4",
            FileName = "modmenu-11.0.3.jar",
            SizeKb = 246,
            Description = "在游戏内查看与管理已安装模组及其配置入口。",
            Dependencies = ["Fabric API", "Text Placeholder API"],
            Side = ModSide.Client,
            IsEnabledByDefault = false,
        },
        new()
        {
            Id = "cloth-config",
            Name = "Cloth Config API",
            Author = "shedaniel",
            Version = "15.0.140",
            Loader = "Fabric",
            GameVersion = "1.21.4",
            FileName = "cloth-config-15.0.140-fabric.jar",
            SizeKb = 612,
            Description = "为模组提供统一的配置界面框架。",
            Dependencies = [],
            Side = ModSide.Both,
        },
        new()
        {
            Id = "appleskin",
            Name = "AppleSkin",
            Author = "squeek502",
            Version = "3.0.5",
            Loader = "Fabric",
            GameVersion = "1.21.4",
            FileName = "appleskin-fabric-3.0.5.jar",
            SizeKb = 148,
            Description = "在饥饿条上显示食物恢复量与饱和度等信息。",
            Dependencies = [],
            Side = ModSide.Client,
        },
        new()
        {
            Id = "iris",
            Name = "Iris Shaders",
            Author = "IrisShaders",
            Version = "1.8.1",
            Loader = "Fabric",
            GameVersion = "1.21.4",
            FileName = "iris-1.8.1+mc1.21.4.jar",
            SizeKb = 1720,
            Description = "兼容 OptiFine 光影包的高性能光影加载器。",
            Dependencies = ["Sodium"],
            Side = ModSide.Client,
            IsEnabledByDefault = false,
        },
        new()
        {
            Id = "xaeros-minimap",
            Name = "Xaero's Minimap",
            Author = "xaero96",
            Version = "24.6.1",
            UpdateVersion = "24.7.0",
            Loader = "Fabric",
            GameVersion = "1.21.4",
            FileName = "xaeros_minimap_24.6.1_Fabric_1.21.jar",
            SizeKb = 1960,
            Description = "可自定义的小地图，支持路径点、实体雷达与洞穴模式。",
            Dependencies = [],
            Side = ModSide.Client,
        },
        new()
        {
            Id = "terralith",
            Name = "Terralith",
            Author = "Stardust Labs",
            Version = "2.5.4",
            Loader = "Fabric",
            GameVersion = "1.21.4",
            FileName = "Terralith_1.21.4_v2.5.4.jar",
            SizeKb = 5240,
            Description = "使用原版方块生成的全新地形，新增上百种生物群系。",
            Dependencies = [],
            Side = ModSide.Both,
            IsEnabledByDefault = false,
        },
    ];

    private static readonly IReadOnlyList<OfflineAccount> OfflineAccounts =
    [
        new()
        {
            Name = "Steve_CN",
            CreatedAt = new DateTimeOffset(2024, 3, 12, 10, 0, 0, TimeSpan.FromHours(8)),
            LastUsedAt = new DateTimeOffset(2024, 11, 24, 20, 30, 0, TimeSpan.FromHours(8)),
        },
        new()
        {
            Name = "CreativeBuilder",
            CreatedAt = new DateTimeOffset(2024, 6, 1, 15, 20, 0, TimeSpan.FromHours(8)),
            LastUsedAt = new DateTimeOffset(2025, 1, 3, 22, 18, 0, TimeSpan.FromHours(8)),
        },
        new()
        {
            Name = "TestWorld",
            CreatedAt = new DateTimeOffset(2025, 1, 6, 9, 45, 0, TimeSpan.FromHours(8)),
        },
    ];

    /// <inheritdoc />
    public Task<IReadOnlyList<OfflineAccount>> GetOfflineAccountsAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(OfflineAccounts);

    /// <inheritdoc />
    public Task<IReadOnlyList<ModEntry>> GetModsAsync(string instanceId, CancellationToken cancellationToken = default)
        => Task.FromResult(Mods);

    /// <inheritdoc />
    public Task<IReadOnlyList<DownloadItem>> GetDownloadItemsAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(DownloadItems);

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
