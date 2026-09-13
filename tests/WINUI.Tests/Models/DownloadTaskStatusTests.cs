using WINUI.Models;
using Xunit;

namespace WINUI.Tests.Models;

/// <summary>下载任务状态机扩展测试（大更新 ⑧-3）。</summary>
public sealed class DownloadTaskStatusTests
{
    [Theory]
    [InlineData(DownloadTaskStatus.Waiting, "等待中")]
    [InlineData(DownloadTaskStatus.Preparing, "准备中")]
    [InlineData(DownloadTaskStatus.Downloading, "下载中")]
    [InlineData(DownloadTaskStatus.Installing, "安装中")]
    [InlineData(DownloadTaskStatus.Completed, "已完成")]
    [InlineData(DownloadTaskStatus.Failed, "失败")]
    [InlineData(DownloadTaskStatus.Cancelled, "已取消")]
    public void ToLabel_ReturnsChineseLabel(DownloadTaskStatus status, string expected)
    {
        Assert.Equal(expected, status.ToLabel());
    }

    [Theory]
    [InlineData(DownloadTaskStatus.Waiting, true)]
    [InlineData(DownloadTaskStatus.Preparing, true)]
    [InlineData(DownloadTaskStatus.Downloading, true)]
    [InlineData(DownloadTaskStatus.Installing, true)]
    [InlineData(DownloadTaskStatus.Completed, false)]
    [InlineData(DownloadTaskStatus.Failed, false)]
    [InlineData(DownloadTaskStatus.Cancelled, false)]
    public void IsActive_MatchesTransitionalStates(DownloadTaskStatus status, bool expected)
    {
        Assert.Equal(expected, status.IsActive());
        Assert.Equal(!expected, status.IsFinished());
    }
}
