namespace WINUI.Models;

/// <summary>下载源的展示辅助。</summary>
public static class DownloadSourceExtensions
{
    /// <summary>取下载源的中文标签。</summary>
    public static string ToLabel(this DownloadSource source) => source switch
    {
        DownloadSource.Official => "官方源",
        DownloadSource.Bmclapi => "BMCLAPI 镜像",
        _ => "社区镜像",
    };
}
