namespace WINUI.Models;

/// <summary>下载分类的展示辅助。</summary>
public static class DownloadCategoryExtensions
{
    /// <summary>取分类的中文标签。</summary>
    public static string ToLabel(this DownloadCategory category) => category switch
    {
        DownloadCategory.GameVersion => "游戏版本",
        DownloadCategory.Loader => "模组加载器",
        DownloadCategory.Mod => "模组",
        DownloadCategory.ResourcePack => "资源包",
        DownloadCategory.Shader => "光影",
        DownloadCategory.Modpack => "整合包",
        _ => "地图存档",
    };
}
