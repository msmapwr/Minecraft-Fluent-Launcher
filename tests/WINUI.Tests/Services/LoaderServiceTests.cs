using System;
using System.Threading.Tasks;
using WINUI.Models;
using WINUI.Services;
using WINUI.Tests.Helpers;
using Xunit;

namespace WINUI.Tests.Services;

/// <summary>
/// 模组加载器（大更新 ⑩）的清单与安装器测试。
/// <para>
/// 不依赖网络：只验证能力判定、profile 解析与「不支持即明确报错」的行为契约。
/// 真实下载链路由用户实测覆盖。
/// </para>
/// </summary>
public sealed class LoaderServiceTests
{
    [Theory]
    [InlineData(ModLoader.Fabric, true)]
    [InlineData(ModLoader.Quilt, true)]
    [InlineData(ModLoader.Forge, false)]
    [InlineData(ModLoader.NeoForge, false)]
    public void CanInstall_ReflectsSupportedLoaders(ModLoader loader, bool expected)
    {
        var catalog = new LoaderCatalogService();
        var installer = new LoaderInstallerService(new FakeGameLauncherService());

        Assert.Equal(expected, catalog.CanInstall(loader));
        Assert.Equal(expected, installer.CanInstall(loader));
    }

    [Theory]
    [InlineData("{\"id\":\"fabric-loader-0.16.9-1.21.4\"}", "fabric-loader-0.16.9-1.21.4")]
    [InlineData("{\"id\":\"\",\"mainClass\":\"x\"}", null)]
    [InlineData("{\"mainClass\":\"x\"}", null)]
    [InlineData("not json", null)]
    public void ReadVersionId_ParsesProfileId(string json, string? expected)
    {
        Assert.Equal(expected, LoaderInstallerService.ReadVersionId(json));
    }

    [Fact]
    public async Task InstallAsync_UnsupportedLoader_ThrowsDescriptiveError()
    {
        var installer = new LoaderInstallerService(new FakeGameLauncherService());

        var exception = await Assert.ThrowsAsync<NotSupportedException>(
            () => installer.InstallAsync("1.21.4", ModLoader.Forge, "47.3.0"));

        Assert.Contains("Forge", exception.Message);
    }
}
