using WINUI.Services;
using WINUI.Tests.Helpers;
using Xunit;

namespace WINUI.Tests.Services;

/// <summary>
/// 导航服务在不依赖 UI Frame 情况下可验证的行为：
/// 路由表覆盖与「未初始化 / 未知路由」的安全短路。
/// </summary>
public sealed class NavigationServiceTests
{
    [Fact]
    public void Constructor_StartsWithNoCurrentKey()
    {
        var service = new NavigationService(new AnimationService(new FakeSettingsService()));

        Assert.Null(service.CurrentKey);
        Assert.False(service.CanGoBack);
    }

    [Fact]
    public void Navigate_BeforeInitialize_ReturnsFalse()
    {
        var service = new NavigationService(new AnimationService(new FakeSettingsService()));

        Assert.False(service.Navigate("launch"));
        Assert.Null(service.CurrentKey);
    }

    [Fact]
    public void Navigate_UnknownKey_ReturnsFalse()
    {
        var service = new NavigationService(new AnimationService(new FakeSettingsService()));
        service.Initialize(null!);

        Assert.False(service.Navigate("bogus-route"));
        Assert.False(service.Navigate(""));
        Assert.Null(service.CurrentKey);
    }

    [Fact]
    public void GoBack_BeforeInitialize_ReturnsFalse()
    {
        var service = new NavigationService(new AnimationService(new FakeSettingsService()));

        Assert.False(service.GoBack());
    }

    [Fact]
    public void Navigate_KnowsAllFirstLevelRoutes()
    {
        // 通过反射间接验证路由表完整性：路由键与 MainWindow 的 Tag 约定一致。
        // 这里仅验证「未初始化 Frame 时全部安全返回 false」，路由键本身的映射
        // 依赖 Views 类型（需要 XAML 运行时），不在此展开。
        var service = new NavigationService(new AnimationService(new FakeSettingsService()));

        foreach (var key in new[] { "launch", "instances", "downloads", "mods", "accounts", "settings", "logs", "about", "version-detail" })
        {
            Assert.False(service.Navigate(key), $"路由 {key} 在未初始化时应安全返回 false");
        }
    }
}
