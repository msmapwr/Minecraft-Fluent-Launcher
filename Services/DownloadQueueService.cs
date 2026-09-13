using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WINUI.Models;

namespace WINUI.Services;

/// <inheritdoc cref="IDownloadQueueService" />
/// <summary>
/// 下载队列服务实现：单执行泵串行处理任务，失败自动重试（3 次指数退避），
/// 取消保留已下载文件。
/// <para>
/// 执行器按任务分派：「版本」类真实条目走 <see cref="IGameLauncherService.InstallAsync"/>；
/// 其余（资源站未接入前）与演示任务走内置模拟。真实安装没有进度回调，以
/// 阶段状态 + 不确定进度呈现（符合「隐藏技术细节」的产品原则）。
/// </para>
/// </summary>
public sealed class DownloadQueueService : IDownloadQueueService
{
    /// <summary>自动重试的退避间隔（第 1/2/3 次重试前）。</summary>
    private static readonly TimeSpan[] RetryDelays =
    [
        TimeSpan.FromSeconds(1),
        TimeSpan.FromSeconds(2),
        TimeSpan.FromSeconds(4),
    ];

    private readonly object _gate = new();
    private readonly IGameLauncherService _gameLauncher;
    private readonly TimeSpan[] _retryDelays;

    /// <summary>全部任务（按入队顺序）。</summary>
    private readonly List<DownloadTask> _tasks = [];

    /// <summary>等待执行的任务 id 顺序队列。</summary>
    private readonly Queue<string> _pending = new();

    /// <summary>推进中任务的取消源。</summary>
    private readonly Dictionary<string, CancellationTokenSource> _cancellations = new(StringComparer.Ordinal);

    private readonly SemaphoreSlim _signal = new(0, 1);
    private bool _pumpRunning;

    public DownloadQueueService(IGameLauncherService gameLauncher) : this(gameLauncher, RetryDelays)
    {
    }

    /// <summary>测试用构造：允许注入更短的重试退避间隔。</summary>
    internal DownloadQueueService(IGameLauncherService gameLauncher, TimeSpan[] retryDelays)
    {
        _gameLauncher = gameLauncher;
        _retryDelays = retryDelays;
    }

    /// <inheritdoc />
    public event EventHandler<DownloadTask>? TaskAdded;

    /// <inheritdoc />
    public event EventHandler<DownloadTask>? TaskChanged;

    /// <inheritdoc />
    public event EventHandler<DownloadTask>? TaskRemoved;

    /// <inheritdoc />
    public IReadOnlyList<DownloadTask> Tasks
    {
        get
        {
            lock (_gate)
            {
                return _tasks.ToArray();
            }
        }
    }

    /// <inheritdoc />
    public int ActiveCount
    {
        get
        {
            lock (_gate)
            {
                return _tasks.Count(task => task.Status.IsActive());
            }
        }
    }

    /// <inheritdoc />
    public DownloadTask Enqueue(
        DownloadItem item,
        string loader,
        string? targetGameVersion,
        string instanceName,
        bool isDemo)
    {
        var task = new DownloadTask
        {
            Item = item,
            Loader = loader,
            TargetGameVersion = targetGameVersion,
            InstanceName = instanceName,
            IsDemo = isDemo,
        };

        lock (_gate)
        {
            _tasks.Add(task);
            _pending.Enqueue(task.Id);
        }

        TaskAdded?.Invoke(this, task);
        EnsurePumpRunning();
        return task;
    }

    /// <inheritdoc />
    public void Cancel(string taskId)
    {
        CancellationTokenSource? cts;

        lock (_gate)
        {
            if (!_cancellations.TryGetValue(taskId, out cts))
            {
                return;
            }
        }

        cts.Cancel();
    }

    /// <inheritdoc />
    public void Retry(string taskId)
    {
        DownloadTask? task;
        bool requeue;

        lock (_gate)
        {
            task = _tasks.FirstOrDefault(candidate => candidate.Id == taskId);
            requeue = task is { IsFailed: true };

            if (requeue)
            {
                task!.Attempt = 0;
                task.ErrorMessage = null;
                task.Progress = 0;
                task.ReceivedMb = 0;
                task.SpeedMbps = 0;
                task.Status = DownloadTaskStatus.Waiting;
                _pending.Enqueue(task.Id);
            }
        }

        if (task is null)
        {
            return;
        }

        TaskChanged?.Invoke(this, task);

        if (requeue)
        {
            EnsurePumpRunning();
        }
    }

    /// <inheritdoc />
    public void Remove(string taskId)
    {
        DownloadTask? task;

        lock (_gate)
        {
            task = _tasks.FirstOrDefault(candidate => candidate.Id == taskId);

            if (task is null || !task.IsFinished)
            {
                return;
            }

            _tasks.Remove(task);
        }

        TaskRemoved?.Invoke(this, task);
    }

    /// <inheritdoc />
    public void ClearFinished()
    {
        List<DownloadTask> removed;

        lock (_gate)
        {
            removed = _tasks.Where(task => task.IsFinished).ToList();

            foreach (var task in removed)
            {
                _tasks.Remove(task);
            }
        }

        foreach (var task in removed)
        {
            TaskRemoved?.Invoke(this, task);
        }
    }

    // ==================== 执行泵 ====================

    /// <summary>确保执行泵在运行（幂等）。</summary>
    private void EnsurePumpRunning()
    {
        lock (_gate)
        {
            if (_pumpRunning)
            {
                return;
            }

            _pumpRunning = true;
        }

        // 泵在后台线程推进任务；所有状态更新经 FireChanged 通知订阅方。
        _ = Task.Run(PumpLoopAsync);
    }

