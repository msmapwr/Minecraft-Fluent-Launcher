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
        // ==================== 版本 ====================
        new()
        {
            Id = "mc-1.21.4",
            Name = "Minecraft 1.21.4",
            Author = "Mojang Studios",
            Category = DownloadCategory.GameVersion,
            Version = "1.21.4",
            Channel = VersionChannel.Release,
            ReleasedAt = new DateOnly(2024, 12, 3),
            SizeMb = 312,
            DownloadCount = 128_400,
            Description = "最新正式版，包含「苍园」生物群系与两种木质建材变体。",
            IsInstalled = true,
        },
        new()
        {
            Id = "mc-1.21.3",
            Name = "Minecraft 1.21.3",
            Author = "Mojang Studios",
            Category = DownloadCategory.GameVersion,
            Version = "1.21.3",
            Channel = VersionChannel.Release,
            ReleasedAt = new DateOnly(2024, 10, 23),
            SizeMb = 309,
            DownloadCount = 42_100,
            Description = "1.21 系列的稳定修正版本，修复了若干崩溃与同步问题。",
        },
        new()
        {
            Id = "mc-1.21.1",
            Name = "Minecraft 1.21.1",
            Author = "Mojang Studios",
            Category = DownloadCategory.GameVersion,
            Version = "1.21.1",
            Channel = VersionChannel.Release,
            ReleasedAt = new DateOnly(2024, 8, 8),
            SizeMb = 306,
            DownloadCount = 96_200,
            Description = "模组生态最成熟的 1.21 版本，推荐用于整合包。",
            IsInstalled = true,
        },
        new()
        {
            Id = "mc-1.21",
            Name = "Minecraft 1.21",
            Author = "Mojang Studios",
            Category = DownloadCategory.GameVersion,
            Version = "1.21",
            Channel = VersionChannel.Release,
            ReleasedAt = new DateOnly(2024, 6, 13),
            SizeMb = 305,
            DownloadCount = 88_700,
            Description = "「诡谲试炼」更新，新增试炼密室、重锤武器与自动合成器。",
        },
        new()
        {
            Id = "mc-1.20.6",
            Name = "Minecraft 1.20.6",
            Author = "Mojang Studios",
            Category = DownloadCategory.GameVersion,
            Version = "1.20.6",
            Channel = VersionChannel.Release,
            ReleasedAt = new DateOnly(2024, 4, 29),
            SizeMb = 298,
            DownloadCount = 34_500,
            Description = "1.20 系列的收尾版本，以小幅平衡性调整为主。",
        },
        new()
        {
            Id = "mc-1.20.4",
            Name = "Minecraft 1.20.4",
            Author = "Mojang Studios",
            Category = DownloadCategory.GameVersion,
            Version = "1.20.4",
            Channel = VersionChannel.Release,
            ReleasedAt = new DateOnly(2023, 12, 7),
            SizeMb = 296,
            DownloadCount = 47_900,
            Description = "修复 1.20.3 中的装饰陶罐与方块更新问题。",
        },
        new()
        {
            Id = "mc-1.20.1",
            Name = "Minecraft 1.20.1",
            Author = "Mojang Studios",
            Category = DownloadCategory.GameVersion,
            Version = "1.20.1",
            Channel = VersionChannel.Release,
            ReleasedAt = new DateOnly(2023, 6, 12),
            SizeMb = 214,
            DownloadCount = 156_300,
            Description = "Forge / NeoForge 生态最成熟的版本，模组兼容性最佳。",
            IsInstalled = true,
        },
        new()
        {
            Id = "mc-1.19.4",
            Name = "Minecraft 1.19.4",
            Author = "Mojang Studios",
            Category = DownloadCategory.GameVersion,
            Version = "1.19.4",
            Channel = VersionChannel.Release,
            ReleasedAt = new DateOnly(2023, 3, 14),
            SizeMb = 210,
            DownloadCount = 39_200,
            Description = "「荒野更新」之后的稳定版本，加入樱花木与饰纹陶罐。",
        },
        new()
        {
            Id = "mc-1.19.2",
            Name = "Minecraft 1.19.2",
            Author = "Mojang Studios",
            Category = DownloadCategory.GameVersion,
            Version = "1.19.2",
            Channel = VersionChannel.Release,
            ReleasedAt = new DateOnly(2022, 8, 5),
            SizeMb = 208,
            DownloadCount = 63_800,
            Description = "经典稳定版本，大量老整合包仍以此为基线。",
        },
        new()
        {
            Id = "mc-1.18.2",
            Name = "Minecraft 1.18.2",
            Author = "Mojang Studios",
            Category = DownloadCategory.GameVersion,
            Version = "1.18.2",
            Channel = VersionChannel.Release,
            ReleasedAt = new DateOnly(2022, 2, 28),
            SizeMb = 196,
            DownloadCount = 55_400,
            Description = "「洞穴与山崖」第二阶段的收官版本。",
        },
        new()
        {
            Id = "mc-25w03a",
            Name = "Minecraft 25w03a",
            Author = "Mojang Studios",
            Category = DownloadCategory.GameVersion,
            Version = "25w03a",
            Channel = VersionChannel.Snapshot,
            ReleasedAt = new DateOnly(2025, 1, 15),
            SizeMb = 288,
            DownloadCount = 6_200,
            Description = "2025 年首个快照，含实验性的世界生成改动。",
        },
        new()
        {
            Id = "mc-b1.7.3",
            Name = "Minecraft Beta 1.7.3",
            Author = "Mojang Studios",
            Category = DownloadCategory.GameVersion,
            Version = "b1.7.3",
            Channel = VersionChannel.Legacy,
            ReleasedAt = new DateOnly(2011, 7, 8),
            SizeMb = 42,
            DownloadCount = 12_900,
            Description = "远古 Beta 版本，仅推荐怀旧与存档研究使用。",
        },

        // ==================== 模组 ====================
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
            Id = "mod-lithium",
            Name = "Lithium",
            Author = "CaffeineMC",
            Category = DownloadCategory.Mod,
            Version = "1.21.4",
            SizeMb = 1,
            DownloadCount = 44_200,
            Description = "优化物理、方块刻与实体 AI，不改变游戏行为。",
        },
        new()
        {
            Id = "mod-iris",
            Name = "Iris Shaders",
            Author = "IrisShaders",
            Category = DownloadCategory.Mod,
            Version = "1.21.4",
            SizeMb = 2,
            DownloadCount = 38_700,
            Description = "兼容 OptiFine 光影包的高性能光影加载器。",
        },
        new()
        {
            Id = "mod-xaero",
            Name = "Xaero's Minimap",
            Author = "xaero96",
            Category = DownloadCategory.Mod,
            Version = "1.21.x",
            SizeMb = 3,
            DownloadCount = 29_500,
            Description = "可自定义的小地图，支持路径点与实体雷达。",
        },
        new()
        {
            Id = "mod-terralith",
            Name = "Terralith",
            Author = "Stardust Labs",
            Category = DownloadCategory.Mod,
            Version = "1.21.4",
            SizeMb = 5,
            DownloadCount = 18_400,
            Description = "使用原版方块生成的全新地形，新增上百种生物群系。",
        },

        // ==================== 资源包 ====================
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
            Id = "resource-barebones",
            Name = "Bare Bones",
            Author = "RobotPants",
            Category = DownloadCategory.ResourcePack,
            Version = "1.21.x",
            SizeMb = 6,
            DownloadCount = 21_800,
            Description = "极简平面风格，配色干净、辨识度高。",
        },
        new()
        {
            Id = "resource-staytrue",
            Name = "Stay True",
            Author = "BobisHere",
            Category = DownloadCategory.ResourcePack,
            Version = "1.21.x",
            SizeMb = 24,
            DownloadCount = 17_300,
            Description = "保留原版比例的同时优化材质细节与光照贴图。",
        },
        new()
        {
            Id = "resource-freshanim",
            Name = "Fresh Animations",
            Author = "FreshLX",
            Category = DownloadCategory.ResourcePack,
            Version = "1.20.1 – 1.21.4",
            SizeMb = 12,
            DownloadCount = 33_600,
            Description = "为生物添加细腻的动画，需要 OptiFine / ETF 支持。",
        },

        // ==================== 光影 ====================
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
            Id = "shader-bsl",
            Name = "BSL Shaders",
            Author = "CaptTatsu",
            Category = DownloadCategory.Shader,
            Version = "8.2.06",
            SizeMb = 10,
            DownloadCount = 46_900,
            Description = "风格柔和的经典光影，配置门槛低、兼容性好。",
        },
        new()
        {
            Id = "shader-seus",
            Name = "SEUS PTGI",
            Author = "Sonic Ether",
            Category = DownloadCategory.Shader,
            Version = "HRR 2.1",
            SizeMb = 26,
            DownloadCount = 15_200,
            Description = "基于光线追踪的写实光影，对显卡性能要求较高。",
        },
        new()
        {
            Id = "shader-sildurs",
            Name = "Sildur's Vibrant Shaders",
            Author = "Sildur",
            Category = DownloadCategory.Shader,
            Version = "1.51",
            SizeMb = 9,
            DownloadCount = 24_700,
            Description = "色彩鲜艳、可分级配置的光影，适合中端显卡。",
        },

        // ==================== 世界 ====================
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
        new()
        {
            Id = "world-oneblock",
            Name = "One Block 单方块生存",
            Author = "社区地图",
            Category = DownloadCategory.World,
            Version = "1.21.x",
            SizeMb = 8,
            DownloadCount = 26_400,
            Description = "从一块方块开始，逐步扩展世界的趣味玩法。",
        },
        new()
        {
            Id = "world-parkour",
            Name = "Parkour Paradise",
            Author = "社区地图",
            Category = DownloadCategory.World,
            Version = "1.20.1",
            SizeMb = 15,
            DownloadCount = 13_600,
            Description = "100 关跑酷地图，难度循序渐进，适合单人挑战。",
        },
        new()
        {
            Id = "world-city",
            Name = "现代都市建造模板",
            Author = "社区地图",
            Category = DownloadCategory.World,
            Version = "1.21.x",
            SizeMb = 96,
            DownloadCount = 9_300,
            Description = "预置道路与地块的现代都市地图，适合建筑与红石创作。",
        },

        // ==================== 数据包 ====================
        new()
        {
            Id = "datapack-vanillatweaks",
            Name = "Vanilla Tweaks 数据包集",
            Author = "Vanilla Tweaks",
            Category = DownloadCategory.DataPack,
            Version = "1.21.x",
            SizeMb = 3,
            DownloadCount = 41_200,
            Description = "社区维护的数据包合集，可按需勾选合成、HUD 与实用调整。",
        },
        new()
        {
            Id = "datapack-terralith",
            Name = "Terralith 数据包版",
            Author = "Stardust Labs",
            Category = DownloadCategory.DataPack,
            Version = "1.21.4",
            SizeMb = 5,
            DownloadCount = 12_100,
            Description = "无需安装模组即可获得 Terralith 的全新地形生成。",
        },
        new()
        {
            Id = "datapack-crafting",
            Name = "更好的合成配方",
            Author = "社区数据包",
            Category = DownloadCategory.DataPack,
            Version = "1.21.x",
            SizeMb = 1,
            DownloadCount = 19_800,
            Description = "补充大量便利合成配方，如马铠、命名牌与附魔金苹果。",
        },
        new()
        {
            Id = "datapack-hardcore",
            Name = "硬核难度调整",
            Author = "社区数据包",
            Category = DownloadCategory.DataPack,
            Version = "1.20.1 – 1.21.4",
            SizeMb = 1,
            DownloadCount = 8_900,
            Description = "提高怪物强度与饥饿消耗，面向硬核生存玩家。",
        },

        // ==================== 整合包 ====================
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
            Id = "modpack-fabulously",
            Name = "Fabulously Optimized",
            Author = "RobotPants 社区",
            Category = DownloadCategory.Modpack,
            Version = "1.21.4",
            SizeMb = 486,
            DownloadCount = 35_700,
            Description = "以性能与帧率优化为核心的轻量整合包，安装即玩。",
        },
        new()
        {
            Id = "modpack-rlcraft",
            Name = "RLCraft",
            Author = "Shivaxi",
            Category = DownloadCategory.Modpack,
            Version = "1.12.2",
            SizeMb = 2048,
            DownloadCount = 61_500,
            Description = "硬核生存整合包的代表作，以极高难度与庞大模组量闻名。",
        },
    ];

    /// <summary>
    /// 可安装的模组加载器（Mock）。具体某个游戏版本能装哪些，由 <see cref="GetLoadersAsync"/> 决定。
    /// </summary>
    private static readonly LoaderEntry FabricEntry = new()
    {
        Loader = ModLoader.Fabric,
        Versions = ["0.16.10", "0.16.9", "0.15.11", "0.15.7"],
        RecommendedVersion = "0.16.9",
    };

    private static readonly LoaderEntry NeoForgeEntry = new()
    {
        Loader = ModLoader.NeoForge,
        Versions = ["21.4.76", "21.1.72", "20.6.119"],
        RecommendedVersion = "21.1.72",
    };

    private static readonly LoaderEntry ForgeEntry = new()
    {
        Loader = ModLoader.Forge,
        Versions = ["47.4.0", "47.3.0", "43.2.0"],
        RecommendedVersion = "47.3.0",
    };

    private static readonly LoaderEntry QuiltEntry = new()
    {
        Loader = ModLoader.Quilt,
        Versions = ["0.24.0", "0.23.1", "0.22.0"],
        RecommendedVersion = "0.23.1",
    };

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

    private static readonly IReadOnlyList<LogEntry> LogEntries =
    [
        new() { Timestamp = new DateTimeOffset(2026, 1, 8, 20, 12, 4, 118, TimeSpan.FromHours(8)), Level = AppLogLevel.Info, Source = "Launcher", Message = "Minecraft Fluent Launcher 0.1.0 启动完成" },
        new() { Timestamp = new DateTimeOffset(2026, 1, 8, 20, 12, 4, 191, TimeSpan.FromHours(8)), Level = AppLogLevel.Info, Source = "Settings", Message = "已从 settings.json 载入设置（主题 = 跟随系统）" },
        new() { Timestamp = new DateTimeOffset(2026, 1, 8, 20, 12, 4, 233, TimeSpan.FromHours(8)), Level = AppLogLevel.Debug, Source = "Theme", Message = "根元素 RequestedTheme 已应用为 Default" },
        new() { Timestamp = new DateTimeOffset(2026, 1, 8, 20, 12, 5, 42, TimeSpan.FromHours(8)), Level = AppLogLevel.Info, Source = "Java", Message = "检测到 Java 21.0.4（C:\\Program Files\\Java\\jdk-21\\bin\\javaw.exe）" },
        new() { Timestamp = new DateTimeOffset(2026, 1, 8, 20, 12, 5, 96, TimeSpan.FromHours(8)), Level = AppLogLevel.Warning, Source = "Java", Message = "未检测到 Java 17，1.17 及以下版本将无法启动" },
        new() { Timestamp = new DateTimeOffset(2026, 1, 8, 20, 13, 11, 507, TimeSpan.FromHours(8)), Level = AppLogLevel.Info, Source = "Versions", Message = "已加载 8 个版本条目，其中 3 个为本地已安装" },
        new() { Timestamp = new DateTimeOffset(2026, 1, 8, 20, 13, 12, 8, TimeSpan.FromHours(8)), Level = AppLogLevel.Debug, Source = "Instances", Message = "扫描实例目录，发现 6 个实例" },
        new() { Timestamp = new DateTimeOffset(2026, 1, 8, 20, 14, 30, 664, TimeSpan.FromHours(8)), Level = AppLogLevel.Error, Source = "Download", Message = "下载 resources.download.minecraft.net 超时，已回退到 BMCLAPI 镜像" },
        new() { Timestamp = new DateTimeOffset(2026, 1, 8, 20, 14, 31, 12, TimeSpan.FromHours(8)), Level = AppLogLevel.Info, Source = "Download", Message = "已切换下载源：官方源 → BMCLAPI 镜像" },
        new() { Timestamp = new DateTimeOffset(2026, 1, 8, 20, 15, 2, 330, TimeSpan.FromHours(8)), Level = AppLogLevel.Info, Source = "Assets", Message = "缺失资源 3 项，已加入下载队列" },
        new() { Timestamp = new DateTimeOffset(2026, 1, 8, 20, 15, 48, 902, TimeSpan.FromHours(8)), Level = AppLogLevel.Info, Source = "Assets", Message = "资源校验完成（3/3），耗时 46.5 秒" },
        new() { Timestamp = new DateTimeOffset(2026, 1, 8, 20, 15, 49, 41, TimeSpan.FromHours(8)), Level = AppLogLevel.Debug, Source = "Launch", Message = "构建启动参数：-Xmx4096M -Xms1024M -Djava.library.path=…" },
        new() { Timestamp = new DateTimeOffset(2026, 1, 8, 20, 15, 50, 118, TimeSpan.FromHours(8)), Level = AppLogLevel.Error, Source = "Launch", Message = "java.lang.NoClassDefFoundError: com/example/OptimizationCore（模组 OptimizationCore 与本版本不兼容）" },
        new() { Timestamp = new DateTimeOffset(2026, 1, 8, 20, 15, 50, 205, TimeSpan.FromHours(8)), Level = AppLogLevel.Warning, Source = "Mods", Message = "已自动禁用 1 个不兼容模组，请前往模组页确认" },
        new() { Timestamp = new DateTimeOffset(2026, 1, 8, 20, 16, 3, 771, TimeSpan.FromHours(8)), Level = AppLogLevel.Info, Source = "Launch", Message = "游戏进程已启动（PID 18344），用时 14.8 秒" },
        new() { Timestamp = new DateTimeOffset(2026, 1, 8, 20, 16, 3, 812, TimeSpan.FromHours(8)), Level = AppLogLevel.Trace, Source = "Launch", Message = "标准输出管道已连接，开始转发日志" },
        new() { Timestamp = new DateTimeOffset(2026, 1, 8, 21, 42, 55, 640, TimeSpan.FromHours(8)), Level = AppLogLevel.Info, Source = "Launch", Message = "游戏进程已退出，退出码 0，本次会话 1 小时 26 分" },
    ];

    /// <inheritdoc />
    public Task<IReadOnlyList<LogEntry>> GetLogEntriesAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(LogEntries);

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
    /// <remarks>
    /// 简化规则（Mock）：快照 / 远古版本不提供第三方加载器；
    /// Forge 仅面向 1.20.x 及以下，NeoForge 面向 1.20.1 及以上。
    /// </remarks>
    public Task<IReadOnlyList<LoaderEntry>> GetLoadersAsync(string gameVersion, CancellationToken cancellationToken = default)
    {
        if (!TryParseVersion(gameVersion, out var major, out var minor))
        {
            return Task.FromResult<IReadOnlyList<LoaderEntry>>([]);
        }

        var result = new List<LoaderEntry> { FabricEntry };

        if (major > 1 || minor >= 20)
        {
            result.Add(NeoForgeEntry);
        }

        if (major == 1 && minor <= 20)
        {
            result.Add(ForgeEntry);
        }

        result.Add(QuiltEntry);

        return Task.FromResult<IReadOnlyList<LoaderEntry>>(result);
    }

    /// <summary>从「主版本.次版本」形式的版本号中解析出两个数字；快照 / 远古版本解析失败。</summary>
    private static bool TryParseVersion(string value, out int major, out int minor)
    {
        major = 0;
        minor = 0;

        var parts = value.Split('.');
        return parts.Length >= 2
            && int.TryParse(parts[0], out major)
            && int.TryParse(parts[1], out minor);
    }

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
