using WINUI.Services;
using Xunit;

namespace WINUI.Tests.Services;

/// <summary>Java 版本识别测试（大更新 ⑨-3）。</summary>
public sealed class JavaLocatorServiceTests
{
    [Theory]
    [InlineData("openjdk version \"21.0.3\" 2024-01-16", 21)]
    [InlineData("java version \"1.8.0_401\"", 8)]
    [InlineData("java version \"17.0.10\" 2024-01-16 LTS", 17)]
    [InlineData("openjdk version \"25\" 2025-09-16", 25)]
    [InlineData("java version \"1.7.0_80\"", 7)]
    public void ParseMajorVersion_ReadsCommonFormats(string output, int expected)
    {
        Assert.Equal(expected, JavaLocatorService.ParseMajorVersion(output));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("no quotes here")]
    [InlineData("version \"abc\"")]
    public void ParseMajorVersion_ReturnsNullWhenUnparsable(string output)
    {
        Assert.Null(JavaLocatorService.ParseMajorVersion(output));
    }

    [Fact]
    public void GetMajorVersion_ReturnsNullForMissingPath()
    {
        var locator = new JavaLocatorService();

        Assert.Null(locator.GetMajorVersion(string.Empty));
        Assert.Null(locator.GetMajorVersion(@"Z:\not-exists\javaw.exe"));
    }
}
