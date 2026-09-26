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
/// 不依赖网络：只验证能力判定、profile 解析与构造契约。
/// 真实下载与 installer 执行链路由用户实测覆盖。
/// </para>
/// </summary>
public sealed class LoaderServiceTests
{
    private static LoaderInstallerService CreateInstaller(FakeGameLauncherService gameLauncher)
        => new(gameLauncher, new JavaLocatorService(), new FakeSettingsService());

    [Theory]
    [InlineData(ModLoader.Fabric)]
    [InlineData(ModLoader.Quilt)]
    [InlineData(ModLoader.Forge)]
    [InlineData(ModLoader.NeoForge)]
    public void CanInstall_SupportsAllFourLoaders(ModLoader loader)
    {
        var catalog = new LoaderCatalogService();
        var installer = CreateInstaller(new FakeGameLauncherService());

        Assert.True(catalog.CanInstall(loader));
        Assert.True(installer.CanInstall(loader));
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
    public void BuildInstallerUrl_FollowsOfficialMavenPaths()
    {
        var forge = LoaderInstallerService.BuildInstallerUrl("1.20.1", ModLoader.Forge, "47.3.0");
        Assert.EndsWith("maven/net/minecraftforge/forge/1.20.1-47.3.0/forge-1.20.1-47.3.0-installer.jar", forge);

        var neoForge = LoaderInstallerService.BuildInstallerUrl("1.21.4", ModLoader.NeoForge, "21.4.158");
        Assert.EndsWith("neoforge/21.4.158/neoforge-21.4.158-installer.jar", neoForge);
    }
}
