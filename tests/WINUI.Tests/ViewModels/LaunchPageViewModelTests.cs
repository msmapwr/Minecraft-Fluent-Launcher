using WINUI.ViewModels;
using Xunit;

namespace WINUI.Tests.ViewModels;

/// <summary>启动页的 Java 需求判定测试（大更新 ⑨-3）。</summary>
public sealed class LaunchPageViewModelTests
{
    [Theory]
    [InlineData("1.21.4", 21)]
    [InlineData("1.21", 21)]
    [InlineData("1.20.5", 21)]
    [InlineData("1.20.4", 17)]
    [InlineData("1.20.1", 17)]
    [InlineData("1.18.2", 17)]
    [InlineData("1.17.1", 16)]
    [InlineData("1.16.5", 8)]
    [InlineData("1.12.2", 8)]
    [InlineData("1.7.10", 8)]
    public void RequiredJavaMajor_MapsVersionToJavaRequirement(string versionId, int expected)
    {
        Assert.Equal(expected, LaunchPageViewModel.RequiredJavaMajor(versionId));
    }

    [Theory]
    [InlineData("24w14a")]      // 快照
    [InlineData("a1.2.6")]      // 远古 Alpha
    [InlineData("b1.7.3")]      // 远古 Beta
    [InlineData("custom-modpack-v2")] // 自定义 / 加载器版本
    [InlineData("")]
    public void RequiredJavaMajor_ReturnsNullWhenNotVanillaRelease(string versionId)
    {
        Assert.Null(LaunchPageViewModel.RequiredJavaMajor(versionId));
    }
}
