using CmlLib.Core.VersionMetadata;
using WINUI.Models;
using WINUI.Services;
using Xunit;

namespace WINUI.Tests.Services;

/// <summary>
/// CMLLib 真实数据服务的纯逻辑测试（大更新 ⑦-1）。
/// 网络相关（真实清单拉取）不在单测范围内，由运行时验证。
/// </summary>
public sealed class CoreLauncherDataServiceTests
{
    [Fact]
    public void MapChannel_Release_And_Snapshot()
    {
        Assert.Equal(VersionChannel.Release, CoreLauncherDataService.MapChannel(MVersionType.Release));
        Assert.Equal(VersionChannel.Snapshot, CoreLauncherDataService.MapChannel(MVersionType.Snapshot));
    }

    [Theory]
    [InlineData(MVersionType.OldBeta)]
    [InlineData(MVersionType.OldAlpha)]
    public void MapChannel_OldVersions_MapToLegacy(MVersionType type)
    {
        Assert.Equal(VersionChannel.Legacy, CoreLauncherDataService.MapChannel(type));
    }
}
