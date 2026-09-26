using System;
using WINUI.Models;
using WINUI.Services;
using WINUI.Tests.Helpers;
using WINUI.Tests.Services;
using WINUI.ViewModels;
using Xunit;

namespace WINUI.Tests.ViewModels;

/// <summary>
/// 下载队列页视图模型测试。
/// <para>
/// 重点回归 v0.2.3 的实测缺陷：队列页<b>不存在加载阶段</b>，
/// 若沿用基类的「Loading 态不可被内容态覆盖」行为，页面会永远停在「正在加载」。
/// </para>
/// </summary>
public sealed class DownloadQueuePageViewModelTests
{
    private static DownloadQueuePageViewModel CreateViewModel(out DownloadQueueService queue)
    {
        queue = new DownloadQueueService(new FakeGameLauncherService());

        var animation = new AnimationService(new FakeSettingsService());
        var navigation = new NavigationService(animation);

        return new DownloadQueuePageViewModel(
            queue,
            new FakeGameLauncherService(),
            navigation,
            new FakeInteractionService());
    }

    private static DownloadItem MakeItem(double sizeMb = 500) => new()
    {
        Id = "mc-1.21.4",
        Name = "Minecraft 1.21.4",
        Author = "Mojang Studios",
        Category = DownloadCategory.GameVersion,
        Version = "1.21.4",
        SizeMb = sizeMb,
        DownloadCount = null,
        Description = "正式版",
    };

    [Fact]
    public void Constructor_WithoutTasks_ShowsEmptyInsteadOfLoading()
    {
        var vm = CreateViewModel(out _);

        // 回归断言：绝不能是 Loading（否则页面永远显示「正在加载」）。
        Assert.NotEqual(PageState.Loading, vm.State);
        Assert.Equal(PageState.Empty, vm.State);
        Assert.False(vm.HasTasks);
    }

    [Fact]
    public void Enqueue_Task_SwitchesToContentState()
    {
        var vm = CreateViewModel(out var queue);

        queue.Enqueue(MakeItem(), "Vanilla", "1.21.4", "1.21.4 · 原版", isDemo: true);

        Assert.Equal(PageState.Content, vm.State);
        Assert.True(vm.HasTasks);
        Assert.Single(vm.Tasks);
    }

    [Fact]
    public void EmptyStateTexts_AreQueueSpecific()
    {
        var vm = CreateViewModel(out _);

        Assert.Equal("暂无下载任务", vm.EmptyTitle);
        Assert.Contains("下载中心", vm.EmptyText);
    }
}
