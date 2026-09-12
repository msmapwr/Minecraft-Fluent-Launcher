using System.Collections.Generic;

namespace WINUI.Models;

/// <summary>
/// 一个模组条目。
/// <para>当前为 UI 阶段的 Mock 数据模型。启用状态由 <c>ModItemViewModel</c> 承载，本类保持不可变。</para>
/// </summary>
public sealed class ModEntry
{
    /// <summary>模组标识。</summary>
    public required string Id { get; init; }

    /// <summary>名称。</summary>
    public required string Name { get; init; }

    /// <summary>作者。</summary>
    public required string Author { get; init; }

    /// <summary>当前版本。</summary>
    public required string Version { get; init; }

    /// <summary>可用更新版本；<c>null</c> 表示已是最新。</summary>
    public string? UpdateVersion { get; init; }

    /// <summary>适配的加载器。</summary>
    public required string Loader { get; init; }

    /// <summary>适配的游戏版本。</summary>
    public required string GameVersion { get; init; }

    /// <summary>文件名。</summary>
    public required string FileName { get; init; }

    /// <summary>体积（KB）。</summary>
    public required double SizeKb { get; init; }

    /// <summary>简介。</summary>
    public required string Description { get; init; }

    /// <summary>前置依赖名称。</summary>
    public required IReadOnlyList<string> Dependencies { get; init; }

    /// <summary>运行侧。</summary>
    public ModSide Side { get; init; } = ModSide.Both;

    /// <summary>默认是否启用。</summary>
    public bool IsEnabledByDefault { get; init; } = true;

    /// <summary>是否存在更新。</summary>
    public bool HasUpdate => UpdateVersion is not null;

    /// <summary>版本展示（含更新提示）。</summary>
    public string VersionLabel => HasUpdate ? $"{Version} → {UpdateVersion}" : Version;

    /// <summary>体积标签。</summary>
    public string SizeLabel => SizeKb >= 1024 ? $"{SizeKb / 1024:0.0} MB" : $"{SizeKb:0} KB";

    /// <summary>运行侧标签。</summary>
    public string SideLabel => Side.ToLabel();

    /// <summary>依赖摘要。</summary>
    public string DependencySummary => Dependencies.Count == 0
        ? "无前置依赖"
        : $"前置依赖：{string.Join("、", Dependencies)}";

    /// <summary>加载器与游戏版本摘要。</summary>
    public string TargetSummary => $"{Loader} · {GameVersion}";
}
