namespace WINUI.Models;

/// <summary>下载分类的展示辅助。</summary>
public static class DownloadCategoryExtensions
{
    /// <summary>取分类的中文标签。</summary>
    public static string ToLabel(this DownloadCategory category) => category switch
    {
        DownloadCategory.GameVersion => "版本",
        DownloadCategory.Mod => "模组",
        DownloadCategory.ResourcePack => "资源包",
        DownloadCategory.Shader => "光影",
        DownloadCategory.World => "世界",
        DownloadCategory.DataPack => "数据包",
        _ => "整合包",
    };
}
