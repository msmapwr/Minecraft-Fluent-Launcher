using System;

namespace WINUI.Models;

/// <summary>
/// 新闻 / 公告条目。
/// <para>当前为 UI 阶段的 Mock 数据模型。</para>
/// </summary>
public sealed class NewsItem
{
    /// <summary>标题。</summary>
    public required string Title { get; init; }

    /// <summary>分类标签，如「更新」「公告」「活动」。</summary>
    public required string Category { get; init; }

    /// <summary>摘要。</summary>
    public required string Summary { get; init; }

    /// <summary>发布时间。</summary>
    public required DateOnly PublishedAt { get; init; }

    /// <summary>发布日期标签。</summary>
    public string PublishedAtLabel => PublishedAt.ToString("MM-dd");
}
