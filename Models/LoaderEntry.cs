using System.Collections.Generic;

namespace WINUI.Models;

/// <summary>
/// 某个游戏版本下可安装的一个加载器（含可选版本清单）。
/// <para>当前为 UI 阶段的 Mock 数据模型。</para>
/// </summary>
public sealed class LoaderEntry
{
    /// <summary>加载器种类。</summary>
    public required ModLoader Loader { get; init; }

    /// <summary>可选的加载器版本（按推荐度排序，首项为最新）。</summary>
    public required IReadOnlyList<string> Versions { get; init; }

    /// <summary>推荐安装的加载器版本。</summary>
    public required string RecommendedVersion { get; init; }

    /// <summary>加载器展示名。</summary>
    public string Label => Loader.ToLabel();

    /// <summary>加载器说明。</summary>
    public string Description => Loader.ToDescription();

    /// <summary>推荐版本标签。</summary>
    public string RecommendedLabel => $"推荐 {RecommendedVersion}";
}
