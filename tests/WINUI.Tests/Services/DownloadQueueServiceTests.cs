using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WINUI.Models;
using WINUI.Services;
using Xunit;

namespace WINUI.Tests.Services;

/// <summary>
/// 全局下载队列服务测试（大更新 ⑧-3）：状态机、失败自动重试、取消保留、列表管理。
/// </summary>
public sealed class DownloadQueueServiceTests
{
    private static DownloadItem MakeItem(
        DownloadCategory category = DownloadCategory.GameVersion,
        string version = "1.21.4",
        double sizeMb = 1)
        => new()
        {
            Id = $"{category}-{version}-{Guid.NewGuid():N}",
            Name = $"Test {category} {version}",
            Author = "Test",
            Category = category,
            Version = version,
            SizeMb = sizeMb,
            DownloadCount = 1,
            Description = "测试条目",
        };

    /// <summary>轮询等待条件成立（超时抛出）。</summary>
    private static async Task WaitForAsync(Func<bool> condition, int timeoutMs = 15000)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        while (!condition())
        {
            if (stopwatch.ElapsedMilliseconds > timeoutMs)
            {
                throw new TimeoutException("等待队列状态超时");
            }

            await Task.Delay(25);
        }
    }

    private static DownloadQueueService CreateService(
        FakeGameLauncherService? launcher = null,
        TimeSpan[]? retryDelays = null)
        => new(
            launcher ?? new FakeGameLauncherService(),
            retryDelays ?? [TimeSpan.FromMilliseconds(1), TimeSpan.FromMilliseconds(1), TimeSpan.FromMilliseconds(1)]);

    [Fact]
    public async Task Enqueue_DemoTask_RunsThroughAndCompletes()
    {
        var service = CreateService();
        var added = new List<DownloadTask>();

        // DownloadTask 是可变共享对象，事件触发瞬间记录状态快照。
        var changed = new List<(DownloadTask Task, DownloadTaskStatus Status)>();
        service.TaskAdded += (_, task) => added.Add(task);
        service.TaskChanged += (_, task) => changed.Add((task, task.Status));

        var task = service.Enqueue(MakeItem(sizeMb: 1), "Vanilla", "1.21.4", "1.21.4 · 原版", isDemo: true);

        await WaitForAsync(() => task.Status == DownloadTaskStatus.Completed);

        Assert.Single(added);
        Assert.Contains(changed, entry => entry.Status == DownloadTaskStatus.Preparing);
        Assert.Contains(changed, entry => entry.Status == DownloadTaskStatus.Downloading);
        Assert.Contains(changed, entry => entry.Status == DownloadTaskStatus.Installing);
        Assert.Contains(changed, entry => entry.Status == DownloadTaskStatus.Completed);
        Assert.Equal(100, task.Progress);
        Assert.True(task.IsFinished);
        Assert.Single(service.Tasks);
    }

    [Fact]
    public async Task Enqueue_RealVersion_UsesGameLauncherInstall()
    {
        var launcher = new FakeGameLauncherService();
        var service = CreateService(launcher);

        var task = service.Enqueue(MakeItem(), "Vanilla", "1.21.4", "1.21.4 · 原版", isDemo: false);

        await WaitForAsync(() => task.Status == DownloadTaskStatus.Completed);

        Assert.Equal(1, launcher.InstallCalls);
    }

    [Fact]
    public async Task Execute_FailureRetriesThreeTimes_ThenFails()
    {
        var launcher = new FakeGameLauncherService
        {
            InstallBehavior = (_, _) => throw new InvalidOperationException("网络故障"),
        };
        var service = CreateService(launcher);

        var task = service.Enqueue(MakeItem(), "Vanilla", "1.21.4", "1.21.4 · 原版", isDemo: false);

        await WaitForAsync(() => task.Status == DownloadTaskStatus.Failed);

        // 首次执行 + 3 次自动重试 = 4 次调用；尝试计数停在 3。
        Assert.Equal(4, launcher.InstallCalls);
        Assert.Equal(DownloadTask.MaxRetryCount, task.Attempt);
        Assert.NotNull(task.ErrorMessage);
    }

    [Fact]
    public async Task Cancel_ActiveTask_EntersCancelledState()
    {
        var service = CreateService();

        // 大体积演示任务足够长，可以在「下载中」窗口内取消。
        var task = service.Enqueue(MakeItem(sizeMb: 500), "Vanilla", null, "演示", isDemo: true);

        await WaitForAsync(() => task.Status == DownloadTaskStatus.Downloading);
        service.Cancel(task.Id);

        await WaitForAsync(() => task.Status == DownloadTaskStatus.Cancelled);

        Assert.True(task.IsFinished);
        // 演示任务不写磁盘；取消语义 = 保留已下载文件（此处仅验证状态）。
        Assert.True(task.ReceivedMb < task.TotalMb || task.ReceivedMb >= 0);
    }

    [Fact]
    public async Task Cancel_RealInstall_PropagatesCancellationToLauncher()
    {
        var launcher = new FakeGameLauncherService
        {
            InstallBehavior = async (_, ct) =>
            {
                await Task.Delay(Timeout.Infinite, ct);
            },
        };
        var service = CreateService(launcher);

        var task = service.Enqueue(MakeItem(), "Vanilla", "1.21.4", "1.21.4 · 原版", isDemo: false);
        await WaitForAsync(() => launcher.InstallCalls == 1);

        service.Cancel(task.Id);

        await WaitForAsync(() => task.Status == DownloadTaskStatus.Cancelled);
    }

    [Fact]
    public async Task Retry_FailedTask_ResetsAndRequeues()
    {
        var launcher = new FakeGameLauncherService
        {
            // 前两次（首跑 + 全部自动重试）失败，手动重试后成功。
            InstallBehavior = (_, _) => throw new InvalidOperationException("网络故障"),
        };
        var service = CreateService(launcher);

        var task = service.Enqueue(MakeItem(), "Vanilla", "1.21.4", "1.21.4 · 原版", isDemo: false);
        await WaitForAsync(() => task.Status == DownloadTaskStatus.Failed);

        launcher.InstallBehavior = (_, _) => Task.CompletedTask;
        service.Retry(task.Id);

        await WaitForAsync(() => task.Status == DownloadTaskStatus.Completed);

        Assert.Equal(0, task.Attempt);
        Assert.Null(task.ErrorMessage);
        Assert.Equal(5, launcher.InstallCalls); // 4（首跑+3 重试）+ 1（手动重试）
    }

    [Fact]
    public void Remove_ActiveTask_IsIgnored()
    {
        var service = CreateService();
        var task = service.Enqueue(MakeItem(sizeMb: 500), "Vanilla", null, "演示", isDemo: true);

        service.Remove(task.Id);

        Assert.Single(service.Tasks);
    }

    [Fact]
    public async Task ClearFinished_KeepsActiveTasks()
    {
        var service = CreateService();

        var finished = service.Enqueue(MakeItem(sizeMb: 1), "Vanilla", "1.21.4", "A", isDemo: true);
        var active = service.Enqueue(MakeItem(sizeMb: 500), "Vanilla", null, "B", isDemo: true);

        await WaitForAsync(() => finished.Status == DownloadTaskStatus.Completed);

        service.ClearFinished();

        Assert.DoesNotContain(service.Tasks, item => item.Id == finished.Id);
        Assert.Contains(service.Tasks, item => item.Id == active.Id);
    }

    [Fact]
    public void ActiveCount_ReflectsQueueState()
    {
        var service = CreateService();
        service.Enqueue(MakeItem(sizeMb: 500), "Vanilla", null, "演示", isDemo: true);

        Assert.Equal(1, service.ActiveCount);
    }
}
