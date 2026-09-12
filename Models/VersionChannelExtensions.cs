namespace WINUI.Models;

/// <summary>发布通道的展示辅助。</summary>
public static class VersionChannelExtensions
{
    /// <summary>取发布通道的中文标签。</summary>
    public static string ToLabel(this VersionChannel channel) => channel switch
    {
        VersionChannel.Release => "正式版",
        VersionChannel.Snapshot => "快照",
        _ => "远古版",
    };
}
