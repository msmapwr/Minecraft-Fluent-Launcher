namespace WINUI.Models;

/// <summary>
/// 玩家账户摘要。
/// <para>当前为 UI 阶段的 Mock 数据模型；真实实现将区分微软正版（OAuth）与离线账户。</para>
/// </summary>
public sealed class PlayerAccount
{
    /// <summary>玩家名（游戏内 ID）。</summary>
    public required string Name { get; init; }

    /// <summary>账户类型描述，如「微软正版」「离线账户」。</summary>
    public required string AccountType { get; init; }

    /// <summary>是否处于登录状态。</summary>
    public bool IsSignedIn { get; init; }

    /// <summary>头像占位文字（取玩家名前两个字符的大写形式）。</summary>
    public string Initials => Name.Length <= 2 ? Name.ToUpperInvariant() : Name[..2].ToUpperInvariant();
}