    private async Task PumpLoopAsync()
    {
        while (true)
        {
            string? taskId;

            lock (_gate)
            {
                taskId = _pending.Count > 0 ? _pending.Dequeue() : null;

                if (taskId is null)
                {
                    _pumpRunning = false;
                    return;
                }
            }

            var task = Find(taskId);

            if (task is not { Status: DownloadTaskStatus.Waiting })
            {
                continue;
            }

            await ExecuteWithRetryAsync(task);
        }
    }

    private DownloadTask? Find(string taskId)
    {
        lock (_gate)
        {
            return _tasks.FirstOrDefault(candidate => candidate.Id == taskId);
        }
    }

    /// <summary>执行一个任务：失败自动重试（最多 3 次），取消进入 Cancelled。</summary>
    private async Task ExecuteWithRetryAsync(DownloadTask task)
    {
        SetStatus(task, DownloadTaskStatus.Preparing);

        for (var attempt = 0; ; attempt++)
        {
            using var cts = new CancellationTokenSource();

            lock (_gate)
            {
                _cancellations[task.Id] = cts;
            }

            try
            {
                task.Attempt = attempt;
                await ExecuteCoreAsync(task, cts.Token);

                SetStatus(task, DownloadTaskStatus.Completed);
                return;
            }
            catch (OperationCanceledException)
            {
                // 取消：已下载文件保留在原处（CMLLib 的分段文件不回滚）。
                SetStatus(task, DownloadTaskStatus.Cancelled);
                return;
            }
            catch (Exception ex)
            {
                task.ErrorMessage = ex.Message;

                var canRetry = attempt < DownloadTask.MaxRetryCount;

                if (!canRetry)
                {
                    SetStatus(task, DownloadTaskStatus.Failed);
                    return;
                }

                // 指数退避后重试：回到「等待中」并提示重试进度。
                SetStatus(task, DownloadTaskStatus.Waiting, keepNote: true);
                await Task.Delay(_retryDelays[attempt]);
                SetStatus(task, DownloadTaskStatus.Preparing, keepNote: true);
            }
            finally
            {
                lock (_gate)
                {
                    _cancellations.Remove(task.Id);
                }
            }
        }
    }

    /// <summary>按任务类别分派到具体执行器。</summary>
    private async Task ExecuteCoreAsync(DownloadTask task, CancellationToken cancellationToken)
    {
        if (task.Item.Category == DownloadCategory.GameVersion && !task.IsDemo)
        {
            await ExecuteVanillaInstallAsync(task, cancellationToken);
        }
        else
        {
            await ExecuteDemoAsync(task, cancellationToken);
        }
    }

    /// <summary>真实安装：阶段式推进，进度以不确定形式呈现。</summary>
    private async Task ExecuteVanillaInstallAsync(DownloadTask task, CancellationToken cancellationToken)
    {
        Update(task, status: DownloadTaskStatus.Downloading, indeterminate: true);
        await _gameLauncher.InstallAsync(task.Item.Version, cancellationToken);

        Update(task, status: DownloadTaskStatus.Installing, progress: 100, indeterminate: false);
        await Task.Delay(300, cancellationToken);
    }

    /// <summary>演示执行：模拟速度与进度，不产生网络请求、不写磁盘。</summary>
    private async Task ExecuteDemoAsync(DownloadTask task, CancellationToken cancellationToken)
    {
        Update(task, status: DownloadTaskStatus.Downloading);

        var total = Math.Max(task.TotalMb, 1);
        var received = task.ReceivedMb;

        while (received < total)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // 模拟 3–9 MB/s 的波动速度，每 120ms 推进一次。
            var speed = 3 + Random.Shared.NextDouble() * 6;
            received = Math.Min(total, received + speed * 0.12);

            Update(
                task,
                status: DownloadTaskStatus.Downloading,
                progress: received / total * 100,
                receivedMb: received,
                speedMbps: speed);

            await Task.Delay(120, cancellationToken);
        }

        Update(task, status: DownloadTaskStatus.Installing, progress: 100, receivedMb: total, speedMbps: 0);
        await Task.Delay(400, cancellationToken);
    }

    /// <summary>更新任务字段并广播变更。可在任意线程调用。</summary>
    private void Update(
        DownloadTask task,
        DownloadTaskStatus? status = null,
        double? progress = null,
        bool? indeterminate = null,
        double? receivedMb = null,
        double? speedMbps = null)
    {
        if (status is { } statusValue)
        {
            task.Status = statusValue;
        }

        if (progress is { } progressValue)
        {
            task.Progress = progressValue;
        }

        task.IsIndeterminate = indeterminate ?? task.IsIndeterminate;

        if (receivedMb is { } receivedValue)
        {
            task.ReceivedMb = receivedValue;
        }

        if (speedMbps is { } speedValue)
        {
            task.SpeedMbps = speedValue;
        }

        // 进度更新同样要广播，否则界面上的进度 / 速度 / 剩余不会刷新。
        TaskChanged?.Invoke(this, task);
    }

    /// <summary>设置状态并广播（可选保留重试提示）。</summary>
    private void SetStatus(DownloadTask task, DownloadTaskStatus status, bool keepNote = false)
    {
        task.Status = status;

        if (!keepNote)
        {
            task.IsIndeterminate = status is DownloadTaskStatus.Downloading or DownloadTaskStatus.Installing
                && task.Item.Category == DownloadCategory.GameVersion
                && !task.IsDemo;
        }

        TaskChanged?.Invoke(this, task);
    }
}
