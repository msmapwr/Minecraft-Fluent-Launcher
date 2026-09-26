namespace WINUI.Models;

/// <summary>
/// 安装 / 下载的真实进度快照（由 CMLLib 的字节进度事件上报）。
/// </summary>
/// <param name="ProgressedBytes">已处理字节数。</param>
/// <param name="TotalBytes">总字节数；<c>0</c> 表示未知。</param>
public readonly record struct DownloadProgress(long ProgressedBytes, long TotalBytes)
{
    /// <summary>完成比例（0–1）。总字节未知时为 0。</summary>
    public double Ratio => TotalBytes <= 0
        ? 0
        : System.Math.Clamp((double)ProgressedBytes / TotalBytes, 0, 1);

    /// <summary>已处理体积（MB）。</summary>
    public double ProgressedMb => ProgressedBytes / 1_000_000.0;

    /// <summary>总体积（MB）。</summary>
    public double TotalMb => TotalBytes / 1_000_000.0;
}
