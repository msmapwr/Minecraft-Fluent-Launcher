using WINUI.Models;
using Xunit;

namespace WINUI.Tests.Models;

/// <summary>下载条目的标签与「未知数据」表现测试（大更新 ⑨-1）。</summary>
public sealed class DownloadItemTests
{
    private static DownloadItem MakeItem(double? sizeMb, int? downloadCount) => new()
    {
        Id = "mc-1.21.4",
        Name = "Minecraft 1.21.4",
        Author = "Mojang Studios",
        Category = DownloadCategory.GameVersion,
        Version = "1.21.4",
        SizeMb = sizeMb,
        DownloadCount = downloadCount,
        Description = "正式版",
    };

    [Fact]
    public void SizeLabel_UsesMbAndGb()
    {
        Assert.Equal("312 MB", MakeItem(312, null).SizeLabel);
        Assert.Equal("2.00 GB", MakeItem(2048, null).SizeLabel);
    }

    [Fact]
    public void SizeLabel_ShowsDashWhenUnknown()
    {
        Assert.Equal("—", MakeItem(null, null).SizeLabel);
    }

    [Fact]
    public void DownloadCountLabel_FormatsAndFallsBackToDash()
    {
        Assert.Equal("5200 次下载", MakeItem(null, 5200).DownloadCountLabel);
        Assert.Equal("12.8 万次下载", MakeItem(null, 128_400).DownloadCountLabel);
        Assert.Equal("—", MakeItem(null, null).DownloadCountLabel);
    }

    [Fact]
    public void IsVersion_MatchesGameVersionCategory()
    {
        Assert.True(MakeItem(null, null).IsVersion);
        Assert.False(MakeItem(null, null).IsNotVersion);
    }
}
