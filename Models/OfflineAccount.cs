using System;

namespace WINUI.Models;

/// <summary>
/// 离线账户。
/// <para>当前为 UI 阶段的 Mock 数据模型；真实实现会为每个离线账户分配固定 UUID。</para>
/// </summary>
public sealed class OfflineAccount
{
    /// <summary>玩家名。</summary>
    public required string Name { get; init; }

    /// <summary>创建时间。</summary>
    public required DateTimeOffset CreatedAt { get; init; }

    /// <summary>最近一次使用时间；<c>null</c> 表示从未使用。</summary>
    public DateTimeOffset? LastUsedAt { get; init; }

    /// <summary>头像占位文字。</summary>
    public string Initials => Name.Length <= 2 ? Name.ToUpperInvariant() : Name[..2].ToUpperInvariant();

    /// <summary>创建时间标签。</summary>
    public string CreatedAtLabel => $"创建于 {CreatedAt:yyyy-MM-dd}";

    /// <summary>最近使用标签。</summary>
    public string LastUsedLabel => LastUsedAt is null
        ? "从未使用"
        : $"最近使用 {LastUsedAt:yyyy-MM-dd HH:mm}";

    /// <summary>账户类型标签。</summary>
    public string AccountTypeLabel => "离线账户";
}
