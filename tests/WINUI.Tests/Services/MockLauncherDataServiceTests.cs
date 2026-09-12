using System.Threading.Tasks;
using WINUI.Models;
using WINUI.Services;
using Xunit;

namespace WINUI.Tests.Services;

/// <summary>
/// Mock 数据服务的行为测试：数据规模与「加载器可用性规则」。
/// </summary>
public sealed class MockLauncherDataServiceTests
{
    private readonly MockLauncherDataService _service = new();

    [Fact]
    public async Task GetVersionsAsync_ReturnsNonEmpty()
    {
        var versions = await _service.GetVersionsAsync();

        Assert.NotEmpty(versions);
        Assert.Contains(versions, version => version.Id == "1.21.4");
    }

    [Fact]
    public async Task GetDownloadItemsAsync_ReturnsPagedDataScale()
    {
        var items = await _service.GetDownloadItemsAsync();

        // Mock 条目约 37 条（支撑分页演示）；放宽为 ≥30 防止演示数据微调导致误报。
        Assert.True(items.Count >= 30, $"Mock 下载条目仅 {items.Count} 条，不足以支撑分页演示");
        Assert.All(items, item => Assert.False(string.IsNullOrWhiteSpace(item.Id)));
    }

    [Fact]
    public async Task GetLoadersAsync_LatestRelease_GetsFabricNeoForgeQuiltButNotForge()
    {
        // Forge 仅面向 1.20.x 及以下；1.21.x 不应出现。
        var loaders = await _service.GetLoadersAsync("1.21.4");

        Assert.Contains(loaders, entry => entry.Loader == ModLoader.Fabric);
        Assert.Contains(loaders, entry => entry.Loader == ModLoader.NeoForge);
        Assert.Contains(loaders, entry => entry.Loader == ModLoader.Quilt);
        Assert.DoesNotContain(loaders, entry => entry.Loader == ModLoader.Forge);
    }

    [Fact]
    public async Task GetLoadersAsync_1201_GetsAllFourLoaders()
    {
        var loaders = await _service.GetLoadersAsync("1.20.1");

        Assert.Contains(loaders, entry => entry.Loader == ModLoader.Fabric);
        Assert.Contains(loaders, entry => entry.Loader == ModLoader.NeoForge);
        Assert.Contains(loaders, entry => entry.Loader == ModLoader.Forge);
        Assert.Contains(loaders, entry => entry.Loader == ModLoader.Quilt);
    }

    [Fact]
    public async Task GetLoadersAsync_OldVersion_NoNeoForge()
    {
        var loaders = await _service.GetLoadersAsync("1.12.2");

        Assert.Contains(loaders, entry => entry.Loader == ModLoader.Forge);
        Assert.DoesNotContain(loaders, entry => entry.Loader == ModLoader.NeoForge);
    }

    [Theory]
    [InlineData("24w14a")] // 快照
    [InlineData("a1.2.5")] // 远古版本
    [InlineData("not-a-version")]
    [InlineData("")]
    public async Task GetLoadersAsync_UnparseableVersion_ReturnsEmpty(string gameVersion)
    {
        var loaders = await _service.GetLoadersAsync(gameVersion);

        Assert.Empty(loaders);
    }

    [Fact]
    public async Task GetLoadersAsync_EveryEntry_ExposesValidVersions()
    {
        foreach (var version in new[] { "1.21.4", "1.20.1", "1.12.2" })
        {
            var loaders = await _service.GetLoadersAsync(version);

            Assert.All(loaders, entry =>
            {
                Assert.NotEmpty(entry.Versions);
                Assert.Contains(entry.RecommendedVersion, entry.Versions);
            });
        }
    }

    [Fact]
    public async Task GetAllQueries_CompleteQuickly_WithDelayDisabled()
    {
        // 模块初始化已把 MFL_MOCK_DELAY 置 0：验证测试环境下 Mock 确实零延迟。
        var versions = await _service.GetVersionsAsync();
        var instances = await _service.GetInstancesAsync();
        var mods = await _service.GetModsAsync("instance-1");

        Assert.NotEmpty(versions);
        Assert.NotEmpty(instances);
        Assert.NotEmpty(mods);
    }
}
