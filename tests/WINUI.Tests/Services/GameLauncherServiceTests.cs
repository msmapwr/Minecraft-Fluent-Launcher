using WINUI.Models;
using WINUI.Services;
using Xunit;

namespace WINUI.Tests.Services;

/// <summary>
/// 实例扫描相关的纯逻辑测试（大更新 ⑦-2）：从版本目录名猜测发布通道。
/// </summary>
public sealed class GameLauncherServiceTests
{
    [Theory]
    [InlineData("1.21.4")]
    [InlineData("1.20.1")]
    [InlineData("1.0")]
    public void GuessChannel_NumericVersions_AreRelease(string versionId)
    {
        Assert.Equal(VersionChannel.Release, CoreLauncherDataService.GuessChannel(versionId));
    }

    [Theory]
    [InlineData("24w14a")]
    [InlineData("1.21.4-pre2")]
    public void GuessChannel_SnapshotLike_AreSnapshot(string versionId)
    {
        Assert.Equal(VersionChannel.Snapshot, CoreLauncherDataService.GuessChannel(versionId));
    }

    [Theory]
    [InlineData("a1.2.5")]
    [InlineData("b1.7.3")]
    public void GuessChannel_AlphaBeta_AreLegacy(string versionId)
    {
        Assert.Equal(VersionChannel.Legacy, CoreLauncherDataService.GuessChannel(versionId));
    }
}
