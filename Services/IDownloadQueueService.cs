using System;
using System.Collections.Generic;
using WINUI.Models;

namespace WINUI.Services;

/// <summary>
/// 全局下载队列服务：把「安装 / 下载」从页面里剥离出来，
/// 页面离开不会中断任务，重进页面（或从外壳入口）随时看到进度。
/// <para>
/// 事件在后台线程触发，订阅方（视图模型）需自行调度到 UI 线程。
/// </para>
/// </summary>
public interface IDownloadQueueService
{
    /// <summary>任务加入队列时触发（后台线程）。</summary>
    event EventHandler<DownloadTask>? TaskAdded;

    /// <summary>任务状态 / 进度变化时触发（后台线程，可能高频）。</summary>
    event EventHandler<DownloadTask>? TaskChanged;

    /// <summary>任务被移出列表时触发（后台线程）。</summary>
    event EventHandler<DownloadTask>? TaskRemoved;

    /// <summary>当前全部任务的快照（含已完成的）。</summary>
    IReadOnlyList<DownloadTask> Tasks { get; }

    /// <summary>正在推进中的任务数（等待 / 准备 / 下载 / 安装）。</summary>
    int ActiveCount { get; }

    /// <summary>把一个任务加入队列，并确保执行泵在运行。</summary>
    /// <param name="item">来源条目。</param>
    /// <param name="loader">加载器展示名（如 <c>Fabric 0.16.9</c>；只装原版时为 <c>Vanilla</c>）。</param>
    /// <param name="targetGameVersion">目标游戏版本（模组等资源的兼容性预判结果）。</param>
    /// <param name="instanceName">将创建的实例名。</param>
    /// <param name="isDemo">是否为演示任务。</param>
    /// <param name="loaderKind">所选加载器种类；<c>null</c> 表示只装原版。</param>
    /// <param name="loaderVersion">所选加载器版本。</param>
    /// <returns>新入队的任务。</returns>
    DownloadTask Enqueue(
        DownloadItem item,
        string loader,
        string? targetGameVersion,
        string instanceName,
        bool isDemo,
        ModLoader? loaderKind = null,
        string? loaderVersion = null);

    /// <summary>取消一个推进中的任务（已下载的文件会被保留）。</summary>
    void Cancel(string taskId);

    /// <summary>手动重试一个失败的任务（进度与尝试次数清零）。</summary>
    void Retry(string taskId);

    /// <summary>把一个已终结的任务移出列表。</summary>
    void Remove(string taskId);

    /// <summary>清空所有已终结（完成 / 失败 / 取消）的任务。</summary>
    void ClearFinished();
}
