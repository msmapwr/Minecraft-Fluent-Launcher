namespace WINUI.Models;

/// <summary>游戏版本的发布通道。</summary>
public enum VersionChannel
{
    /// <summary>正式版（Release）。</summary>
    Release,

    /// <summary>快照 / 预览版（Snapshot）。</summary>
    Snapshot,

    /// <summary>远古版本（Alpha / Beta）。</summary>
    Legacy,
}
